using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using RetroFoot.App;
using RetroFoot.Motor.Base;
using RetroFoot.Motor.Ciclo;
using RetroFoot.Motor.Online;
using RetroFoot.Motor.Save;
using Xunit;

namespace RetroFoot.App.Testes;

// O convidado compra na Lista de Jogadores como o host: a janela continua aberta (o JogoRemoto de
// verdade, numa sala do banco da memoria; o host e simulado: responde e publica o retrato novo)
public class OnlineCompraTestes
{
    const BindingFlags Priv = BindingFlags.Instance | BindingFlags.NonPublic;
    const string Rodada = "2026-1";

    // roda a fila da interface ate a condicao valer
    static bool Ate(Func<bool> cond, int ms = 10_000)
    {
        var fim = DateTime.UtcNow.AddMilliseconds(ms);
        while (DateTime.UtcNow < fim)
        {
            Dispatcher.UIThread.RunJobs();
            if (cond()) return true;
            Thread.Sleep(10);
        }
        return cond();
    }

    static void Publicar(Sala sala, Retrato r, int versao) =>
        sala.B.Gravar($"{sala.Raiz}/retrato", new JsonObject
        {
            ["rodada"] = Rodada, ["versao"] = versao, ["host"] = "h1", ["dados"] = Convert.ToBase64String(r.Bytes()),
        }).GetAwaiter().GetResult();

