namespace Pellychan.GUI.Widgets;

/// <summary>
/// A vertical line. Draws in the center of its rectangle from the top to the bottom.
/// </summary>
public class VLine : ShapedFrame
{
    public VLine(Widget? parent = null) : base(parent)
    {
        FrameShape = Shape.VLine;
    }
}