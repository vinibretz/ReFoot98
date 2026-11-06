using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.VisualTree;
using AVisual = Avalonia.Visual;

namespace RetroFoot.App.Testes;

// um texto desenhado na tela: a caixa e o recorte (o que os controles de cima deixam ver) nas
// coordenadas da janela
public sealed record Texto(string Valor, Rect Caixa, Rect Recorte, AVisual Dono, int Ordem)
{
    // coberto por um fundo opaco desenhado depois (a pagina do celular por cima da anterior, um
    // dialogo por cima da tela)
    public bool Coberto { get; internal set; }
    // aparece (ao menos em parte) na janela
    public bool Visivel => !Coberto && Caixa.Intersects(Recorte);
    public override string ToString() => $"\"{Valor}\" em {Caixa.X:F0},{Caixa.Y:F0} ({Caixa.Width:F0}x{Caixa.Height:F0})";
}

// Le de volta os textos que as telas desenham, dos controles (TextBlock...) e das telas
// desenhadas a mao (o Classico, as fichas: DrawingContext.DrawText): cada controle e desenhado
// sozinho num DrawingGroup e os GlyphRuns dele vao para as coordenadas da janela
public static class Inspecao
{
    // os controles cujo Render nao deu para gravar (o motivo), da ultima leitura
    public static readonly List<string> Falhas = new();

    // O contexto do DrawingGroup (DrawingGroupDrawingContext) lanca NotImplementedException nas
    // imagens (DrawBitmap) e no recorte arredondado (PushClipCore(RoundedRect)): sem o remendo, um
    // controle com uma imagem perdia todos os textos. As imagens sao puladas; o recorte arredondado
    // vira o retangular (os textos nao precisam dos cantos)
    static bool _remendado;

    static void Remendar()
    {
        if (_remendado) return;
        _remendado = true;
        var t = typeof(DrawingGroup).GetNestedTypes(System.Reflection.BindingFlags.NonPublic).First(x => x.Name == "DrawingGroupDrawingContext");
        var h = new HarmonyLib.Harmony("retrofoot.app.testes");
        var fl = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        h.Patch(t.GetMethod("DrawBitmap", fl), prefix: new HarmonyLib.HarmonyMethod(typeof(Inspecao), nameof(Pular)));
        h.Patch(t.GetMethod("PushClipCore", fl, new[] { typeof(RoundedRect) }), prefix: new HarmonyLib.HarmonyMethod(typeof(Inspecao), nameof(ClipReto)));
    }

    static bool Pular() => false;

    static bool ClipReto(object __instance, RoundedRect rect)
    {
        var fl = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        __instance.GetType().GetMethod("PushClipCore", fl, new[] { typeof(Rect) })!.Invoke(__instance, new object[] { rect.Rect });
        return false;
    }

    public static List<Texto> Textos(AVisual raiz)
    {
        Remendar();
        Falhas.Clear();
        var l = new Leitura();
        var janela = new Rect(raiz.Bounds.Size);
        Andar(raiz, raiz, janela, 1, l);
        // um texto e coberto por um fundo opaco desenhado depois dele que tapa ao menos 80% da parte
        // visivel dele (o resto, uma lasca de letra na borda do dialogo, nao se le)
        foreach (var t in l.Textos)
        {
            var vis = t.Caixa.Intersect(t.Recorte);
            double area = vis.Width * vis.Height;
            if (area <= 0) continue;
            t.Coberto = l.Fundos.Any(f => f.Ordem > t.Ordem && f.Caixa.Intersect(vis) is var x && x.Width * x.Height >= 0.8 * area);
        }
        return l.Textos;
    }

    sealed class Leitura
    {
        public readonly List<Texto> Textos = new();
        public readonly List<(int Ordem, Rect Caixa)> Fundos = new();
        public int Ordem;
    }

