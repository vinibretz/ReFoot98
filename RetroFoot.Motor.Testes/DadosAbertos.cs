using System;
using System.Collections.Generic;
using System.IO;
using RetroFoot.Motor.Base;
using RetroFoot.Motor.NovoJogo;
using RetroFoot.Motor.Save;
using TRegras = RetroFoot.Motor.Regras.Regras;

namespace RetroFoot.Motor.Testes;

// a pasta de dados aberta do RetroFoot (RetroFoot.App/Assets/Dados), para testes que nao
// dependem do jogo original
static class DadosAbertos
{
    public static string Pasta()
    {
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10 && dir != null; i++, dir = Path.GetDirectoryName(dir))
        {
            var p = Path.Combine(dir, "RetroFoot.App", "Assets", "Dados");
            if (Directory.Exists(p)) return p;
        }
        throw new DirectoryNotFoundException("RetroFoot.App/Assets/Dados");
    }

    // um jogo novo (Reais, um humano) com as regras dadas, como o da App
    public static Jogo98 JogoNovo(TRegras regras, uint semente = 1)
    {
        var d = Dados.Ler(Pasta());
        var sel = d.ContarEquipas();
        sel.Minimo = regras.MinimoEquipas;
        sel.Todos();
        sel.Confirmar(new Aleatorio(semente * 7919u));
        var g = new Aleatorio(semente * 104729u + 1);
        var e = Criacao.Criar(d, new Opcoes { Moeda = Tabelas.MoedaReais, Ano = 2026, Humanos = new List<string> { "Teste" }, Regras = regras }, g);
        e.IniciarTemporada();
        e.PrepararJornada(1, g);
        return Jogo98.Ler(e.Save(Exportar.DataHora(new DateTime(2026, 1, 1))).Gravar());
    }
}
