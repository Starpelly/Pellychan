using Pellychan.GUI.Widgets;

namespace Pellychan.GUI.Layouts;

public struct Padding
{
    public int Left, Top, Right, Bottom;

    public Padding(int uniform) => Left = Top = Right = Bottom = uniform;
    public Padding(int horizontal, int vertical)
    {
        Left = Right = horizontal;
        Top = Bottom = vertical;
    }
    public Padding(int left, int top, int right, int bottom)
    {
        Left = left; Top = top; Right = right; Bottom = bottom;
    }

    public int Horizontal => Left + Right;
    public int Vertical => Top + Bottom;
}

public abstract class Layout
{
    public Padding Padding { get; set; } = new(0);

    public abstract void PerformLayout(Widget parent);
}