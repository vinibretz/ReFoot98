using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using RetroFoot.Motor.Temporada;
using Chicotada = RetroFoot.Motor.Rodada.Chicotada;

namespace RetroFoot.App;

// O que vai com as janelas que so informam (HostOnline.PublicarJanela, rodada/janela) e volta nos
// outros aparelhos (JogoRemoto.MostrarJanela): a lista das demissoes de tecnicos e o resultado
// do fim de temporada. (O banco nao guarda lista vazia: a que falta e vazia.)
static class JanelasOnline
{
    public static JsonObject Chicotadas(IEnumerable<Chicotada> l) => new()
    {
        ["lista"] = new JsonArray(l.Select(c => (JsonNode)new JsonObject
        {
            ["t"] = (int)c.Treinador, ["e"] = (int)c.Equipa, ["tipo"] = (int)c.Tipo, ["flag"] = (int)c.Flag,
        }).ToArray()),
    };

    // No online o motor junta todas as demissoes (OpcoesDoJogo.NoJogoOnline): cada aparelho
    // mostra o que a propria opcao pede (0 nunca, 1 dos humanos, 2 todas).

    // as demissoes que a opcao mostra: cada uma sao tres linhas (o antigo, o novo e a de fim,
    // com o treinador 0); "dos humanos" = um dos dois tecnicos e humano
    public static List<Chicotada> FiltrarChicotadas(RetroFoot.Motor.Save.Jogo98 s, IEnumerable<Chicotada> l, short opcao)
    {
        var r = new List<Chicotada>();
        if (opcao <= 0) return r;
        var grupo = new List<Chicotada>();
        bool Humano(ushort t) => t != 0 && s.Treinadores.Any(x => x.ID == t && x.Humano != 0);
        foreach (var c in l)
        {
            grupo.Add(c);
            if (c.Treinador != 0 || c.Equipa != 0) continue;
            if (opcao >= 2 || grupo.Any(x => Humano(x.Treinador))) r.AddRange(grupo);
            grupo.Clear();
        }
        if (grupo.Count > 0 && (opcao >= 2 || grupo.Any(x => Humano(x.Treinador)))) r.AddRange(grupo);
        return r;
    }

    public static List<Chicotada> ChicotadasDe(JsonObject o) =>
        Itens(o["lista"]).Select(x => new Chicotada((ushort)((int?)x["t"] ?? 0), (ushort)((int?)x["e"] ?? 0),
            (byte)((int?)x["tipo"] ?? 0), (byte)((int?)x["flag"] ?? 0))).ToList();

    public static JsonObject FimDeTemporada(Resultado r) => new()
    {
        ["campeao"] = (int)r.Campeao,
        ["taca"] = (int)r.VencedorTaca,
        ["subiram"] = Divisoes(r.Subiram),
        ["desceram"] = Divisoes(r.Desceram),
        ["premios"] = new JsonArray(r.Premios.Select(p => (JsonNode)new JsonObject
        {
            ["d"] = p.Divisao, ["p"] = p.Posicao, ["e"] = (int)p.Equipa, ["v"] = p.Valor,
        }).ToArray()),
    };

    public static Resultado ResultadoDe(JsonObject o)
    {
        var r = new Resultado { Campeao = (ushort)((int?)o["campeao"] ?? 0), VencedorTaca = (ushort)((int?)o["taca"] ?? 0) };
        LerDivisoes(o["subiram"], r.Subiram);
        LerDivisoes(o["desceram"], r.Desceram);
        foreach (var p in Itens(o["premios"]))
            r.Premios.Add(new Premio((int?)p["d"] ?? 0, (int?)p["p"] ?? 0, (ushort)((int?)p["e"] ?? 0), (int?)p["v"] ?? 0));
        return r;
    }

    // as divisoes num texto ("12,40;;7": as equipas de cada uma, separadas por ";"), porque o
    // banco tira as listas vazias e as do meio mudariam de divisao
    static string Divisoes(List<ushort>[] d) => string.Join(";", d.Select(l => string.Join(",", l)));

    static void LerDivisoes(JsonNode? n, List<ushort>[] destino)
    {
        var partes = ((string?)n ?? "").Split(';');
        for (int i = 0; i < partes.Length && i < destino.Length; i++)
            foreach (var x in partes[i].Split(',', System.StringSplitOptions.RemoveEmptyEntries))
                if (ushort.TryParse(x, out var id)) destino[i].Add(id);
    }

    // a lista do banco: array, ou objeto com as chaves 0, 1... (quando falta um item)
    static IEnumerable<JsonObject> Itens(JsonNode? n) => n switch
    {
        JsonArray a => a.Select(x => x as JsonObject ?? new JsonObject()),
        JsonObject o => o.OrderBy(kv => int.TryParse(kv.Key, out int k) ? k : int.MaxValue).Select(kv => kv.Value as JsonObject ?? new JsonObject()),
        _ => Enumerable.Empty<JsonObject>(),
    };
}
