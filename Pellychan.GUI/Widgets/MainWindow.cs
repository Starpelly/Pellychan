using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class MainWindow : Widget
{
    public MainWindow(Widget? parent = null) : base(parent)
    {
    }

    /// <summary>
    /// Sets the title of the window.
    /// </summary>
    public void SetTitle(string title)
    {
        m_nativeWindow?.SetTitle(title);
    }
}