using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class DialogWindow : WindowWidget, IPaintHandler
{
    public DialogWindow(Widget? parent = null) : base(WindowType.Dialog, parent)
    {

    }

    public void OnPaint(SKCanvas canvas)
    {
        canvas.Clear(EffectivePalette.Get(ColorGroup.Active, ColorRole.Window));
    }
}