using System;
using Avalonia;
using Avalonia.Controls;

namespace RetroFoot.App;

// Os cartoes das telas do online em duas colunas quando cabem (computador em 16:9 ou 4:3 com a
// janela larga) e numa coluna so, centrada e com a largura de leitura, no celular e nas janelas
// estreitas. Decide pela largura de verdade (vale para os dois visuais, os temas e a janela
// redimensionada). A coluna da direita vazia (nenhum cartao visivel) volta a uma coluna so.
public sealed class DuasColunas : Panel
{
    public double Limite { get; set; } = 900;   // a largura a partir da qual vao duas colunas
    public double Coluna { get; set; } = 560;   // a largura de uma coluna so
    public double Espaco { get; set; } = 14;

    public DuasColunas(Control esquerda, Control direita)
    {
        Children.Add(esquerda);
        Children.Add(direita);
    }

    Control Esq => Children[0];
    Control Dir => Children[1];

    bool _duas;

    protected override Size MeasureOverride(Size a)
    {
        double w = double.IsInfinity(a.Width) ? Coluna : a.Width;
        if (w >= Limite)
        {
            double cw = (w - Espaco) / 2;
            Dir.Measure(new Size(cw, double.PositiveInfinity));
            if (Dir.IsVisible && Dir.DesiredSize.Height > 0)
            {
                _duas = true;
                Esq.Measure(new Size(cw, double.PositiveInfinity));
                return new Size(w, Math.Max(Esq.DesiredSize.Height, Dir.DesiredSize.Height));
            }
        }
        _duas = false;
        double lw = Math.Min(w, Coluna);
        Esq.Measure(new Size(lw, double.PositiveInfinity));
        Dir.Measure(new Size(lw, double.PositiveInfinity));
        double h = Esq.DesiredSize.Height + Dir.DesiredSize.Height;
        if (Esq.DesiredSize.Height > 0 && Dir.DesiredSize.Height > 0) h += Espaco;
        return new Size(lw, h);
    }

    protected override Size ArrangeOverride(Size f)
    {
        if (_duas)
        {
            double cw = (f.Width - Espaco) / 2;
            Esq.Arrange(new Rect(0, 0, cw, Esq.DesiredSize.Height));
            Dir.Arrange(new Rect(cw + Espaco, 0, cw, Dir.DesiredSize.Height));
            return f;
        }
        double lw = Math.Min(f.Width, Coluna), x = (f.Width - lw) / 2;
        Esq.Arrange(new Rect(x, 0, lw, Esq.DesiredSize.Height));
        double y = Esq.DesiredSize.Height + (Esq.DesiredSize.Height > 0 ? Espaco : 0);
        Dir.Arrange(new Rect(x, y, lw, Dir.DesiredSize.Height));
        return f;
    }
}
