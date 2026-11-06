using System;
using RetroFoot.Motor.Base;
using RetroFoot.Motor.Rodada;
using RetroFoot.Motor.Save;
using Xunit;
using static RetroFoot.Motor.Rodada.Campos;
using Equipa = RetroFoot.Motor.Save.Equipa;

namespace RetroFoot.Motor.Testes;

// O preco dos ingressos escolhido pelo tecnico (patch do launcher, ingressos.asm; secao 12
// de docs internos do launcher)
public class IngressosTestes
{
    static Ext X(double v) => Ext.F64(v);

    static Equipa Time(ushort id, byte escolha = 0)
    {
        var e = new Equipa { ID = id, Estadio = 200, Treinador = id };
        e.Cabeca[0] = (byte)'E'; e.Cabeca[1] = (byte)'F'; e.Cabeca[2] = (byte)'a';
        e.Cabeca[Ingressos.CabecaByte] = escolha;
        e.Moral = new byte[] { 0x81, 0, 0, 0, 0, 0 };          // 1,0
        for (int i = 0; i < 16; i++)
            e.Jogadores.Add(new Jogador { ID = (ushort)(id * 100 + i), Forca = 40, Posicao = (byte)(i % 4) });
        return e;
    }

    static (Mundo, Equipa, Equipa) Mundo2(byte escolha, uint semente = 1234, bool preco = true)
    {
        var s = new Jogo98();
        X(1).Bytes().CopyTo(s.Cabecalho.Real2B46, 0);
        s.Treinadores.Add(new Treinador { ID = 10 });
        s.Treinadores.Add(new Treinador { ID = 20 });
        var casa = Time(10, escolha);
        var fora = Time(20);
        s.Equipas.Add(casa); s.Equipas.Add(fora);
        var m = new Mundo(s, semente);
        m.Regras = m.Regras with { PrecoIngresso = preco };
        return (m, casa, fora);
    }

    static int PublicoDe(Mundo m, Equipa casa, Equipa fora)
    {
        var j = new Jogo { Taca = 0, Casa = casa.ID, Fora = fora.ID };
        m.Publico(j);
        return (int)j.D40;
    }

    static int BilheteiraDe(Mundo m, Equipa casa, Equipa fora)
    {
        var j = new Jogo { Taca = 0, Casa = casa.ID, Fora = fora.ID, D40 = 12345 };
        X(7).Bytes().CopyTo(j.B36, 0);
        m.FimDeJogo(j);
        return Mundo.Saldo(casa, Mundo.BalBilheteira);
    }

    // a opcao "Preco do ingresso" desligada: o preco gravado no save nao vale (publico e
    // bilheteira de tabela), mas fica guardado para quando a opcao for religada
    [Fact]
    public void PrecoDesligadoUsaTabela()
    {
        var (m0, c0, f0) = Mundo2(0);
        int tabela = PublicoDe(m0, c0, f0);
        var (m, casa, fora) = Mundo2(12, preco: false);
        Assert.Equal(tabela, PublicoDe(m, casa, fora));
        var (mb, cb, fb) = Mundo2(12, preco: false);
        Assert.Equal(12345 * 7, BilheteiraDe(mb, cb, fb));
    }

    [Fact]
    public void PrecoReligadoVoltaOEscolhido()
    {
        var (mq, cq, fq) = Mundo2(12);
        int escolhido = PublicoDe(mq, cq, fq);
        var (md, cd, fd) = Mundo2(12, preco: false);
        Assert.NotEqual(escolhido, PublicoDe(md, cd, fd));
        Assert.Equal(12, cd.Cabeca[4]);   // o preco escolhido continua no save
        // a mesma equipa (mesmo sorteio) com a opcao religada: volta o preco escolhido
        var (m, casa, fora) = Mundo2(12, preco: false);
        m.Regras = m.Regras with { PrecoIngresso = true };
        Assert.Equal(escolhido, PublicoDe(m, casa, fora));
        var (mb, cb, fb) = Mundo2(12, preco: false);
        mb.Regras = mb.Regras with { PrecoIngresso = true };
        Assert.Equal(12345 * 21, BilheteiraDe(mb, cb, fb));
    }

