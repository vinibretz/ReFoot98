using System;
using System.Collections.Generic;
using System.Linq;
using RetroFoot.App;
using Xunit;
using static RetroFoot.App.Idioma;

namespace RetroFoot.App.Testes;

// Cada opcao da versao Atual mostra (ligada) ou esconde (desligada) so o que e dela, em cada
// visual: todas ligadas, todas desligadas e cada uma desligada sozinha
public class OpcoesTestes
{
    // um texto exatamente igual em algum lugar da tela (visivel ou rolado para fora)
    static bool Ha(List<Texto> t, string s) => t.Any(x => x.Valor == s);

    static void Conferir(List<string> erros, string onde, string oque, bool quero, bool tem)
    {
        if (quero != tem) erros.Add($"{onde}: {oque} {(quero ? "devia aparecer e nao aparece" : "devia sumir e aparece")}");
    }

    static List<Texto> Abrir(Perfil p, Opcoes4 o, Action<Cena> abrir)
    {
        using var c = new Cena(p, o.Cfg());
        abrir(c);
        return c.Textos();
    }

    [Theory]
    [MemberData(nameof(Perfil.Nomes), MemberType = typeof(Perfil))]
    public void CadaOpcaoSoOQueEDela(string perfil)
    {
        var p = Perfil.De(perfil);
        var erros = new List<string>();
        Ui.Rodar(() =>
        {
            foreach (var o in Opcoes4.Combinacoes())
            {
                // as fichas: a nota e o risco (Nota e risco de lesao) e a linha dos amarelos no
                // historico (Cartoes amarelos)
                foreach (var (nome, abrir) in new (string, Action<Cena>)[] { ("Ficha (Elenco)", c => c.Plantel()), ("Leilão", c => c.Leilao()) })
                {
                    var t = Abrir(p, o, abrir);
                    Conferir(erros, $"{nome} {o}", "a Nota", o.Nota, Ha(t, T("Nota")));
                    Conferir(erros, $"{nome} {o}", "o Risco de lesão", o.Nota, Ha(t, T("Risco de lesão")));
                    Conferir(erros, $"{nome} {o}", "os Cartões amarelos", o.Amarelos, Ha(t, T("Cartões amarelos")));
                }

                // o Procurar estendido (Nota e risco de lesao): os filtros e o Comprar
                {
                    var t = Abrir(p, o, c => c.Procura());
                    Conferir(erros, $"Procurar {o}", "o filtro de Comportamento", o.Nota, Ha(t, T("Comportamento")));
                    Conferir(erros, $"Procurar {o}", "o filtro de Risco de lesão", o.Nota, Ha(t, T("Risco de lesão")));
                    // (o Procurar do celular Moderno compra pela ficha: nunca tem o botao)
                    if (p != Perfil.CelularModerno)
                        Conferir(erros, $"Procurar {o}", "o botão Comprar", o.Nota, Ha(t, T("Comprar")));
                }

                // a tela do time: o jogador (no celular a ficha e a do Elenco, acima)
                if (!p.NoCelular)
                {
                    var t = Abrir(p, o, c => c.Equipa("jogador"));
                    Conferir(erros, $"Equipa: jogador {o}", "a Nota", o.Nota, Ha(t, T("Nota")));
                    Conferir(erros, $"Equipa: jogador {o}", "o Risco de lesão", o.Nota, Ha(t, T("Risco de lesão")));
                    Conferir(erros, $"Equipa: jogador {o}", "os Cartões amarelos", o.Amarelos, Ha(t, T("Cartões amarelos")));
                    // (no Moderno, os rotulos da ficha inteiros, sem "...": o comportamento comprido
                    // encolhe a letra)
                    if (p.Moderno)
                    {
                        Conferir(erros, $"Equipa: jogador {o}", "o rótulo Valor de mercado inteiro", true, Ha(t, T("Valor de mercado")));
                        Conferir(erros, $"Equipa: jogador {o}", "o rótulo Comportamento inteiro", true, Ha(t, T("Comportamento")));
                    }
                }

                // as Financas: a previsao do publico e da renda (Preco do ingresso)
                {
                    var t = Abrir(p, o, c => c.Equipa("financas"));
                    Conferir(erros, $"Equipa: finanças {o}", "a Renda prevista", o.Ingresso, Inspecao.Tem(t, T("Renda prevista")));
                }

                // o arbitro: a chance de penalti de cada time (Campinho e arbitro)
                {
                    var t = Abrir(p, o, c => c.Equipa("arbitro"));
                    string penalti = T("   Pênalti: {0}%   Cartões: -{1}%").Trim().Split('{')[0].Trim();
                    Conferir(erros, $"Equipa: árbitro {o}", $"o \"{penalti}\"", o.Campinho, Inspecao.Tem(t, penalti));
                }
            }
        });
        Assert.True(erros.Count == 0, $"{p}:\n" + string.Join("\n", erros));
    }
}
