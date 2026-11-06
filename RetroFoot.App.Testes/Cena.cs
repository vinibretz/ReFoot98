using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using RetroFoot.App;
using RetroFoot.Motor.Partida;
using RetroFoot.Motor.Save;

namespace RetroFoot.App.Testes;

// um visual em que as telas sao conferidas
public sealed record Perfil(string Nome, bool Moderno, TemaCor Tema, bool Largo, bool NoCelular)
{
    public double Largura => NoCelular ? 420 : Largo ? 1280 : 1024;
    public double Altura => NoCelular ? 860 : Largo ? 720 : 768;

    public void Aplicar()
    {
        Celular.Ativo = NoCelular;
        Visual.Aplicar(Moderno, Tema);
        if (!NoCelular) Formato.Aplicar(Largo);
    }

    public override string ToString() => Nome;

    public static readonly Perfil Classico43 = new("Clássico 4:3", false, TemaCor.Verde, false, false);
    public static readonly Perfil Classico169 = new("Clássico 16:9", false, TemaCor.Verde, true, false);
    public static readonly Perfil Moderno43 = new("Moderno 4:3 verde", true, TemaCor.Verde, false, false);
    public static readonly Perfil Moderno169 = new("Moderno 16:9 verde", true, TemaCor.Verde, true, false);
    public static readonly Perfil Moderno169Escuro = new("Moderno 16:9 escuro", true, TemaCor.Escuro, true, false);
    public static readonly Perfil Moderno43Claro = new("Moderno 4:3 claro", true, TemaCor.Claro, false, false);
    public static readonly Perfil CelularModerno = new("Celular Moderno", true, TemaCor.Verde, false, true);
    public static readonly Perfil CelularRetro = new("Celular Retrô", false, TemaCor.Verde, false, true);

    public static readonly Perfil[] Todos = { Classico43, Classico169, Moderno43, Moderno169, Moderno169Escuro, Moderno43Claro, CelularModerno, CelularRetro };

    // (para os [Theory]: o nome do perfil)
    public static IEnumerable<object[]> Nomes() => Todos.Select(p => new object[] { p.Nome });
    public static Perfil De(string nome) => Todos.Single(p => p.Nome == nome);
}

// as quatro opcoes da versao Atual que vinham com os amarelos
public sealed record Opcoes4(bool Amarelos, bool Nota, bool Ingresso, bool Campinho)
{
    public static readonly Opcoes4 Tudo = new(true, true, true, true);
    public static readonly Opcoes4 Nada = new(false, false, false, false);

    public Configuracoes Cfg() => new()
    {
        Modificada = true, CartoesAmarelos = Amarelos, NotaERisco = Nota, PrecoIngresso = Ingresso, CampinhoArbitro = Campinho,
    };

    public override string ToString() => $"A{(Amarelos ? 1 : 0)}N{(Nota ? 1 : 0)}I{(Ingresso ? 1 : 0)}C{(Campinho ? 1 : 0)}";

    // cada uma desligada sozinha, todas ligadas e todas desligadas
    public static IEnumerable<Opcoes4> Combinacoes()
    {
        yield return Tudo; yield return Nada;
        yield return Tudo with { Amarelos = false }; yield return Tudo with { Nota = false };
        yield return Tudo with { Ingresso = false }; yield return Tudo with { Campinho = false };
    }
}

// Uma tela de verdade numa janela sem tela: o jogo novo dos testes, a Sessao com as opcoes, o
// time humano e um jogador de outro time (o das fichas)
public sealed class Cena : IDisposable
{
    public readonly Perfil P;
    public readonly Configuracoes Cfg;
    public readonly Window Janela;
    public readonly Border Raiz = new();
    public readonly Sessao S;
    public readonly Jogo98 Jogo;
    public readonly Equipa Humano, Outro;
    public readonly Jogador Jogador;

    public Cena(Perfil p, Configuracoes cfg)
    {
        P = p; Cfg = cfg;
        p.Aplicar();
        Jogo = Ui.JogoNovo();
        Janela = new Window { Width = p.Largura, Height = p.Altura, Content = Raiz };
        Janela.Show();
        S = new Sessao(Jogo, c => Raiz.Child = c, cfg.OpcoesDoJogo(Jogo, Ui.Dados));
        Humano = Jogo.Equipas.First(e => Jogo.Treinadores.Any(t => t.ID == e.Treinador && t.Humano != 0));
        (Outro, Jogador) = Jogo.Equipas.Where(e => e != Humano).SelectMany(e => e.Jogadores.Select(j => (e, j)))
            .OrderByDescending(x => x.j.Forca).First();
        for (int i = 0; i < 12; i++) Memoria.SomaJogoNaTemporada(Jogador);
    }

    public void Mostrar(Control c) => S.Mostrar(c);

    // os textos da tela agora
    public List<Texto> Textos()
    {
        Ui.Assentar(Janela);
        return Inspecao.Textos(Janela);
    }

    // ------------------------------------------------------------------ as telas
    // Configuracoes, na secao Regras
    // (devolve a copia das Configuracoes que a tela edita: o que ela grava no OK)
    public Configuracoes Configuracoes(Configuracoes? cfg = null)
    {
        TelaConfiguracoes? tela = null;
        ComVariavel("RETROFOOT_SECAO", "1", () => Mostrar(tela = new TelaConfiguracoes(cfg ?? Cfg)));
        return (Configuracoes)typeof(TelaConfiguracoes).GetField("_cfg", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(tela)!;
    }

    // a tela do time na pagina (jogador, financas, arbitro; null = a que abre)
    public void Equipa(string? pagina = null) => ComVariavel("RETROFOOT_PAGINA", pagina, () =>
    {
        var m = S.J.M;
        var jogo = m.S.Jogos.FirstOrDefault(j => j.Casa == Humano.ID) ?? m.S.Jogos.FirstOrDefault(j => j.Fora == Humano.ID);
        Mostrar(new TelaEquipa(S, Humano, jogo));
    });

    public void Procura() => TelaProcura.Abrir(S, Humano);
    public void Plantel() => TelaPlantel.Abrir(S, Outro, Humano, Jogador.ID);
    public void Leilao() => TelaLeilao.DeTodos.Abrir(S, Outro, Jogador, 1_234_000);

    // (os ganchos das telas para abrir numa secao ou pagina sao variaveis de ambiente, lidas no
    // construtor)
    static void ComVariavel(string nome, string? valor, Action a)
    {
        Environment.SetEnvironmentVariable(nome, valor);
        try { a(); }
        finally { Environment.SetEnvironmentVariable(nome, null); }
    }

    public void Dispose() => Janela.Close();
}
