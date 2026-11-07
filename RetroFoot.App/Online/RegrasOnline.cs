using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia.Media;
using RetroFoot.Motor.Online;
using CicloOpcoes = RetroFoot.Motor.Ciclo.Opcoes;
using static RetroFoot.App.Idioma;

namespace RetroFoot.App;

// As regras de um jogo online na tela da sala (docs/multiplayer-online.md): as do jogo, nao as
// Configuracoes de cada aparelho. O host ve e acerta as que podem mudar no meio do jogo; a
// versao (Retro ou Atual) e os acrescimos ficam como o jogo foi criado. Os convidados veem o
// resumo. O JSON e o de info/opcoes (Sala.OpcoesJson).
public static class RegrasOnline
{
    // as regras com que o jogo vai abrir: as gravadas com o jogo (regras.json, TelaInicio.RegrasFixasOnline)
    // ou, sem elas (jogo novo, ou de antes desta versao), as das Configuracoes
    public static JsonObject Iniciais(string codigo, string? arquivo)
    {
        if (arquivo != null)
        {
            try
            {
                var arq = Path.Combine(Path.GetDirectoryName(arquivo)!, "regras.json");
                if (File.Exists(arq) && JsonNode.Parse(File.ReadAllText(arq)) is JsonObject o && Sala.OpcoesCompletas(o))
                {
                    // (gravadas antes do intervalo virar regra: o que o host tinha nas Opcoes)
                    o["intervalo"] ??= OpcoesDoJogo.Preferencias.SubstituirNoIntervalo;
                    return o;
                }
            }
            catch { }
        }
        var cfg = Configuracoes.Ler();
        var pasta = TelaInicio.PastaDoJogo();
        CicloOpcoes op;
        try
        {
            op = arquivo != null && File.Exists(arquivo)
                ? cfg.OpcoesDoJogo(RetroFoot.Motor.Save.Jogo98.Ler(File.ReadAllBytes(arquivo)), pasta)
                : NovoJogo(cfg, pasta);
        }
        catch { op = NovoJogo(cfg, pasta); }
        // (os penaltis dos jogos sem humanos e o intervalo: o que o host tinha nas Opcoes)
        var pref = OpcoesDoJogo.Preferencias;
        return Sala.OpcoesJson(op with { VerPenaltisCPU = pref.VerPenaltisCPU, SubstituirNoIntervalo = pref.SubstituirNoIntervalo });
    }

    static CicloOpcoes NovoJogo(Configuracoes cfg, string? pasta)
    {
        var (bosman, plop) = cfg.ListasEstrangeiros(pasta);
        var r = cfg.RegrasDoJogoNovo;
        return CicloOpcoes.Padrao with { Regras = r, Patrocinios = r.Tatica505 && cfg.Patrocinios, Bosman = bosman, PLOP = plop };
    }

    static bool B(JsonNode? o, string k) => o?[k] is JsonValue v && v.TryGetValue<bool>(out var b) && b;
    static int I(JsonNode? o, string k) => o?[k] is JsonValue v && v.TryGetValue<int>(out var i) ? i : 0;
    static bool Atual(JsonNode? o) => B(o, "tatica505");

    // as listas dos estrangeiros com a opcao liberada ou nao (as da pasta do jogo deste aparelho)
    static string? Listas(bool liberado)
    {
        var c = Configuracoes.Ler().Copia();
        c.EstrangeirosLiberados = liberado;
        var (bosman, _) = c.ListasEstrangeiros(TelaInicio.PastaDoJogo());
        return bosman == null ? null : "," + string.Join(",", bosman);
    }

    // estrangeiros liberados? (null: o host nao mandou as listas)
    static bool? Liberados(JsonNode? o)
    {
        if (o?["bosman"] is not JsonValue v || !v.TryGetValue<string>(out var b)) return null;
        var lib = Listas(true);
        return lib != null && Iguais(b, lib);
    }

    static bool Iguais(string a, string b) =>
        a.Split(',', StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x).SequenceEqual(b.Split(',', StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x));

