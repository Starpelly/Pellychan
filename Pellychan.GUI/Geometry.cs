using SkiaSharp;

namespace Pellychan.GUI;

[Flags]
public enum Edges
{
    None = 0,
    Left = 1 << 0,
    Top = 1 << 1,
    Right = 1 << 2,
    Bottom = 1 << 3
}

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

public struct Margins
{
    public int Left, Top, Right, Bottom;

    public Margins(int uniform) => Left = Top = Right = Bottom = uniform;
    public Margins(int horizontal, int vertical)
    {
        Left = Right = horizontal;
        Top = Bottom = vertical;
    }
    public Margins(int left, int top, int right, int bottom)
    {
        Left = left; Top = top; Right = right; Bottom = bottom;
    }

    public int Horizontal => Left + Right;
    public int Vertical => Top + Bottom;
}

public static class SKRectExtensions
{
    public static SKRect Adjusted(this SKRect rect, float left, float top, float right, float bottom)
    {
        return new(
            rect.Left + left,
            rect.Top + top,
            rect.Right + right,
            rect.Bottom + bottom
        );
    }

    public static SKRectI Adjusted(this SKRectI rect, int left, int top, int right, int bottom)
    {
        return new(
            rect.Left + left,
            rect.Top + top,
            rect.Right + right,
            rect.Bottom + bottom
        );
    }

    public static SKRectI SetX(this SKRectI rect, int x)
    {
        var width = rect.Width;
        return new(x, rect.Top, x + width, rect.Bottom);
    }

    public static SKRectI SetY(this SKRectI rect, int y)
    {
        var height = rect.Height;
        return new(rect.Left, y, rect.Right, y + height);
    }
}