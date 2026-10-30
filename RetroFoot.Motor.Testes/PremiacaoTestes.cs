using System;
using System.Collections.Generic;
using System.Linq;
using RetroFoot.Motor.Base;
using RetroFoot.Motor.Temporada;
using Xunit;
using R = RetroFoot.Motor.Regras.Regras;

namespace RetroFoot.Motor.Testes;

// A opcao "Premiacao ampliada" (Regras.PremiacaoAmpliada; docs/premiacao.md): os valores por
// divisao e posicao, a marca no save e o pagamento no fim da temporada.
public class PremiacaoTestes
{
    const int P = 20_000_000;

    [Theory]
    [InlineData(0, 1, 2, 20_000_000)]
    [InlineData(0, 2, 2, 10_000_000)]
    [InlineData(1, 1, 2, 8_000_000)]
    [InlineData(2, 2, 2, 1_500_000)]
    [InlineData(3, 1, 2, 1_000_000)]
    [InlineData(3, 2, 2, 500_000)]
    [InlineData(0, 2, 3, 13_333_000)]
    [InlineData(0, 3, 3, 6_667_000)]
    [InlineData(1, 4, 4, 2_000_000)]
    [InlineData(0, 1, 1, 20_000_000)]
    public void Valores(int divisao, int posicao, int k, int esperado) =>
        Assert.Equal(esperado, PremiacaoAmpliada.Valor(P, divisao, posicao, k));

    [Theory]
    [InlineData(4, 1, 2)]
    [InlineData(-1, 1, 2)]
    [InlineData(0, 3, 2)]
    [InlineData(0, 0, 2)]
    public void ForaDaFaixa(int divisao, int posicao, int k) =>
        Assert.Equal(0, PremiacaoAmpliada.Valor(P, divisao, posicao, k));

    [Fact]
    public void ViceDaTaca()
    {
        Assert.Equal(3_000_000, PremiacaoAmpliada.ViceTaca(6_000_000));
        Assert.Equal(2_500_000, PremiacaoAmpliada.ViceTaca(5_000_000));
    }

    // ---------- a marca no save ----------

    static R Ampliada(int n = 8) => R.Launcher(n) with { DesempateVitorias = true, PremiacaoAmpliada = true };

    [Fact]
    public void JogoNovoClassicoSemMarca()
    {
        var s = DadosAbertos.JogoNovo(R.Launcher(8));
        Assert.False(R.PremiacaoDoSave(s));
        Assert.False(R.DoSave(s, R.Original).PremiacaoAmpliada);
        // a base das Configuracoes nao vale: o save manda
        Assert.False(R.DoSave(s, Ampliada()).PremiacaoAmpliada);
    }

    [Fact]
    public void JogoNovoAmpliadoComMarca()
    {
        var classico = DadosAbertos.JogoNovo(R.Launcher(8) with { DesempateVitorias = true }).Gravar();
        var s = Save.Jogo98.Ler(DadosAbertos.JogoNovo(Ampliada()).Gravar());
        Assert.True(R.PremiacaoDoSave(s));
        Assert.True(R.DoSave(s, R.Original).PremiacaoAmpliada);
        // fora a marca, o mesmo save da Classica
        Assert.True(R.MarcarPremiacao(s, false));
        Assert.True(s.Gravar().AsSpan().SequenceEqual(classico));
    }

    [Fact]
    public void MarcaNaoMexeNoK()
    {
        var s = Save.Jogo98.Ler(DadosAbertos.JogoNovo(Ampliada(10) with { SobemDescemEscolhido = 3 }).Gravar());
        Assert.Equal(3, R.SobemDescemDoSave(s));
        Assert.True(R.PremiacaoDoSave(s));
        Assert.True(R.MarcarSobemDescem(s, 0));
        Assert.True(R.PremiacaoDoSave(s));
        Assert.True(R.MarcarPremiacao(s, false));
        Assert.Equal(0, R.SobemDescemDoSave(s));
    }

    [Fact]
    public void NomeLongoNaoMarca()
    {
        var s = DadosAbertos.JogoNovo(R.Launcher(8));
        var nome = s.Divisoes[4].Nome;
        nome[0] = 19;
        var antes = (byte[])nome.Clone();
        Assert.False(R.MarcarPremiacao(s, true));
        Assert.Equal(antes, nome);
        Assert.False(R.PremiacaoDoSave(s));
    }

    // ---------- o fim de temporada ----------

