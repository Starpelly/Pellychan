using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class ToolWindow : Widget
{
    public ToolWindow(Widget? parent = null) : base(parent, WindowType.Tool)
    {
    }
}