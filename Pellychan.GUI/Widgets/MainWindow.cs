using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class MainWindow : Widget, IPaintHandler, IResizeHandler
{
    public MenuBar? Menubar;

    public MainWindow(Widget? parent = null) : base(parent)
    {
    }

    public void OnPaint(SKCanvas canvas)
    {
        canvas.Clear(EffectivePalette.Get(ColorGroup.Active, ColorRole.Window));
    }

    public void OnResize(int width, int height)
    {
        Menubar?.Resize(width, Menubar.Height);
    }
}