    // um save gravado no meio da jornada (a marca do preco que entrou no publico vai no cabecalho
    // da equipa) e reaberto com a opcao trocada: o publico do jogo da jornada e refeito
    static (Mundo, Jogo, Equipa) Reaberto(bool criadoCom, bool reabertoCom, byte escolha = 12)
    {
        var (m, casa, fora) = Mundo2(escolha, preco: criadoCom);
        var j = new Jogo { Taca = 0, Casa = casa.ID, Fora = fora.ID };
        m.S.Jogos.Add(j);
        m.Publico(j);
        m.S.Cabecalho.G271E = casa.ID;   // gravado na preparacao do mandante
        var m2 = new Mundo(m.S, 99);
        m2.Regras = m2.Regras with { PrecoIngresso = reabertoCom };
        return (m2, j, casa);
    }

    [Fact]
    public void SaveReabertoSemAOpcaoVoltaATabela()
    {
        var (m, j, casa) = Reaberto(true, true);
        uint com = j.D40;
        m.AjustarPublicoAoPreco();
        Assert.Equal(com, j.D40);   // a mesma opcao: nada muda
        Assert.Equal(30000, Ingressos.Aplicada(casa));
        (m, j, casa) = Reaberto(true, false);
        m.AjustarPublicoAoPreco();
        // 300% tirava ~76% do publico: sem o preco, ~1/0,2397 (com o sorteio no meio)
        Assert.InRange(j.D40 / (double)com, 1 / 0.2397 * 0.8, 1 / 0.2397 * 1.25);
        Assert.Equal(Ingressos.Cem, Ingressos.Aplicada(casa));
        uint depois = j.D40;
        m.AjustarPublicoAoPreco();
        Assert.Equal(depois, j.D40);   // ja refeito: nao muda de novo
    }

    [Fact]
    public void SaveReabertoComAOpcaoAplicaOPreco()
    {
        var (m, j, casa) = Reaberto(false, true);
        uint sem = j.D40;
        Assert.Equal(Ingressos.Cem, Ingressos.Aplicada(casa));
        m.AjustarPublicoAoPreco();
        Assert.InRange(j.D40 / (double)sem, 0.2397 * 0.8, 0.2397 * 1.25);
        // sem preco escolhido nao ha marca (o save fica como o do original)
        var (m0, j0, c0) = Reaberto(false, true, 0);
        uint p0 = j0.D40;
        m0.AjustarPublicoAoPreco();
        Assert.Equal(p0, j0.D40);
        Assert.Equal(0, Ingressos.Aplicada(c0));
    }

    // a versao Retro (Regras.Original) como o jogo de 1998: um preco gravado (do launcher ou de
    // uma carreira na Atual) nao vale
    [Fact]
    public void RetroIgnoraPrecoGravado()
    {
        var (m0, c0, f0) = Mundo2(0);
        int tabela = PublicoDe(m0, c0, f0);
        var s = new Jogo98();
        X(1).Bytes().CopyTo(s.Cabecalho.Real2B46, 0);
        s.Treinadores.Add(new Treinador { ID = 10 });
        s.Treinadores.Add(new Treinador { ID = 20 });
        var casa = Time(10, 12); var fora = Time(20);
        s.Equipas.Add(casa); s.Equipas.Add(fora);
        var m = new Mundo(s, 1234);
        Assert.False(m.Regras.PrecoIngresso);
        Assert.Equal(tabela, PublicoDe(m, casa, fora));
        Assert.Equal(12345 * 7, BilheteiraDe(m, casa, fora));
    }

    [Fact]
    public void EscolhaNoCabecalho()
    {
        var e = Time(1);
        Assert.Equal(0, Ingressos.Escolha(e));
        Assert.Equal(100, Ingressos.Percentual(e));
        // o campo do launcher: de 25 em 25 o mais perto, de 50 a 300; o byte = valor/25
        foreach (var (pedido, byteQuero, pctQuero) in new[] { (100, 4, 100), (300, 12, 300), (50, 2, 50),
                     (10, 2, 50), (999, 12, 300), (137, 5, 125), (138, 6, 150) })
        {
            Ingressos.Escolher(e, pedido);
            Assert.Equal(byteQuero, e.Cabeca[4]);
            Assert.Equal(pctQuero, Ingressos.Percentual(e));
        }
        // fora de 2..12 vale como nao escolhido (fator() do ingressos.asm)
        e.Cabeca[4] = 1; Assert.Equal(0, Ingressos.Escolha(e));
        e.Cabeca[4] = 13; Assert.Equal(0, Ingressos.Escolha(e));
    }