    [Fact]
    public void ConvidadoCompraVariosSemFecharAListaDeJogadores() => Ui.Rodar(() =>
    {
        Perfil.Classico43.Aplicar();
        var cfg = new Configuracoes { Modificada = true };
        var save = Ui.JogoNovo();

        // o host, parado na preparacao
        var jogoHost = new JogoEmCurso(save, 77, cfg.OpcoesDoJogo(save, Ui.Dados) with { Registrado = true }, new Humano());
        var timeHost = save.Equipas.First(e => save.Treinadores.Any(t => t.ID == e.Treinador && t.Humano != 0));
        var jogoDoTime = save.Jogos.First(j => j.Casa == timeHost.ID || j.Fora == timeHost.ID);
        var executor = new ExecutorComandos(jogoHost);

        var banco = new BancoMemoria();
        var sh = Sala.Criar(banco, "host", "Juan", "teste", "Sala dos testes").GetAwaiter().GetResult();
        var sc = Sala.Entrar(banco, "cli", sh.Codigo, "João").GetAwaiter().GetResult();
        sh.DarEquipa("cli", timeHost.ID).GetAwaiter().GetResult();

        var janela = new Window { Width = 1024, Height = 768, Content = new Border() };
        janela.Show();
        var raiz = (Border)janela.Content!;
        using var cli = new ClienteOnline(sc);
        var remoto = new JogoRemoto(c => raiz.Child = c, () => { }, sc, cli);
        // (sem o sorteio dos times da 1a rodada: direto a tela do time)
        typeof(JogoRemoto).GetField("_viuSorteio", Priv)!.SetValue(remoto, true);
        Sessao? Atual() => (Sessao?)typeof(JogoRemoto).GetField("_sessao", Priv)!.GetValue(remoto);

        int versao = 1, publicados = 0, aceitos = 0;
        string motivo = "";
        using var host = banco.Observar($"{sh.Raiz}/comandos/cli", (caminho, v) =>
        {
            var chave = caminho.Trim('/');
            if (chave.Length == 0 || chave.Contains('/') || v == null) return;
            var r = executor.Executar(timeHost.ID, Comando.Ler((string?)v["c"] ?? ""));
            if (r.Ok) Interlocked.Increment(ref aceitos); else motivo = r.Motivo;
            // (como o host: primeiro a resposta, depois o retrato novo)
            int nova = Interlocked.Increment(ref versao);
            banco.Gravar($"{sh.Raiz}/resultados/cli/{chave}", new JsonObject { ["ok"] = r.Ok, ["motivo"] = r.Motivo, ["versao"] = nova }).GetAwaiter().GetResult();
            Publicar(sh, jogoHost.Fotografar(timeHost, jogoDoTime), nova);
            Interlocked.Increment(ref publicados);
        });

        try
        {
            Publicar(sh, jogoHost.Fotografar(timeHost, jogoDoTime), 1);
            Assert.True(Ate(() => Atual() != null && raiz.GetVisualDescendants().OfType<TelaEquipa>().Any()), "a tela do time nao abriu");
            var antes = Atual()!;
            var meu = antes.Equipa(timeHost.ID)!;
            long caixa = meu.Dinheiro;
            var alvos = antes.S.Equipas.Where(e => e.ID != meu.ID)
                .SelectMany(e => e.Jogadores.Select(p => (e, p, Preco: antes.Mercado.PrecoParaComprar(meu, e, p))))
                .Where(x => x.Preco > 0).OrderBy(x => x.Preco).Take(2).ToList();
            Assert.Equal(2, alvos.Count);

            TelaProcura.Abrir(antes, meu);
            var form = raiz.GetVisualDescendants().OfType<MercadoForm>().Single();
            form.GetType().GetMethod("Procurar", Priv)!.Invoke(form, null);
            var achados = (System.Collections.Generic.List<ushort>)typeof(TelaProcura).GetField("_achados", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
            var lista = (MercadoForm.Lista)form.GetType().GetField("_lista", Priv)!.GetValue(form)!;

            void Comprar(ushort id, int nPublicados)
            {
                lista.Sel = achados.IndexOf(id);
                Assert.True(lista.Sel >= 0, "o jogador nao esta na lista");
                form.GetType().GetMethod("Comprar", Priv)!.Invoke(form, null);
                Assert.True(Ate(() => Volatile.Read(ref publicados) >= nPublicados), "o host nao recebeu a compra");
                Assert.True(Volatile.Read(ref aceitos) >= nPublicados, "o host recusou: " + motivo);
                // (o tempo do retrato chegar e, antes, a tela ser refeita)
                Ate(() => false, 400);
            }

            Comprar(alvos[0].p.ID, 1);
            Assert.Same(antes, Atual());
            Assert.NotNull(TopLevel.GetTopLevel(form));
            int selecionado = achados.IndexOf(alvos[0].p.ID);
            Assert.Equal(selecionado, lista.Sel);
            // (a janela mostra a compra na hora, como no host: o jogador sem preco e o dinheiro)
            Assert.Equal(meu.ID, alvos[0].p.W36);
            Assert.Equal(caixa - alvos[0].Preco, (long)meu.Dinheiro);

            Comprar(alvos[1].p.ID, 2);
            Assert.Same(antes, Atual());
            Assert.NotNull(TopLevel.GetTopLevel(form));
            Assert.Equal(2, Volatile.Read(ref aceitos));
            Assert.Equal(caixa - alvos[0].Preco - alvos[1].Preco, (long)meu.Dinheiro);

            // fechou: a tela do time se refaz com o jogo do host
            form.Fechar();
            Assert.True(Ate(() => Atual() != antes && raiz.GetVisualDescendants().OfType<TelaEquipa>().Any()), "a tela nao se refez ao fechar a janela");
            var depois = Atual()!;
            Assert.Empty(raiz.GetVisualDescendants().OfType<MercadoForm>());
            Assert.Equal(caixa - alvos[0].Preco - alvos[1].Preco, (long)depois.Equipa(meu.ID)!.Dinheiro);
            Assert.Equal(meu.ID, depois.S.Equipas.SelectMany(e => e.Jogadores).First(p => p.ID == alvos[1].p.ID).W36);
        }
        finally
        {
            ((DispatcherTimer)typeof(JogoRemoto).GetField("_vigia", Priv)!.GetValue(remoto)!).Stop();
            TelaJornada.SoTime = null;
            janela.Close();
        }
    });
}
