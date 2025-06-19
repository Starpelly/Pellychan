using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace Pellychan.GUI.Layouts;

public enum LayoutFlushType
{
    All = 0,
    Position,
    Size
}

public abstract class Layout
{
    public Padding Padding { get; set; } = new(0);
    public int Spacing { get; set; } = 0;

    internal bool PerformingPasses { get; private set; } = false;

    public void Start()
    {
        PerformingPasses = true;
    }

    public void End()
    {
        PerformingPasses = false;
    }

    internal Padding GetFinalPadding(Widget parent)
    {
        return new Padding(Padding.Left + parent.ContentsMargins.Left, Padding.Top + parent.ContentsMargins.Top, Padding.Right + parent.ContentsMargins.Right, Padding.Bottom + parent.ContentsMargins.Bottom);
    }

    public abstract SKSizeI SizeHint(Widget parent);

    public abstract void FitSizingPass(Widget parent);
    public abstract void GrowSizingPass(Widget parent);
    public abstract void PositionsPass(Widget parent);
}