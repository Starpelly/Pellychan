using Pellychan.GUI.Input;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public struct MouseEvent
{
    public int x;
    public int y;
    public int globalX;
    public int globalY;

    public MouseButton button;
}

public struct MouseWheelEvent
{
    public int x;
    public int y;
    public int globalX;
    public int globalY;

    public int deltaX;
    public int deltaY;
}

public interface IPaintHandler
{
    public void OnPaint(SKCanvas canvas);
}

public interface IPostPaintHandler
{
    public void OnPostPaint(SKCanvas canvas);
}

public interface IMouseEnterHandler
{
    public void OnMouseEnter();
}

public interface IMouseLeaveHandler
{
    public void OnMouseLeave();
}

public interface IMouseMoveHandler
{
    public bool OnMouseMove(int x, int y);
}

public interface IMouseDownHandler
{
    public bool OnMouseDown(MouseEvent evt);
}

public interface IMouseUpHandler
{
    public bool OnMouseUp(MouseEvent evt);
}

public interface IMouseClickHandler
{
    public bool OnMouseClick(MouseEvent evt);
}

public interface IMouseWheelHandler
{
    public bool OnMouseScroll(MouseWheelEvent evt);
}

public interface IResizeHandler
{
    public void OnResize(int width, int height);
}