    // o resumo numa linha (os convidados; e o que nao muda, no host)
    public static string Resumo(JsonNode? o)
    {
        if (!Sala.OpcoesCompletas(o)) return T("O host ainda não definiu as regras.");
        var p = new List<string>();
        if (!Atual(o)) p.Add(T("Versão Retrô (como em 1998)"));
        else
        {
            p.Add(T("Versão Atual"));
            p.Add(B(o, "amarelos") ? T("amarelos e vermelhos") : T("só vermelhos"));
            p.Add(B(o, "nota") ? T("nota e risco ligados") : T("nota e risco desligados"));
            p.Add(B(o, "ingresso") ? T("preço do ingresso escolhido") : T("ingresso a preço de tabela"));
            p.Add(B(o, "campinho") ? T("campinho ligado") : T("campinho desligado"));
            p.Add(B(o, "patrocinios") ? T("patrocínios ligados") : T("patrocínios desligados"));
            p.Add(B(o, "acrescimos") ? T("acréscimos ligados") : T("acréscimos desligados"));
            p.Add(T("{0} substituições", I(o, "substituicoes") > 0 ? I(o, "substituicoes") : 3));
            p.Add(T("{0} reservas", I(o, "reservas") > 0 ? I(o, "reservas") : 5));
        }
        if (Liberados(o) is bool lib) p.Add(lib ? T("estrangeiros liberados") : T("estrangeiros realísticos"));
        if (o?["intervalo"] != null) p.Add(B(o, "intervalo") ? T("substituições no intervalo") : T("sem parar no intervalo"));
        if (o?["penaltisCpu"] != null) p.Add(B(o, "penaltisCpu") ? T("pênaltis da CPU cobrados um a um") : T("pênaltis da CPU decididos na hora"));
        if (DoSave(o) is { } s) p.Add(s);
        return string.Join(" · ", p);
    }

    // o que fica gravado no save na criacao: as equipas por divisao, quantas sobem e descem e a
    // premiacao (null: um host de antes destas chaves)
    static string? DoSave(JsonNode? o) => I(o, "equipas") <= 0 ? null
        : T("{0} equipas por divisão", I(o, "equipas")) + " · " + T("sobem e descem {0}", I(o, "sobem"))
            + " · " + (B(o, "premiacao") ? T("premiação ampliada") : T("premiação clássica"));

    // o que nao muda no editor: a versao, os acrescimos, as equipas e quantas sobem e descem (a
    // premiacao so na versao Retro, que nao tem a ampliada)
    static List<string> Fixas(JsonNode? o)
    {
        var l = new List<string>();
        if (Atual(o))
        {
            l.Add(T("versão Atual"));
            l.Add(B(o, "acrescimos") ? T("acréscimos ligados") : T("acréscimos desligados"));
        }
        else l.Add(T("versão Retrô (sem as opções da versão Atual)"));
        if (I(o, "equipas") > 0)
        {
            l.Add(T("{0} equipas por divisão", I(o, "equipas")));
            l.Add(T("sobem e descem {0}", I(o, "sobem")));
            if (!Atual(o)) l.Add(T("premiação clássica"));
        }
        return l;
    }

    // a explicacao do cartao (didatica e curta)
    public static string Explicacao(bool host, bool novo) => host
        ? (novo
            ? T("Estas regras valem só para este jogo e são iguais para todos os técnicos. Quando o jogo começar, elas ficam gravadas nele: mudar as Configurações do aparelho depois não muda o jogo. As opções abaixo você pode mudar sempre que abrir a sala, antes de começar ou continuar.")
            : T("Estas regras são deste jogo e valem para todos os técnicos. Mudar as Configurações do aparelho não muda o jogo: para mudar uma regra, use as opções abaixo antes de continuar."))
        : T("O host define as regras do jogo, e elas valem para todos: as Configurações do seu aparelho não mudam nada aqui. O host só pode mudar uma regra nesta tela, antes de começar ou continuar o jogo.");

