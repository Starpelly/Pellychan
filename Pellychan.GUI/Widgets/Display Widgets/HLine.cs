namespace Pellychan.GUI.Widgets;

/// <summary>
/// A horizontal line. Draws in the center of its rectangle from the left to the right.
/// </summary>
public class HLine : ShapedFrame
{
    public HLine(Widget? parent = null) : base(parent)
    {
        FrameShape = Shape.HLine;
    }
}