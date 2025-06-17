using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace Pellychan.GUI.Layouts;

public abstract class Layout
{
    public Padding Padding { get; set; } = new(0);

    public abstract void PerformLayout(Widget parent);
    public abstract SKSizeI SizeHint(Widget parent);

    public abstract void FitSizingPass(Widget parent);
    public abstract void GrowSizingPass(Widget parent);
    public abstract void PositionsPass(Widget parent);
}