    static void Andar(AVisual raiz, AVisual v, Rect recorte, double opacidade, Leitura r)
    {
        if (!v.IsVisible || v.Opacity <= 0) return;
        opacidade *= v.Opacity;
        var m = v.TransformToVisual(raiz);
        if (m == null) return;
        if (v.ClipToBounds) recorte = recorte.Intersect(new Rect(v.Bounds.Size).TransformToAABB(m.Value));
        var dg = new DrawingGroup();
        try
        {
            using (var ctx = dg.Open()) v.Render(ctx);
            Coletar(dg, m.Value, recorte, opacidade, v, r);
        }
        catch (Exception e) { Falhas.Add($"{v.GetType().Name}: {e.GetType().Name} {e.Message} @ {e.StackTrace?.Split((char)10).FirstOrDefault()?.Trim()}"); }
        foreach (var c in v.GetVisualChildren()) Andar(raiz, c, recorte, opacidade, r);
    }

    static void Coletar(Drawing d, Matrix t, Rect recorte, double opacidade, AVisual dono, Leitura r)
    {
        switch (d)
        {
            case DrawingGroup g:
                var t2 = (g.Transform?.Value ?? Matrix.Identity) * t;
                if (g.ClipGeometry is { } cg) recorte = recorte.Intersect(cg.Bounds.TransformToAABB(t2));
                foreach (var c in g.Children) Coletar(c, t2, recorte, opacidade * g.Opacity, dono, r);
                break;
            // um fundo opaco (cor solida ou degrade sem transparencia): cobre o que foi desenhado antes
            case GeometryDrawing gd when Opaco(gd.Brush) && opacidade >= 1 && gd.Geometry is { } geo:
                var f = geo.Bounds.TransformToAABB(t).Intersect(recorte);
                if (f.Width > 0 && f.Height > 0) r.Fundos.Add((r.Ordem++, f));
                break;
            case GlyphRunDrawing gr when gr.GlyphRun is { } run:
                var s = run.Characters.ToString();
                if (string.IsNullOrWhiteSpace(s)) break;
                // (Bounds: a caixa da linha, com o espaco acima e abaixo das letras; InkBounds: so o
                // que as letras pintam; os dois ja nas coordenadas do controle)
                var caixa = run.InkBounds;
                r.Textos.Add(new Texto(s.TrimEnd(), caixa.TransformToAABB(t), recorte, dono, r.Ordem++));
                break;
        }
    }

    static bool Opaco(IBrush? b) => b switch
    {
        ISolidColorBrush sb => sb.Color.A == 255 && sb.Opacity >= 1,
        IGradientBrush gb => gb.Opacity >= 1 && gb.GradientStops.Count > 0 && gb.GradientStops.All(x => x.Color.A == 255),
        _ => false,
    };

    // o texto da tela inteira, juntando as linhas (para procurar frases quebradas)
    public static string Tudo(IEnumerable<Texto> l) => string.Join(" ", l.Select(x => x.Valor));

    public static bool Tem(IEnumerable<Texto> l, string s) =>
        l.Any(x => x.Valor.Contains(s, StringComparison.Ordinal)) || Tudo(l).Contains(s, StringComparison.Ordinal);

    // textos visiveis que se sobrepoem (os dois com mais de 3 px de cada lado em comum); o mesmo
    // texto desenhado duas vezes (o cinza com sombra do Win95) nao conta
    public static List<(Texto A, Texto B)> Sobrepostos(IEnumerable<Texto> l)
    {
        var v = l.Where(x => x.Visivel && x.Caixa.Width > 2).Select(x => (T: x, C: x.Caixa.Intersect(x.Recorte))).ToList();
        var r = new List<(Texto, Texto)>();
        for (int i = 0; i < v.Count; i++)
            for (int j = i + 1; j < v.Count; j++)
            {
                if (v[i].T.Valor == v[j].T.Valor) continue;
                var x = v[i].C.Intersect(v[j].C);
                if (x.Width > 3 && x.Height > 3) r.Add((v[i].T, v[j].T));
            }
        return r;
    }

    // textos visiveis cortados na horizontal pelo recorte (o fim ou o comeco fora); na vertical
    // e a rolagem, que pode deixar uma linha pela metade
    public static List<Texto> CortadosNaLargura(IEnumerable<Texto> l) =>
        l.Where(x => x.Visivel && x.Caixa.Height > 2
            && (x.Caixa.Right > x.Recorte.Right + 2 || x.Caixa.X < x.Recorte.X - 2)
            && x.Caixa.Bottom > x.Recorte.Y + 2 && x.Caixa.Y < x.Recorte.Bottom - 2).ToList();
}