    [Fact]
    public void PrecoDaRenda()
    {
        // os casos comprovados no launcher: tabela 2 x 300% = 6, tabela 2 x 150% = 3
        Assert.Equal(6.0, Ingressos.Preco(X(2), Time(1, 12)).Float64());
        Assert.Equal(3.0, Ingressos.Preco(X(2), Time(1, 6)).Float64());
        Assert.Equal(1.0, Ingressos.Preco(X(2), Time(1, 2)).Float64());
        // sem escolha (ou byte invalido) o preco e o mesmo Ext, bit a bit
        var p = X(7.3);
        Assert.Equal(p.Bytes(), Ingressos.Preco(p, Time(1)).Bytes());
        Assert.Equal(p.Bytes(), Ingressos.Preco(p, Time(1, 200)).Bytes());
        Assert.Equal(p.Bytes(), Ingressos.Preco(p, null).Bytes());
        // 4 = 100% escolhido: x 1,0 exato
        Assert.Equal(p.Bytes(), Ingressos.Preco(p, Time(1, 4)).Bytes());
    }

    [Fact]
    public void ProcuraCaiComOPreco()
    {
        // (escolha/4)^-1,3, como floats da tabela
        for (byte k = 2; k <= 12; k++)
        {
            double f = Ingressos.Publico(X(1), Time(1, k)).Float64();
            Assert.True(Math.Abs(f - Math.Pow(k / 4.0, -1.3)) < 2e-4, $"k={k}: {f}");
            Assert.Equal((double)(float)f, f);
        }
        Assert.Equal(1.0, Ingressos.Publico(X(1), Time(1, 4)).Float64());
        Assert.Equal(X(1234.5).Bytes(), Ingressos.Publico(X(1234.5), Time(1)).Bytes());
    }

    [Fact]
    public void PublicoDoJogo()
    {
        int Pub(byte escolha, byte taca = 0)
        {
            var (m, casa, fora) = Mundo2(escolha);
            var j = new Jogo { Taca = taca, Casa = casa.ID, Fora = fora.ID };
            m.Publico(j);
            return (int)j.D40;
        }
        foreach (byte taca in new byte[] { 0, 1 })
        {
            int base0 = Pub(0, taca);
            Assert.True(base0 > 0);
            Assert.Equal(base0, Pub(4, taca));                     // 100%: igual
            for (byte k = 2; k <= 12; k++)
            {
                // base0 ja esta arredondado: erro de 0,5 x fator, mais o arredondamento novo
                double f = Ingressos.Publico(X(1), Time(1, k)).Float64();
                double quero = base0 * f;
                Assert.True(Math.Abs(Pub(k, taca) - quero) <= 0.5 * f + 0.5 + 1e-9, $"k={k} taca={taca}: {Pub(k, taca)} x {quero}");
            }
        }
        // o limite do estadio vem depois: com 50% a procura passa da capacidade
        var (m2, c2, f2) = Mundo2(2);
        c2.Estadio = 1;
        var j2 = new Jogo { Taca = 0, Casa = c2.ID, Fora = f2.ID };
        m2.Publico(j2);
        Assert.Equal(5000u, j2.D40);
    }

