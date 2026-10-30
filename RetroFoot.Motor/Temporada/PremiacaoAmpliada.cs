using System;

namespace RetroFoot.Motor.Temporada;

// A opcao "Premiacao ampliada" (Regras.PremiacaoAmpliada; docs/premiacao.md), so do RetroFoot:
// as K primeiras de cada divisao (1a a 4a) recebem uma fracao do premio do campeao da moeda,
// pela divisao (FatorDivisao) e pela posicao ((K - i + 1) / K); o vice da Taca recebe metade
// do premio da Taca. Sem inflacao, como os premios do original; arredondado ao milhar.
public static class PremiacaoAmpliada
{
    // 1a, 2a, 3a e 4a divisao; o Distrital nao premia (sobe por sorteio)
    public static readonly double[] FatorDivisao = { 1.0, 0.40, 0.15, 0.05 };

    // divisao 0..3, posicao 1..k; fora disso, 0
    public static int Valor(int premioCampeao, int divisao, int posicao, int k)
    {
        if (divisao < 0 || divisao >= FatorDivisao.Length || k < 1 || posicao < 1 || posicao > k) return 0;
        return Milhar((decimal)premioCampeao * (decimal)FatorDivisao[divisao] * (k - posicao + 1) / k);
    }

    public static int ViceTaca(int premioTaca) => Milhar(premioTaca / 2m);

    static int Milhar(decimal v) => (int)(Math.Round(v / 1000m, MidpointRounding.AwayFromZero) * 1000m);
}
