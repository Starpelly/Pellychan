using Pellychan.GUI.Styles;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

/// <summary>
/// Base class of widgets that can have a frame.
/// </summary>
public class ShapedFrame : Widget, IPaintHandler
{
    public enum Shape
    {
        NoFrame = 0,
        Box,        // Rectangular box
        Panel,      // Rectangular panel
        HLine,      // Horizontal line
        VLine,      // Vertical line
    }

    public Shape FrameShape { get; set; } = Shape.Box;

    public ShapedFrame(Widget? parent = null) : base(parent)
    {
        ContentsMargins = new(1);
    }

    public void OnPaint(SKCanvas canvas)
    {
        var option = new StyleOptionShapedFrame
        {
        };
        option.InitFrom(this);

        Application.DefaultStyle.DrawShapedFrame(canvas, this, option);
    }
}