    // o mesmo jogo novo (N = 8, K = 2, Reais) com a Classica e com a Ampliada, a mesma Taca
    // (campea = 3a divisao, 6a da lista) e o mesmo gerador: a diferenca de dinheiro entre os
    // dois e so a premiacao nova (no jogo novo ninguem marcou, entao o artilheiro paga todos)
    static (Save.Jogo98 S, Resultado R) Fim(bool ampliada, ushort? vice)
    {
        var s = DadosAbertos.JogoNovo(ampliada ? Ampliada() : R.Launcher(8) with { DesempateVitorias = true });
        var taca = s.Equipas.First(e => e.ID == s.Divisoes[2].Equipas[5]);
        s.OutrasEquipas.Clear();
        s.OutrasEquipas.Add(taca.ID);
        // uma meia-final nao jogada (golos 0xFFFF) e a final jogada contra o vice
        taca.Calendario.Add(new Save.EntradaCalendario { W08 = s.Divisoes[4].Equipas[0], B07 = 1, W0A = 0xFFFF, W0C = 0xFFFF, D10 = 0xFFFFFFFF });
        if (vice is ushort v) taca.Calendario.Add(new Save.EntradaCalendario { W08 = v, B07 = 1, W0A = 1, W0C = 0, D10 = 0xFFFFFFFF });
        var r = new Epoca(s, new Aleatorio(12345)).FimDeTemporada();
        return (s, r);
    }

    static Save.Equipa Eq(Save.Jogo98 s, ushort id) => s.Equipas.First(e => e.ID == id);
    static int Delta(Save.Jogo98 cla, Save.Jogo98 amp, ushort id) => (int)Eq(amp, id).Dinheiro - (int)Eq(cla, id).Dinheiro;

    static List<List<ushort>> Antes() =>
        DadosAbertos.JogoNovo(R.Launcher(8) with { DesempateVitorias = true }).Divisoes.Select(d => d.Equipas.ToList()).ToList();

    [Fact]
    public void ClassicaNaoPremiaMais()
    {
        var a = Antes();
        var (_, r) = Fim(false, a[3][5]);
        Assert.Empty(r.Premios);
    }

    [Fact]
    public void AmpliadaPagaAsDivisoes()
    {
        var a = Antes();
        var (cla, _) = Fim(false, a[3][5]);
        var (amp, r) = Fim(true, a[3][5]);
        Assert.Equal(0, Delta(cla, amp, a[0][0]));   // o campeao ja recebe P no caminho original
        Assert.Equal(10_000_000, Delta(cla, amp, a[0][1]));
        Assert.Equal(8_000_000, Delta(cla, amp, a[1][0]));
        Assert.Equal(4_000_000, Delta(cla, amp, a[1][1]));
        Assert.Equal(3_000_000, Delta(cla, amp, a[2][0]));
        Assert.Equal(1_500_000, Delta(cla, amp, a[2][1]));
        Assert.Equal(1_000_000, Delta(cla, amp, a[3][0]));
        Assert.Equal(500_000, Delta(cla, amp, a[3][1]));
        Assert.Equal(0, Delta(cla, amp, a[0][2]));
        Assert.Equal(0, Delta(cla, amp, a[4][0]));
        Assert.Equal(9, r.Premios.Count);
        Assert.Equal(new Premio(0, 1, a[0][0], P), r.Premios[0]);
        Assert.Equal(new Premio(3, 2, a[3][1], 500_000), r.Premios[7]);
    }

    [Fact]
    public void ViceDaCopa()
    {
        var a = Antes();
        var (cla, _) = Fim(false, a[3][5]);
        var (amp, r) = Fim(true, a[3][5]);
        Assert.Equal(3_000_000, Delta(cla, amp, a[3][5]));
        Assert.Equal(new Premio(-1, 2, a[3][5], 3_000_000), r.Premios[^1]);
        // o adversario da meia-final nao jogada nao conta
        Assert.Equal(0, Delta(cla, amp, a[4][0]));
    }

    [Fact]
    public void SemFinalSemVice()
    {
        var (_, r) = Fim(true, null);
        Assert.DoesNotContain(r.Premios, p => p.Divisao < 0);
        Assert.Equal(8, r.Premios.Count);
    }

    [Fact]
    public void AcumulaPremios()
    {
        var a = Antes();
        var (cla, _) = Fim(false, a[0][1]);
        var (amp, _) = Fim(true, a[0][1]);
        Assert.Equal(13_000_000, Delta(cla, amp, a[0][1]));
    }

    [Fact]
    public void UsaClassificacaoDeAntes()
    {
        var a = Antes();
        var (amp, r) = Fim(true, a[3][5]);
        Assert.Equal(a[1].Take(2), r.Premios.Where(p => p.Divisao == 1).Select(p => p.Equipa));
        // as que subiram da 2a ja estao na 1a e mesmo assim recebem como 2a divisao
        Assert.All(r.Subiram[1], id => Assert.Contains(id, amp.Divisoes[0].Equipas));
    }

    [Fact]
    public void BalancoPremios()
    {
        var a = Antes();
        var (cla, _) = Fim(false, a[3][5]);
        var (amp, _) = Fim(true, a[3][5]);
        Assert.Equal(8_000_000, Mercado.MercadoJogo.Balanco(Eq(amp, a[1][0]))[2] - Mercado.MercadoJogo.Balanco(Eq(cla, a[1][0]))[2]);
    }
}
