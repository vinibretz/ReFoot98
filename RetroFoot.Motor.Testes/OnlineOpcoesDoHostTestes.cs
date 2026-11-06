using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using RetroFoot.Motor.Ciclo;
using RetroFoot.Motor.Save;
using RetroFoot.Motor.Online;
using Xunit;
using TRegras = RetroFoot.Motor.Regras.Regras;

namespace RetroFoot.Motor.Testes;

// as opcoes da versao Atual do host (info/opcoes da sala): quem entra joga com elas, nao com as
// das proprias Configuracoes
public class OnlineOpcoesDoHostTestes
{
    [Fact]
    public void ConvidadoUsaAsOpcoesDoHost()
    {
        var convidado = TRegras.Launcher(8);   // tudo ligado neste aparelho
        var o = JsonNode.Parse("""{"amarelos":true,"nota":false,"ingresso":false,"campinho":true}""");
        var r = Sala.ComOpcoesDoHost(convidado, o);
        Assert.True(r.CartoesAmarelos);
        Assert.False(r.NotaERisco);
        Assert.False(r.PrecoIngresso);
        Assert.True(r.CampinhoArbitro);
        Assert.Equal(convidado.N, r.N);
        // sem as opcoes (um host de antes delas, ou ainda nao vieram): as do aparelho
        Assert.Equal(convidado, Sala.ComOpcoesDoHost(convidado, null));
        // uma chave que falta fica como estava
        var parcial = Sala.ComOpcoesDoHost(convidado, JsonNode.Parse("""{"ingresso":false}"""));
        Assert.False(parcial.PrecoIngresso);
        Assert.True(parcial.NotaERisco);
    }

    // pela sala (o banco da memoria): o host grava info/opcoes e quem entra le as mesmas
    [Fact]
    public async Task PelaSala()
    {
        var banco = new BancoMemoria();
        var sh = await Sala.Criar(banco, "host", "Juan", "0.2.0");
        var sc = await Sala.Entrar(banco, "cli", sh.Codigo, "João");
        var host = TRegras.Launcher(8) with { NotaERisco = false, PrecoIngresso = false };
        await sh.DefinirOpcoes(host);
        var lidas = await banco.Ler($"{sc.Raiz}/info/opcoes");
        var convidado = Sala.ComOpcoesDoHost(TRegras.Launcher(8), lidas);
        Assert.True(convidado.CartoesAmarelos);
        Assert.False(convidado.NotaERisco);
        Assert.False(convidado.PrecoIngresso);
        Assert.True(convidado.CampinhoArbitro);
    }

    // o HostOnline publica as opcoes do jogo dele ao comecar
    [FactComArquivo("Oraculo", "j3-entrada.e98")]
    public async Task HostPublicaAoComecar()
    {
        var banco = new BancoMemoria();
        var sh = await Sala.Criar(banco, "host", "Juan", "0.2.0");
        var s = Jogo98.Ler(File.ReadAllBytes(Requisitos.Achar("Oraculo", "j3-entrada.e98")!));
        var regras = TRegras.Launcher(8) with { CartoesAmarelos = false, CampinhoArbitro = false };
        var jg = new JogoEmCurso(s, 1, Opcoes.Padrao with { Regras = regras }, new Humano());
        using var host = new HostOnline(sh, jg);
        JsonNode? o = null;
        for (int i = 0; i < 50 && o == null; i++) { o = await banco.Ler($"{sh.Raiz}/info/opcoes"); if (o == null) await Task.Delay(20); }
        Assert.NotNull(o);
        Assert.Equal(jg.M.Regras with { }, Sala.ComOpcoesDoHost(jg.M.Regras, o));
        var r = Sala.ComOpcoesDoHost(TRegras.Launcher(8), o);
        Assert.False(r.CartoesAmarelos);
        Assert.True(r.NotaERisco);
        Assert.True(r.PrecoIngresso);
        Assert.False(r.CampinhoArbitro);
    }
}
