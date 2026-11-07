using System;
using System.IO;
using System.Linq;
using Avalonia.Headless;
using Xunit;

namespace RetroFoot.App.Testes;

// Ferramenta (nao e um teste): os textos que uma tela desenha em cada visual, com tudo ligado e
// tudo desligado, um arquivo por combinacao, com as sobreposicoes e os cortes no fim. Para
// escrever um teste novo ou entender uma falha (docs/testes.md):
//   $env:DESPEJO = "C:\pasta"; $env:DESPEJO_TELA = "config"   (ou plantel, leilao, procura,
//   jogador, financas, arbitro, online, sala, salaconvidado; DESPEJO_PNG=1 grava as imagens);
//   dotnet test RetroFoot.App.Testes --filter Despejar
// Sem DESPEJO, nao faz nada.
public class Despejo
{
    [Fact]
    public void Despejar()
    {
        var saida = Environment.GetEnvironmentVariable("DESPEJO");
        if (string.IsNullOrEmpty(saida)) return;
        Directory.CreateDirectory(saida);
        var tela = Environment.GetEnvironmentVariable("DESPEJO_TELA") ?? "config";
        Ui.Rodar(() =>
        {
            foreach (var p in Perfil.Todos)
                foreach (var o in new[] { Opcoes4.Tudo, Opcoes4.Nada })
                {
                    using var c = new Cena(p, o.Cfg());
                    switch (tela)
                    {
                        case "config": c.Configuracoes(); break;
                        case "plantel": c.Plantel(); break;
                        case "leilao": c.Leilao(); break;
                        case "procura": c.Procura(); break;
                        case "online": c.OnlineEntrada(); break;
                        case "sala": c.Sala(true); break;
                        case "salaconvidado": c.Sala(false); break;
                        default: c.Equipa(tela); break;
                    }
                    var t = c.Textos();
                    // (x = fora da vista: rolado, coberto ou recortado)
                    var l = t.Select(x => (x.Visivel ? "  " : "x ") + x + "  [" + x.Dono.GetType().Name + "]").ToList();
                    l.Add("SOBREPOSTOS: " + string.Join(" | ", Inspecao.Sobrepostos(t).Select(s => s.A + " x " + s.B)));
                    l.Add("FALHAS: " + string.Join(" | ", Inspecao.Falhas.Distinct()));
                    l.Add("CORTADOS: " + string.Join(" | ", Inspecao.CortadosNaLargura(t)));
                    var arq = Path.Combine(saida, $"{tela}-{p.Nome.Replace(' ', '_').Replace(':', '-')}-{o}");
                    File.WriteAllLines(arq + ".txt", l);
                    // (DESPEJO_PNG=1: a imagem da tela tambem)
                    if (Environment.GetEnvironmentVariable("DESPEJO_PNG") == "1") c.Janela.CaptureRenderedFrame()?.Save(arq + ".png");
                }
        });
    }
}
