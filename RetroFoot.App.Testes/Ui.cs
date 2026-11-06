using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using RetroFoot.App;
using RetroFoot.Motor.Base;
using RetroFoot.Motor.NovoJogo;
using RetroFoot.Motor.Save;
using Xunit;
using NovoOpcoes = RetroFoot.Motor.NovoJogo.Opcoes;

// (as telas mexem em estado global: o visual, o tema, o formato, o celular; um teste por vez)
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace RetroFoot.App.Testes;

sealed class AppTeste : Application
{
    public override void Initialize()
    {
        RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
        Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
    }
}

// O Avalonia sem tela numa thread so dele (a do Dispatcher): cada teste roda inteiro nela
// (Ui.Rodar), um de cada vez
static class Ui
{
    static Thread? _thread;
    static readonly object _trava = new();

    // a pasta de dados do jogo no repositorio (o jogo novo dos testes sai dela)
    public static string Dados { get; } = AcharDados();

    static string AcharDados()
    {
        for (var d = AppContext.BaseDirectory; d != null; d = Path.GetDirectoryName(d))
        {
            var p = Path.Combine(d, "RetroFoot.App", "Assets", "Dados");
            if (Directory.Exists(p)) return p;
        }
        throw new InvalidOperationException("RetroFoot.App/Assets/Dados nao encontrada");
    }

    public static void Rodar(Action a)
    {
        Iniciar();
        ExceptionDispatchInfo? erro = null;
        Dispatcher.UIThread.Invoke(() =>
        {
            try { a(); }
            catch (Exception e) { erro = ExceptionDispatchInfo.Capture(e); }
        });
        erro?.Throw();
    }

    static void Iniciar()
    {
        lock (_trava)
        {
            if (_thread != null) return;
            Isolar();
            using var pronto = new ManualResetEventSlim();
            Exception? falha = null;
            _thread = new Thread(() =>
            {
                try
                {
                    AppBuilder.Configure<AppTeste>().UseSkia()
                        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                        .WithInterFont().SetupWithoutStarting();
                }
                catch (Exception e) { falha = e; }
                pronto.Set();
                if (falha == null) Dispatcher.UIThread.MainLoop(CancellationToken.None);
            }) { IsBackground = true, Name = "Avalonia dos testes" };
            _thread.Start();
            pronto.Wait();
            if (falha != null) throw new InvalidOperationException("Avalonia sem tela nao iniciou", falha);
        }
    }

    // nada dos testes toca nas Configuracoes e nos saves de verdade: a pasta de dados (o modo
    // portatil, DadosJogo) e a da saida dos testes, comecando sem config.json; as pastas do
    // sistema (de onde a 1a execucao copiaria) apontam para uma pasta temporaria vazia
    static void Isolar()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "retrofoot-app-testes-" + Environment.ProcessId);
        Directory.CreateDirectory(tmp);
        Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", tmp);
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", tmp);
        Environment.SetEnvironmentVariable("RETROFOOT_JOGO", Dados);
        foreach (var v in new[] { "RETROFOOT_VISUAL", "RETROFOOT_TEMA", "RETROFOOT_CELULAR", "RETROFOOT_SECAO", "RETROFOOT_PAGINA" })
            Environment.SetEnvironmentVariable(v, null);
        var cfg = Path.Combine(DadosJogo.PastaPortatil, "config.json");
        if (File.Exists(cfg)) File.Delete(cfg);
    }

    // espera a tela assentar (layout, timers que ja venceram)
    public static void Assentar(Window w)
    {
        for (int i = 0; i < 20; i++) Dispatcher.UIThread.RunJobs();
        w.UpdateLayout();
        for (int i = 0; i < 5; i++) Dispatcher.UIThread.RunJobs();
    }

    // ------------------------------------------------------------------ o jogo dos testes
    static byte[]? _save;

    // um jogo novo de 8 equipas por divisao (o mesmo em todos os testes), na 1a jornada, com um
    // tecnico humano
    public static Jogo98 JogoNovo()
    {
        if (_save == null)
        {
            var cfg = new Configuracoes { Modificada = true };
            var d = RetroFoot.Motor.NovoJogo.Dados.Ler(Dados);
            var sel = d.ContarEquipas();
            sel.Minimo = cfg.RegrasDoJogoNovo.MinimoEquipas;
            sel.Todos();
            sel.Confirmar(new Aleatorio(7919u));
            var g = new Aleatorio(104730u);
            var e = Criacao.Criar(d, new NovoOpcoes
            {
                Moeda = Tabelas.MoedaReais, Ano = 2026, Regras = cfg.RegrasDoJogoNovo,
                Humanos = new() { "Teste" },
            }, g);
            e.IniciarTemporada();
            e.PrepararJornada(1, g);
            _save = e.Save(Exportar.DataHora(new DateTime(2026, 11, 6, 12, 0, 0))).Gravar();
        }
        return Jogo98.Ler(_save);
    }
}
