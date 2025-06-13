using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class MainWindow : Widget, IResizeHandler
{
    public MenuBar? Menubar;

    public MainWindow() : base()
    {
    }

    public void OnResize(int width, int height)
    {
        Menubar?.Resize(width, Menubar.Height);
    }
}