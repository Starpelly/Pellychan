using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class ClickableLabel : Label, IMouseEnterHandler, IMouseLeaveHandler
{
    public ClickableLabel(SKFont font, Widget? parent = null) : base(font, parent)
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