using Pellychan.GUI;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace Pellychan.Widgets;

public class AboutWindow : DialogWindow, IPaintHandler
{
    public AboutWindow(Widget? parent = null) : base(parent)
    {
        Resize(400, 400);
    }

    public void OnPaint(SKCanvas canvas)
    {
        canvas.Clear(EffectivePalette.Get(ColorGroup.Active, ColorRole.Window));
    }

    public override void OnShown()
    {
        SetWindowTitle("About Pellychan");
    }
}