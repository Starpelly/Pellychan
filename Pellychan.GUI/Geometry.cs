using SkiaSharp;
using System.Runtime.CompilerServices;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetX(this SKRectI rect)
    {
        return rect.Left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetY(this SKRectI rect)
    {
        return rect.Top;
    }

    public static SKRectI SetWidth(this SKRectI rect, int width)
    {
        return new SKRectI(rect.Left, rect.Top, rect.Left + width, rect.Bottom);
    }

    public static SKRectI SetHeight(this SKRectI rect, int height)
    {
        return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Top + height);
    }

    public static SKRectI SetSize(this SKRectI rect, int width, int height)
    {
        return new SKRectI(rect.Left, rect.Top, rect.Left + width, rect.Top + height);
    }

    public static SKRectI Translate(this SKRectI rect, int dx, int dy)
    {
        return new SKRectI(rect.Left + dx, rect.Top + dy, rect.Right + dx, rect.Bottom + dy);
    }

    public static SKRectI MoveTo(this SKRectI rect, int x, int y)
    {
        int width = rect.Width;
        int height = rect.Height;
        return new SKRectI(x, y, x + width, y + height);
    }

    public static SKRectI MoveCenter(this SKRectI rect, SKPointI center)
    {
        int width = rect.Width;
        int height = rect.Height;
        int left = center.X - width / 2;
        int top = center.Y - height / 2;
        return new SKRectI(left, top, left + width, top + height);
    }

    public static SKPointI Center(this SKRectI rect)
    {
        return new SKPointI((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
    }

    public static SKSizeI Size(this SKRectI rect)
    {
        return new SKSizeI(rect.Width, rect.Height);
    }

    public static bool Contains(this SKRectI rect, SKPointI point)
    {
        return point.X >= rect.Left && point.X < rect.Right &&
               point.Y >= rect.Top && point.Y < rect.Bottom;
    }
}