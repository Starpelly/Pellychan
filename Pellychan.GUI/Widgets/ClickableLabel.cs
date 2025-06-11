using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class ClickableLabel : Label
{
    public ClickableLabel(SKFont font) : base(font)
    {
        Paint.Color = SKColors.Blue;
    }

    public override void OnMouseEnter()
    {
        base.OnMouseEnter();
    }

    public override void OnMouseLeave()
    {
        base.OnMouseLeave();
    }
}