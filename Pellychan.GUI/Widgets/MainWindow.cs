namespace Pellychan.GUI.Widgets;

public class MainWindow : Widget, IResizeHandler
{
    public MenuBar? Menubar;

    public MainWindow(Widget? parent = null) : base(parent)
    {
    }

    public void OnResize(int width, int height)
    {
        Menubar?.Resize(width, Menubar.Height);
    }
}