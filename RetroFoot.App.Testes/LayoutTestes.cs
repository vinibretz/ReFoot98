using System;
using System.Collections.Generic;
using System.Linq;
using RetroFoot.App;
using Xunit;

namespace RetroFoot.App.Testes;

// As telas que as opcoes da versao Atual mudam, em cada visual, com tudo ligado e tudo
// desligado e nos 4 idiomas: nenhum texto por cima de outro e nenhum cortado na largura
public class LayoutTestes
{
    // as telas conferidas: o nome e como abrir
    public static readonly (string Nome, Action<Cena> Abrir)[] Telas =
    {
        ("Configurações", c => c.Configuracoes()),
        ("Ficha (Elenco)", c => c.Plantel()),
        ("Leilão", c => c.Leilao()),
        ("Procurar", c => c.Procura()),
        ("Equipa: jogador", c => c.Equipa("jogador")),
        ("Equipa: finanças", c => c.Equipa("financas")),
        ("Equipa: árbitro", c => c.Equipa("arbitro")),
    };

    // os problemas de uma tela aberta (vazio = tudo certo)
    public static List<string> Problemas(Cena c)
    {
        var t = c.Textos();
        var r = new List<string>();
        r.AddRange(Inspecao.Sobrepostos(t).Select(x => $"sobrepostos: {x.A} e {x.B}"));
        r.AddRange(Inspecao.CortadosNaLargura(t).Select(x => $"cortado: {x}"));
        r.AddRange(Inspecao.Falhas.Distinct().Select(x => "nao deu para ler: " + x));
        if (!t.Any(x => x.Visivel)) r.Add("nenhum texto na tela");
        return r;
    }

    [Theory]
    [MemberData(nameof(Perfil.Nomes), MemberType = typeof(Perfil))]
    public void SemSobreposicaoNemCorte(string perfil)
    {
        var p = Perfil.De(perfil);
        var erros = new List<string>();
        Ui.Rodar(() =>
        {
            try
            {
                foreach (var idioma in Idioma.Codigos)
                {
                    Idioma.Aplicar(idioma);
                    foreach (var o in new[] { Opcoes4.Tudo, Opcoes4.Nada })
                        foreach (var (nome, abrir) in Telas)
                        {
                            using var c = new Cena(p, o.Cfg());
                            abrir(c);
                            foreach (var e in Problemas(c)) erros.Add($"{nome} [{idioma}, {o}]: {e}");
                        }
                }
            }
            finally { Idioma.Aplicar(Idioma.PT); }
        });
        Assert.True(erros.Count == 0, $"{p}: {erros.Count} problema(s)\n" + string.Join("\n", erros.Take(40)));
    }
}
