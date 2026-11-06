using System;
using System.IO;
using System.Linq;
using Xunit;

namespace RetroFoot.Motor.Testes;

// Os testes que precisam de arquivos do jogo original de 1998 (que nao vao para o git): sem
// eles, o teste aparece como ignorado (com o motivo), nao como falha. Assim uma falha na suite
// e sempre um problema de verdade. Com os arquivos (RETROFOOT_JOGO, Oraculo/, Patrocinio/), rodam.
static class Requisitos
{
    // pasta/arquivo procurado a partir da pasta do teste, subindo (como os testes fazem)
    public static string? Achar(string pasta, string arquivo)
    {
        var d = AppContext.BaseDirectory;
        for (int i = 0; i < 8 && d != null; i++, d = Path.GetDirectoryName(d))
        {
            var f = Path.Combine(d, pasta, arquivo);
            if (File.Exists(f)) return f;
        }
        return null;
    }
}

// a pasta do jogo original (Arquivos.Raiz: RETROFOOT_JOGO ou a pasta game ao lado do projeto)
public sealed class FactComOriginalAttribute : FactAttribute
{
    public FactComOriginalAttribute()
    {
        if (Arquivos.Raiz() == null) Skip = "precisa da pasta do jogo original (RETROFOOT_JOGO)";
    }
}

// um arquivo gravado pelo jogo original ou pelo launcher (Oraculo/, Patrocinio/)
public sealed class FactComArquivoAttribute : FactAttribute
{
    public FactComArquivoAttribute(string pasta, string arquivo)
    {
        if (Requisitos.Achar(pasta, arquivo) == null) Skip = $"precisa de {pasta}/{arquivo} (gravado pelo jogo original)";
    }
}

// os casos do oraculo (Oraculo/*-original.e98)
public sealed class TheoryDoOraculoAttribute : TheoryAttribute
{
    public TheoryDoOraculoAttribute()
    {
        if (!OraculoTestes.Casos().Any()) Skip = "precisa dos casos do oraculo (Oraculo/*-original.e98, gravados pelo jogo original)";
    }
}