    [Fact]
    public void BilheteiraNoFimDoJogo()
    {
        foreach (var (escolha, quero) in new[] { (0, 12345 * 7), (12, 12345 * 21), (6, (int)Math.Round(12345 * 10.5, MidpointRounding.ToEven)) })
        {
            // campeonato: tudo para o mandante
            var (m, casa, fora) = Mundo2((byte)escolha);
            var j = new Jogo { Taca = 0, Casa = casa.ID, Fora = fora.ID, D40 = 12345 };
            X(7).Bytes().CopyTo(j.B36, 0);
            m.FimDeJogo(j);
            Assert.Equal(quero, Mundo.Saldo(casa, Mundo.BalBilheteira));
            Assert.Equal(0, Mundo.Saldo(fora, Mundo.BalBilheteira));
            // o preco guardado no jogo nao muda (a tela mostra o de tabela)
            Assert.Equal(7.0, Ext.Bytes(j.B36).Float64());
        }
        // a escolha do visitante nao conta
        {
            var (m, casa, fora) = Mundo2(0);
            fora.Cabeca[4] = 12;
            var j = new Jogo { Taca = 0, Casa = casa.ID, Fora = fora.ID, D40 = 1000 };
            X(7).Bytes().CopyTo(j.B36, 0);
            m.FimDeJogo(j);
            Assert.Equal(7000, Mundo.Saldo(casa, Mundo.BalBilheteira));
        }
    }

    [Fact]
    public void GuardadoNoSave()
    {
        string? arq = null;
        for (var d = AppContext.BaseDirectory; d != null && arq == null; d = System.IO.Path.GetDirectoryName(d))
        {
            var f = System.IO.Path.Combine(d, "Oraculo", "j3-entrada.e98");
            if (System.IO.File.Exists(f)) arq = f;
        }
        Assert.NotNull(arq);
        var s = Jogo98.Ler(System.IO.File.ReadAllBytes(arq!));
        Assert.All(s.Equipas, e => Assert.Equal(0, e.Cabeca[4]));
        Ingressos.Escolher(s.Equipas[3], 225);
        var lido = Jogo98.Ler(s.Gravar());
        Assert.Equal(9, lido.Equipas[3].Cabeca[4]);
        Assert.Equal(225, Ingressos.Percentual(lido.Equipas[3]));
        Assert.Equal(0, lido.Equipas[2].Cabeca[4]);
    }

    // RetroFoot: o -/+ muda o preco de $1 em $1, de metade ate o triplo da tabela; o preco
    // mostrado e o cobrado; o publico cai com o preco (razao^-1,3, nos quartos a tabela)
    [Fact]
    public void PrecoDeUmEmUm()
    {
        var e = Time(1);
        var tabela = X(10);
        Assert.Equal(10, Ingressos.PrecoCom(tabela, Ingressos.Razao(e)));
        long antes = 10;
        for (int i = 0; i < 25; i++)
        {
            Ingressos.MudarPreco(e, tabela, 1);
            long p = Ingressos.PrecoCom(tabela, Ingressos.Razao(e));
            Assert.Equal(Math.Min(antes + 1, 30), p);
            Assert.Equal((double)p, Ingressos.Preco(tabela, e).Float64());
            antes = p;
        }
        Assert.Equal(300, Ingressos.Percentual(e));
        for (int i = 0; i < 30; i++) Ingressos.MudarPreco(e, tabela, -1);
        Assert.Equal(5, Ingressos.PrecoCom(tabela, Ingressos.Razao(e)));
        // a procura cai sempre com o preco e bate com a tabela nos quartos
        double ant = double.MaxValue;
        for (int r = Ingressos.RazaoMinima; r <= Ingressos.RazaoMaxima; r += 100)
        {
            double f = Ingressos.FatorProcura(r).Float64();
            Assert.True(f < ant);
            ant = f;
        }
        Assert.Equal(1.4535f, (float)Ingressos.FatorProcura(7500).Float64());
        // um preco de tabela quebrado (com inflacao): cada clique muda $1
        var t2 = X(137.6);
        var e2 = Time(2);
        long a2 = Ingressos.PrecoCom(t2, Ingressos.Razao(e2));
        for (int i = 0; i < 50; i++)
        {
            Ingressos.MudarPreco(e2, t2, 1);
            long p = Ingressos.PrecoCom(t2, Ingressos.Razao(e2));
            Assert.Equal(a2 + 1, p);
            a2 = p;
        }
        // o escolher em quartos (o launcher) apaga a razao fina
        Ingressos.Escolher(e2, 150);
        Assert.Equal(15000, Ingressos.Razao(e2));
    }
}
