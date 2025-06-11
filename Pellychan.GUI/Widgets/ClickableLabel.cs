using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class ClickableLabel : Label, IMouseEnterHandler, IMouseLeaveHandler
{
    public ClickableLabel(SKFont font) : base(font)
    {
        Paint.Color = SKColors.Blue;
    }

    public void OnMouseEnter()
    {
    }

    public void OnMouseLeave()
    {
    }
}