    // o editor do host: muda o JSON r e chama mudou. novo: um jogo que ainda vai ser criado (o que
    // nao muda vem das Configuracoes, e ainda da para trocar la)
    public static Control Editor(JsonObject r, bool novo, Action mudou)
    {
        var painel = new StackPanel { Spacing = 8 };
        string lista = string.Join("; ", Fixas(r));
        painel.Children.Add(CUi.Txt(novo
            ? T("Vêm das suas Configurações e ficam fixas quando o jogo começar: {0}. Para mudar alguma delas, saia da sala, ajuste no menu Configurações e crie a sala de novo.", lista)
            : T("Não mudam neste jogo (ficaram fixas quando ele foi criado): {0}.", lista),
            13, CUi.Fraco, Avalonia.Media.FontWeight.Normal, true));
        void Mudar(string k, JsonNode? v) { r[k] = v; mudou(); }
        if (Atual(r))
        {
            painel.Children.Add(CUi.Interruptor(T("Cartões amarelos"), null, B(r, "amarelos"), v => Mudar("amarelos", v)));
            painel.Children.Add(CUi.Interruptor(T("Nota e risco de lesão"), null, B(r, "nota"), v => Mudar("nota", v)));
            painel.Children.Add(CUi.Interruptor(T("Preço do ingresso"), null, B(r, "ingresso"), v => Mudar("ingresso", v)));
            painel.Children.Add(CUi.Interruptor(T("Campinho e árbitro"), null, B(r, "campinho"), v => Mudar("campinho", v)));
            painel.Children.Add(CUi.Interruptor(T("Patrocínios"), null, B(r, "patrocinios"), v => Mudar("patrocinios", v)));
            painel.Children.Add(CUi.Interruptor(T("5 substituições"), T("Desligado: 3."), I(r, "substituicoes") == 5, v => Mudar("substituicoes", v ? 5 : 3)));
            painel.Children.Add(CUi.Interruptor(T("9 reservas"), T("Desligado: 5."), I(r, "reservas") == 9, v => Mudar("reservas", v ? 9 : 5)));
            if (I(r, "equipas") > 0)
                painel.Children.Add(CUi.Interruptor(T("Premiação ampliada"), T("Prêmios também para as primeiras de cada divisão e para o vice da copa, pagos no fim da temporada. Desligado: a clássica (campeão, copa e artilheiro)."), B(r, "premiacao"), v => Mudar("premiacao", v)));
        }
        // o intervalo (nas duas versoes): para todos ou para ninguem (o jogo para em todos os aparelhos)
        painel.Children.Add(CUi.Interruptor(T("Substituições no intervalo"), T("Ligado: no intervalo o jogo para 30 segundos para todos os técnicos mexerem no time. Desligado: o jogo segue direto."), B(r, "intervalo"), v => Mudar("intervalo", v)));
        // os penaltis dos jogos sem tecnico humano (nas duas versoes): mudam o resultado
        painel.Children.Add(CUi.Interruptor(T("Pênaltis da CPU cobrados um a um"), T("Nos jogos da copa sem técnico humano. Desligado: decididos na hora, sem a disputa. Ligado: a disputa cobrança a cobrança, que aparece na tela (e pode dar outro vencedor)."), B(r, "penaltisCpu"), v => Mudar("penaltisCpu", v)));
        // os estrangeiros (nas duas versoes), se este aparelho tem as listas
        if (Liberados(r) is bool lib && Listas(true) is { } listaLib && Listas(false) is { } listaReal)
        {
            painel.Children.Add(CUi.Txt(T("Jogadores estrangeiros"), 13, CUi.Texto, Avalonia.Media.FontWeight.SemiBold));
            var (esc, _) = CUi.Escolha(new[] { T("Realístico"), T("Liberado") }, lib ? 1 : 0, i => Mudar("bosman", i == 1 ? listaLib : listaReal));
            painel.Children.Add(esc);
        }
        return painel;
    }
}
