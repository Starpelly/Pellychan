using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace Pellychan.GUI.Styles;

public enum ArrowType
{
    NoArrow,
    Up,
    Down,
    Left,
    Right
}

public abstract class Style
{
    [Flags]
    public enum StateFlag
    {
        None = 0,
        Enabled = 1 << 0,
        Raised = 1 << 1,
        Sunken = 1 << 2,
        Off = 1 << 3,
        On = 1 << 4,
        HasFocus = 1 << 5,
    }
    
    public abstract void DrawPushButton(SKCanvas canvas, PushButton button, StyleOptionButton option);
    public abstract void DrawScrollBar(SKCanvas canvas, ScrollBar scrollBar, StyleOptionScrollBar option);
}