using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class DialogWindow : Widget, IPaintHandler
{
    public DialogWindow(Widget? parent = null) : base(parent, WindowType.Dialog)
    {

    }

    public void OnPaint(SKCanvas canvas)
    {
        canvas.Clear(EffectivePalette.Get(ColorGroup.Active, ColorRole.Window));
    }
}