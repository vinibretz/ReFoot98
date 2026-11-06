using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using AVisual = Avalonia.Visual;
using RetroFoot.App;
using Xunit;
using static RetroFoot.App.Idioma;

namespace RetroFoot.App.Testes;

// Configuracoes > Regras em cada visual (os grupos, as quatro opcoes, a versao Retro travada, o
// resumo) e o config.json (gravar e ler, e o de antes das opcoes separadas)
public class ConfiguracoesTestes
{
    // as quatro opcoes: o nome na tela e o valor nas Configuracoes
    static readonly (string Nome, Func<Configuracoes, bool> Valor)[] Quatro =
    {
        ("Cartões amarelos", c => c.CartoesAmarelos),
        ("Nota e risco de lesão", c => c.NotaERisco),
        ("Preço do ingresso", c => c.PrecoIngresso),
        ("Campinho e árbitro", c => c.CampinhoArbitro),
    };

    // o controle de marcar de uma opcao, em qualquer visual: o interruptor (Moderno, celular
    // Moderno), a Marca desenhada (Classico) ou o Marcador (celular Retro)
    sealed record Interruptor(bool Habilitado, bool Ligado, Action Alternar);

    static Interruptor? Achar(AVisual raiz, string nome)
    {
        var todos = raiz.GetVisualDescendants().ToList();
        if (todos.OfType<Marca>().FirstOrDefault(m => m.Texto == nome) is { } marca)
            return new(marca.Habilitado, marca.Marcado, () =>
            {
                // (o clique de verdade: OnPointerReleased troca o Marcado e avisa)
                if (!marca.Habilitado) return;
                marca.Marcado = !marca.Marcado;
                var ev = typeof(Marca).GetField("Clique", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(marca) as Action;
                ev?.Invoke();
            });
        if (todos.OfType<CR95.Marcador>().FirstOrDefault(m => m.Texto.Text == nome) is { } mc)
            return new(mc.IsEffectivelyEnabled, mc.Marcado, () => { if (mc.IsEffectivelyEnabled) mc.Clicar(); });
        // o interruptor mais perto do rotulo
        var rot = todos.OfType<TextBlock>().FirstOrDefault(t => t.Text == nome);
        for (AVisual? v = rot; v != null; v = v.GetVisualParent())
            if (v.GetVisualDescendants().OfType<ToggleSwitch>().ToList() is { Count: 1 } l)
            {
                var sw = l[0];
                return new(sw.IsEffectivelyEnabled, sw.IsChecked == true, () => { if (sw.IsEffectivelyEnabled) sw.IsChecked = sw.IsChecked != true; });
            }
        return null;
    }

    [Theory]
    [MemberData(nameof(Perfil.Nomes), MemberType = typeof(Perfil))]
    public void GruposEQuatroOpcoes(string perfil)
    {
        var p = Perfil.De(perfil);
        var erros = new List<string>();
        Ui.Rodar(() =>
        {
            using var c = new Cena(p, Opcoes4.Tudo.Cfg());
            c.Configuracoes();
            var t = c.Textos();
            bool Grupo(string g) => t.Any(x => string.Equals(x.Valor, T(g), StringComparison.CurrentCultureIgnoreCase));
            // os grupos pelo que afetam: no Classico (Win95) dois, Partida e Clube
            var quero = p.Moderno || p.NoCelular
                ? new[] { "Partida", "Jogadores", "Estádio e dinheiro", "Tela da equipa" }
                : new[] { "Partida", "Clube" };
            foreach (var g in quero) if (!Grupo(g)) erros.Add($"falta o grupo {T(g)}");
            if (!p.Moderno && !p.NoCelular && Grupo("Estádio e dinheiro")) erros.Add("o Clássico tem o grupo Estádio e dinheiro");
            foreach (var (nome, _) in Quatro)
                if (Achar(c.Janela, T(nome)) == null) erros.Add($"falta a opção {T(nome)}");
        });
        Assert.True(erros.Count == 0, $"{p}: " + string.Join("; ", erros));
    }

    // ligadas e habilitadas na versao Atual; um toque muda so a opcao dela
    [Theory]
    [MemberData(nameof(Perfil.Nomes), MemberType = typeof(Perfil))]
    public void CadaInterruptorMudaASuaOpcao(string perfil)
    {
        var p = Perfil.De(perfil);
        var erros = new List<string>();
        Ui.Rodar(() =>
        {
            foreach (var (nome, valor) in Quatro)
            {
                using var c = new Cena(p, Opcoes4.Tudo.Cfg());
                var cfg = c.Configuracoes();
                c.Textos();
                var i = Achar(c.Janela, T(nome));
                if (i == null) { erros.Add($"{T(nome)}: não achei o controle"); continue; }
                if (!i.Habilitado) erros.Add($"{T(nome)}: travado na versão Atual");
                if (!i.Ligado) erros.Add($"{T(nome)}: desligado com a opção ligada");
                i.Alternar();
                if (valor(cfg)) erros.Add($"{T(nome)}: o toque não desligou a opção");
                foreach (var (outro, v2) in Quatro.Where(x => x.Nome != nome))
                    if (!v2(cfg)) erros.Add($"{T(nome)}: o toque desligou também {T(outro)}");
            }
        });
        Assert.True(erros.Count == 0, $"{p}: " + string.Join("; ", erros));
    }

    // na versao Retro: o aviso vermelho, as quatro travadas e desligadas na tela, e a escolha
    // guardada nao muda
    [Theory]
    [MemberData(nameof(Perfil.Nomes), MemberType = typeof(Perfil))]
    public void RetroTravaAsOpcoes(string perfil)
    {
        var p = Perfil.De(perfil);
        var erros = new List<string>();
        Ui.Rodar(() =>
        {
            var retro = Opcoes4.Tudo.Cfg();
            retro.Modificada = false;
            using var c = new Cena(p, retro);
            var cfg = c.Configuracoes();
            var t = c.Textos();
            if (!t.Any(x => x.Visivel && x.Valor.StartsWith(T("Na versão Retrô as regras são as de 1998").Split(':')[0], StringComparison.Ordinal)))
                erros.Add("sem o aviso da versão Retrô");
            foreach (var (nome, valor) in Quatro)
            {
                var i = Achar(c.Janela, T(nome));
                if (i == null) { erros.Add($"{T(nome)}: não achei o controle"); continue; }
                if (i.Habilitado) erros.Add($"{T(nome)}: habilitado na Retrô");
                if (i.Ligado) erros.Add($"{T(nome)}: aparece ligado na Retrô");
                i.Alternar();
                if (!valor(cfg)) erros.Add($"{T(nome)}: a escolha guardada mudou na Retrô");
            }
        });
        Assert.True(erros.Count == 0, $"{p}: " + string.Join("; ", erros));
    }

    // e na versao Atual o aviso nao aparece
    [Theory]
    [MemberData(nameof(Perfil.Nomes), MemberType = typeof(Perfil))]
    public void AtualSemOAvisoDaRetro(string perfil)
    {
        var p = Perfil.De(perfil);
        bool aviso = false;
        Ui.Rodar(() =>
        {
            using var c = new Cena(p, Opcoes4.Tudo.Cfg());
            c.Configuracoes();
            aviso = c.Textos().Any(x => x.Visivel && x.Valor.StartsWith(T("Na versão Retrô as regras são as de 1998").Split(':')[0], StringComparison.Ordinal));
        });
        Assert.False(aviso, $"{p}: o aviso da Retrô aparece na versão Atual");
    }

    // o resumo ao lado (Moderno 16:9): o ingresso escolhido ou de tabela, o campinho ligado ou nao
    [Fact]
    public void ResumoDoModerno169()
    {
        var erros = new List<string>();
        Ui.Rodar(() =>
        {
            foreach (var o in Opcoes4.Combinacoes())
            {
                using var c = new Cena(Perfil.Moderno169, o.Cfg());
                c.Configuracoes();
                var t = c.Textos();
                bool Ha(string s) => t.Any(x => x.Visivel && x.Valor == T(s));
                if (Ha("Escolhido") != o.Ingresso) erros.Add($"{o}: Ingresso Escolhido");
                if (Ha("Preço de tabela") == o.Ingresso) erros.Add($"{o}: Ingresso Preço de tabela");
                if (!Ha("Campinho")) erros.Add($"{o}: sem a linha Campinho");
                if (!Ha("Nota e risco")) erros.Add($"{o}: sem a linha Nota e risco");
                if (Ha("Amarelos e vermelhos") != o.Amarelos) erros.Add($"{o}: Cartões");
            }
        });
        Assert.True(erros.Count == 0, string.Join("; ", erros));
    }

    // ------------------------------------------------------------------ o config.json
    static string Temporario() => Path.Combine(Path.GetTempPath(), "retrofoot-cfg-" + Guid.NewGuid().ToString("N") + ".json");

    // as 16 combinacoes das quatro opcoes voltam iguais
    [Fact]
    public void GravarELerAsQuatro()
    {
        for (int b = 0; b < 16; b++)
        {
            var o = new Opcoes4((b & 1) != 0, (b & 2) != 0, (b & 4) != 0, (b & 8) != 0);
            var arq = Temporario();
            try
            {
                o.Cfg().Gravar(arq);
                var lida = Configuracoes.Ler(arq);
                Assert.Equal(o, new Opcoes4(lida.CartoesAmarelos, lida.NotaERisco, lida.PrecoIngresso, lida.CampinhoArbitro));
                Assert.True(lida.Modificada);
            }
            finally { File.Delete(arq); }
        }
    }

    // um config.json de antes das opcoes separadas (so "cartoesAmarelos"): as tres novas herdam
    // o valor dele; uma chave nova gravada vale por si
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConfigAntigaHerdaOsAmarelos(bool amarelos)
    {
        var arq = Temporario();
        try
        {
            File.WriteAllText(arq, $$"""{ "versao": "modificada", "cartoesAmarelos": {{(amarelos ? "true" : "false")}} }""");
            var c = Configuracoes.Ler(arq);
            Assert.Equal(amarelos, c.CartoesAmarelos);
            Assert.Equal(amarelos, c.NotaERisco);
            Assert.Equal(amarelos, c.PrecoIngresso);
            Assert.Equal(amarelos, c.CampinhoArbitro);
            File.WriteAllText(arq, $$"""{ "versao": "modificada", "cartoesAmarelos": {{(amarelos ? "true" : "false")}}, "precoIngresso": {{(!amarelos ? "true" : "false")}} }""");
            c = Configuracoes.Ler(arq);
            Assert.Equal(!amarelos, c.PrecoIngresso);
            Assert.Equal(amarelos, c.NotaERisco);
        }
        finally { File.Delete(arq); }
    }

    // as regras do jogo seguem as quatro opcoes (Configuracoes.RegrasPara)
    [Fact]
    public void RegrasSeguemAsOpcoes()
    {
        foreach (var o in Opcoes4.Combinacoes())
        {
            var r = o.Cfg().RegrasPara(8);
            Assert.Equal(o, new Opcoes4(r.CartoesAmarelos, r.NotaERisco, r.PrecoIngresso, r.CampinhoArbitro));
        }
        // a Retro: tudo desligado, mesmo com a escolha guardada
        var retro = Opcoes4.Tudo.Cfg();
        retro.Modificada = false;
        var rr = retro.RegrasPara(8);
        Assert.False(rr.CartoesAmarelos || rr.NotaERisco || rr.PrecoIngresso || rr.CampinhoArbitro);
    }
}
