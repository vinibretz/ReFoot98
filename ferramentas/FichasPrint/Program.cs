using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using RetroFoot.App;
using RetroFoot.Motor.Partida;
using RetroFoot.Motor.Save;

// uso: FichasPrint <save.e98> <pasta de saida> [plantel|leilao|equipa|procura|config]
// (RETROFOOT_VISUAL=classico|moderno, RETROFOOT_TEMA=verde|escuro|claro, RETROFOOT_CELULAR=1
// para o celular, RETROFOOT_FORMATO=43|169 no computador; equipa: RETROFOOT_PAGINA=jogador|
// financas|arbitro; config: abre na secao Regras)
// As opcoes da versao Atual que vinham com os amarelos, para conferir as combinacoes:
// RETROFOOT_AMARELOS, RETROFOOT_NOTA, RETROFOOT_INGRESSO, RETROFOOT_CAMPINHO = 0|1 (sem a
// variavel, a do config.json). Com qualquer uma delas, a versao Atual.
sealed class App0 : Application
{
    public override void Initialize()
    {
        RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
        Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
    }
}

static class Programa
{
    static bool? Flag(string nome) => Environment.GetEnvironmentVariable(nome) switch { "1" => true, "0" => false, _ => null };

    static int Main(string[] a)
    {
        AppBuilder.Configure<App0>().UseSkia().UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).WithInterFont().SetupWithoutStarting();
        var s = Jogo98.Ler(File.ReadAllBytes(a[0]));
        string modo = a.Length > 2 ? a[2] : "leilao";
        bool cel = Environment.GetEnvironmentVariable("RETROFOOT_CELULAR") == "1";
        bool largo = Environment.GetEnvironmentVariable("RETROFOOT_FORMATO") is string fm ? fm == "169" : Formato.Largo;
        if (!cel) Formato.Aplicar(largo);

        var cfg = Configuracoes.Ler();
        var fl = new[] { Flag("RETROFOOT_AMARELOS"), Flag("RETROFOOT_NOTA"), Flag("RETROFOOT_INGRESSO"), Flag("RETROFOOT_CAMPINHO") };
        if (fl.Any(x => x != null)) cfg.Modificada = true;
        cfg.CartoesAmarelos = fl[0] ?? cfg.CartoesAmarelos;
        cfg.NotaERisco = fl[1] ?? cfg.NotaERisco;
        cfg.PrecoIngresso = fl[2] ?? cfg.PrecoIngresso;
        cfg.CampinhoArbitro = fl[3] ?? cfg.CampinhoArbitro;

        var w = new Window { Width = cel ? 420 : largo ? 1280 : 1024, Height = cel ? 860 : largo ? 720 : 768 };
        var raiz = new Border();
        w.Content = raiz;
        w.Show();
        var sessao = new Sessao(s, c => raiz.Child = c, cfg.OpcoesDoJogo(s, Environment.GetEnvironmentVariable("RETROFOOT_JOGO") ?? ""));
        sessao.Mostrar(new Border { Background = Avalonia.Media.Brushes.DarkGreen });
        var humano = s.Equipas.First(e => s.Treinadores.Any(t => t.ID == e.Treinador && t.Humano != 0));
        // um atacante de outro time com gols, e uns jogos na temporada para a media
        var (v, p) = s.Equipas.Where(e => e != humano).SelectMany(e => e.Jogadores.Select(j => (e, j)))
            .OrderByDescending(x => Memoria.Historial(x.j, Memoria.HistGolos)).First();
        for (int i = 0; i < 12; i++) Memoria.SomaJogoNaTemporada(p);
        if (p.B14[0] == 0) p.B14[0] = 5;
        switch (modo)
        {
            case "plantel": TelaPlantel.Abrir(sessao, v, humano, p.ID); break;
            case "equipa":
            {
                var m = sessao.J.M;
                var jogo = m.S.Jogos.FirstOrDefault(j => j.Casa == humano.ID || j.Fora == humano.ID);
                if (jogo == null)
                {
                    m.LimparJogos();
                    for (int d = 0; d <= 3 && d < m.S.Divisoes.Count; d++) m.CriarJogosDivisao(d);
                    jogo = m.S.Jogos.FirstOrDefault(j => j.Casa == humano.ID || j.Fora == humano.ID);
                }
                sessao.Mostrar(new TelaEquipa(sessao, humano, jogo));
                break;
            }
            case "procura": TelaProcura.Abrir(sessao, humano); break;
            case "config": Environment.SetEnvironmentVariable("RETROFOOT_SECAO", "1"); sessao.Mostrar(new TelaConfiguracoes(cfg)); break;
            default: TelaLeilao.DeTodos.Abrir(sessao, v, p, 1_234_000); break;
        }
        for (int i = 0; i < 20; i++) Dispatcher.UIThread.RunJobs();
        var f = w.CaptureRenderedFrame();
        string visual = cel ? "cel-" + (Environment.GetEnvironmentVariable("RETROFOOT_VISUAL") ?? "padrao")
            : (Environment.GetEnvironmentVariable("RETROFOOT_VISUAL") ?? "padrao") + "-" + (largo ? "169" : "43");
        if (Environment.GetEnvironmentVariable("RETROFOOT_TEMA") is { Length: > 0 } tema) visual += "-" + tema;
        string pagina = Environment.GetEnvironmentVariable("RETROFOOT_PAGINA") is { Length: > 0 } pg && modo == "equipa" ? "-" + pg : "";
        string combo = $"A{(cfg.CartoesAmarelos ? 1 : 0)}N{(cfg.NotaERisco ? 1 : 0)}I{(cfg.PrecoIngresso ? 1 : 0)}C{(cfg.CampinhoArbitro ? 1 : 0)}";
        var nome = Path.Combine(a[1], $"{modo}{pagina}-{visual}-{combo}.png");
        f!.Save(nome);
        Console.WriteLine(nome);
        return 0;
    }
}
