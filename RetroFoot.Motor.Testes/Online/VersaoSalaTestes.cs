using System.Threading.Tasks;
using RetroFoot.Motor.Online;
using Xunit;

namespace RetroFoot.Motor.Testes.Online;

// A versao da sala: quem entra com outra versao do jogo e recusado (VersaoDiferente); o host
// que reabre a sala depois de atualizar o jogo grava a versao nova nela
public class VersaoSalaTestes
{
    [Theory]
    [InlineData("ReFoot98 0.2.6", "ReFoot98 0.2.6", true)]
    [InlineData("ReFoot98 0.2.5", "ReFoot98 0.2.6", false)]
    [InlineData("RetroFoot 0.2.6", "ReFoot98 0.2.6", false)]
    // (as salas de antes do aviso e as do RetroFoot original: so o numero)
    [InlineData("0.2.6", "ReFoot98 0.2.6", true)]
    [InlineData("0.2.2", "ReFoot98 0.2.6", false)]
    public void MesmaVersao(string daSala, string minha, bool igual) =>
        Assert.Equal(igual, Sala.MesmaVersao(daSala, minha));

    [Fact]
    public async Task EntrarRecusaOutraVersao()
    {
        var b = new BancoMemoria();
        var sh = await Sala.Criar(b, "host", "Host", "ReFoot98 0.2.6");
        var ex = await Assert.ThrowsAsync<Sala.VersaoDiferente>(() => Sala.Entrar(b, "cli", sh.Codigo, "Cli", versao: "ReFoot98 0.2.5"));
        Assert.Equal("ReFoot98 0.2.6", ex.DaSala);
        // (recusado antes de pegar uma vaga)
        Assert.Single(await sh.Jogadores());
        await Sala.Entrar(b, "cli", sh.Codigo, "Cli", versao: "ReFoot98 0.2.6");
        Assert.Equal(2, (await sh.Jogadores()).Count);
        // (sem a versao, como antes: entra)
        await Sala.Entrar(b, "outro", sh.Codigo, "Outro");
    }

    [Fact]
    public async Task ReabrirGravaAVersaoDoHost()
    {
        var b = new BancoMemoria();
        var sh = await Sala.Criar(b, "host", "Host", "ReFoot98 0.2.5");
        Assert.NotNull(await Sala.Reabrir(b, "host", sh.Codigo, versao: "ReFoot98 0.2.6"));
        Assert.Equal("ReFoot98 0.2.6", (string?)await b.Ler($"salas/{sh.Codigo}/info/versao"));
        await Assert.ThrowsAsync<Sala.VersaoDiferente>(() => Sala.Entrar(b, "cli", sh.Codigo, "Cli", versao: "ReFoot98 0.2.5"));
    }
}
