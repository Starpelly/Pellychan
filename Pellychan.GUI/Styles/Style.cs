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
        /// <summary>
        /// The widget doesn't have a state.
        /// </summary>
        None = 0,

        /// <summary>
        /// The widget is active.
        /// </summary>
        Active = 1 << 0,

        /// <summary>
        /// The widget is enabled.
        /// </summary>
        Enabled = 1 << 1,

        /// <summary>
        /// Indicate if auto-raise appearance should be used on a tool button.
        /// </summary>
        Raised = 1 << 2,

        Sunken = 1 << 3,

        /// <summary>
        /// The widget is not "checked".
        /// </summary>
        Off = 1 << 4,

        /// <summary>
        /// The widget is "checked".
        /// </summary>
        On = 1 << 5,

        /// <summary>
        /// The widget has focus.
        /// </summary>
        HasFocus = 1 << 6,
    }
    
    public abstract void DrawPushButton(SKCanvas canvas, PushButton button, StyleOptionButton option);
    public abstract void DrawScrollBar(SKCanvas canvas, ScrollBar scrollBar, StyleOptionScrollBar option);
    public abstract void DrawShapedFrame(SKCanvas canvas, ShapedFrame frame, StyleOptionShapedFrame option);

    // Idk how I feel about this...
    // I'm feeling like this should just be part of the palette?
    public abstract SKColor GetFrameColor();
}