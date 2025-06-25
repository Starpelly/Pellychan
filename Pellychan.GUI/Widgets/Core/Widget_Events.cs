using SkiaSharp;

namespace Pellychan.GUI.Widgets;

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
    public bool OnMouseDown(int x, int y);
}

public interface IMouseUpHandler
{
    public bool OnMouseUp(int x, int y);
}

public interface IMouseClickHandler
{
    public bool OnMouseClick(int x, int y);
}

public interface IMouseWheelHandler
{
    public bool OnMouseScroll(int x, int y, int deltaX, int deltaY);
}

public interface IResizeHandler
{
    public void OnResize(int width, int height);
}