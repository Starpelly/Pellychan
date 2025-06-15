using Pellychan.GUI.Input;
using Pellychan.GUI.Platform.Skia;
using SDL2;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public interface IPaintHandler
{
    public void OnPaint(SKCanvas canvas);
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
    public void OnMouseMove(int x, int y);
}

public interface IMouseDownHandler
{
    public void OnMouseDown(int x, int y);
}

public interface IMouseUpHandler
{
    public void OnMouseUp(int x, int y);
}

public interface IMouseClickHandler
{
    public void OnMouseClick(int x, int y);
}

public interface IResizeHandler
{
    public void OnResize(int width, int height);
}

public class Widget : IDisposable
{
    private Widget? m_parent;
    public Widget? Parent
    {
        get => m_parent;
    }
    private readonly List<Widget> m_children = [];

    public int X = 0;
    public int Y = 0;
    public int Width;
    public int Height;
    
    public SKRect Rect => new(X, Y, X + Width, Y + Height);

    public bool Visible = true;

    protected virtual bool ShouldCache => false;

    private bool IsHovered { get; set; } = false;
    private Widget? m_lastHovered = null;

    private static Widget? s_mouseGrabber = null;

    private bool m_isDirty = false;
    private bool m_hasDirtyDescendants = false;

    // Palette
    public ColorPalette? Palette { get; set; }

    public ColorPalette EffectivePalette => Palette ?? Parent?.EffectivePalette ?? ColorPalette.Default;

    // Cursor
    public MouseCursor.CursorType? CursorShape = null;
    
    // Cache
    private SKSurface? m_cachedSurface;
    private SKImage? m_cachedImage;
    private int m_cachedWidth;
    private int m_cachedHeight;

    // If top-level, owns a native window
    public bool IsTopLevel => Parent == null;
    internal SkiaWindow? m_nativeWindow;

    public Widget(Widget? parent = null)
    {
        if (parent != null)
            SetParent(parent);

        if (IsTopLevel)
        {
            initializeIfTopLevel();
        }

        Invalidate();
    }

    /// <summary>
    /// Shows the widget and its child widgets.
    /// 
    /// For child windows, this is the equivalent to calling `<see cref="Visible"/> = true`.
    /// </summary>
    public void Show()
    {
        Visible = true;

        if (IsTopLevel)
        {
            Application.Instance!.TopLevelWidgets.Add(this);

            m_nativeWindow?.Resize(Width, Height);
            m_nativeWindow?.CreateFrameBuffer(Width, Height);
            m_nativeWindow?.Center();
            m_nativeWindow?.Show();
        }
    }

    /// <summary>
    /// Sets the parent of the widget to the parent. The widget is moved to position (0, 0) in its new parent.
    /// 
    /// If the "new" parent widget is the old parent widget, this function does nothing.
    /// </summary>
    public void SetParent(Widget parent)
    {
        if (m_parent == parent)
            return;

        m_parent?.m_children.Remove(this);

        m_parent = parent;
        m_parent?.m_children.Add(this);
    }

    public void SetPosition(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public void SetRect(int x, int y, int width, int height)
    {
        this.X = x;
        this.Y = y;
        Width = width;
        Height = height;
    }

    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;

        m_nativeWindow?.Resize(width, height);

        (this as IResizeHandler)?.OnResize(width, height);
    }

    public void Invalidate()
    {
        invalidate();
    }

    /// <summary>
    /// Sets the title of the window (if this is a top level widget).
    /// </summary>
    public void SetWindowTitle(string title)
    {
        m_nativeWindow?.SetTitle(title);
    }

    public bool HitTest(int x, int y)
    {
        return Visible && (x >= 0 && y >= 0 && x < Width && y < Height);
    }

    public virtual void Dispose()
    {
        m_nativeWindow?.Dispose();
        m_cachedSurface?.Dispose();

        foreach (var child in m_children)
            child.Dispose();

        GC.SuppressFinalize(this);
    }

    #region Internal methods

    internal void Paint(SKCanvas canvas)
    {
        if (Width <= 0 || Height <= 0 || !Visible)
            return;

        if (ShouldCache)
        {
            SKSurface? GetPaintSurface() => (IsTopLevel) ? canvas.Surface : m_cachedSurface!;

            if (m_isDirty)
            {
                m_isDirty = false;

                if (!IsTopLevel)
                {
                    if (m_cachedSurface == null || Width != m_cachedWidth || Height != m_cachedHeight)
                    {
                        m_cachedSurface?.Dispose();
                        m_cachedSurface = SKSurface.Create(new SKImageInfo(Width, Height), new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
                        m_cachedWidth = Width;
                        m_cachedHeight = Height;
                    }
                }

                var paintSurface = GetPaintSurface()!;
                var sc = paintSurface.Canvas;

                sc.Save();
                sc.Clear(SKColors.Transparent);

                var paintHandler = this as IPaintHandler;
                paintHandler?.OnPaint(sc);

                sc.Restore();

                m_cachedImage?.Dispose();
                m_cachedImage = paintSurface.Snapshot();
            }

            if (m_cachedImage != null)
            {
                canvas.DrawImage(m_cachedImage, X, Y);
            }
        }
        else
        {
            (this as IPaintHandler)?.OnPaint(canvas);
        }

        if (m_children.Count > 0)
        {
            foreach (var child in m_children)
            {
                // Don't paint anything that isn't set as visible.
                if (!child.Visible)
                    continue;
                
                // Don't paint anything that is outside the view bounds
                if (!child.Rect.IntersectsWith(Rect))
                    continue;

                canvas.Save();

                // Clip to the child's bounds relative to the parent
                canvas.ClipRect(new SKRect(child.X, child.Y, child.X + child.Width, child.Y + child.Height));
                canvas.Translate(child.X, child.Y);

                // Paint the child with the canvas offset to its local space
                child.Paint(canvas);

                canvas.Restore();
            }
        }

        m_hasDirtyDescendants = false;
    }

    [Obsolete]
    internal void Paint_OLD(SKCanvas canvas)
    {
        if (Width <= 0 || Height <= 0)
            return;

        SKSurface? GetPaintSurface()
        {
            return (IsTopLevel) ? canvas.Surface : m_cachedSurface!;
        }

        if (m_isDirty)
        {
            m_isDirty = false;

            // We'll only recreate the surface when the widget is resized
            // AND only if we're not a top level widget, as the surface for a top level widget is the window itself
            if (!IsTopLevel)
            {
                if (m_cachedSurface == null || Width != m_cachedWidth || Height != m_cachedHeight)
                {
                    m_cachedSurface?.Dispose();

                    m_cachedSurface = SKSurface.Create(new SKImageInfo(Width, Height), new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
                    m_cachedWidth = Width;
                    m_cachedHeight = Height;
                }
            }

            var paintSurface = GetPaintSurface()!;

            var sc = paintSurface.Canvas;
            sc.Clear(SKColors.Transparent);
            var paint = this as IPaintHandler;
            paint?.OnPaint(sc);

            // We can recreate the image every paint, that's fine.
            m_cachedImage?.Dispose();
            m_cachedImage = paintSurface.Snapshot();
        }

        // Draw the cached image to the real canvas
        // This is never actually null, but I don't want Rider yelling at me... :<
        if (m_cachedImage != null)
        {
            canvas.DrawImage(m_cachedImage, X, Y);
        }

        // Draw the children afterwards (obviously)
        if (m_children.Count > 0)
        {
            var paintSurface = GetPaintSurface()!;

            foreach (var child in m_children)
            {
                child.Paint(paintSurface.Canvas);
            }
        }

        m_hasDirtyDescendants = false;
    }

    internal void RenderTopLevel()
    {
        if (!IsTopLevel) return;
        if (Width == 0 || Height == 0) return;

        // Lock texture to get pixel buffer
        m_nativeWindow!.Lock();

        var surface = m_nativeWindow!.Surface!;
        var canvas = surface.Canvas;

        {
            canvas.Clear(SKColors.White);

            Paint(canvas);

            canvas.Flush();
        }

        canvas.Flush();

        m_nativeWindow!.Unlock();

        m_nativeWindow!.Present();
    }

    internal bool ShouldClose()
    {
        return IsTopLevel && m_nativeWindow!.ShouldClose;
    }

    #endregion

    #region Private Methods

    private void initializeIfTopLevel()
    {
        if (!IsTopLevel) return;

        Console.WriteLine($"Initialized top level widget of type: {GetType().Name}");

        m_nativeWindow = new(this, Width, Height, GetType().Name);
        WindowRegistry.Register(m_nativeWindow);

        m_nativeWindow.OnWindowResize += delegate (int w, int h)
        {
            m_nativeWindow!.CreateFrameBuffer(w, h);
            Resize(w, h);

            Invalidate();
        };
        m_nativeWindow.OnMouseEvent += dispatchMouseEvent;
        m_nativeWindow.OnMouseMoved += dispatchMouseMove;
    }

    private void invalidate()
    {
        if (m_isDirty) return;
        m_isDirty = true;
        Parent?.markChildDirty();
    }

    private void markChildDirty()
    {
        if (m_hasDirtyDescendants) return;
        m_hasDirtyDescendants = true;
        Parent?.markChildDirty();
    }

    private (int, int) getLocalPosition(Widget widget, int globalX, int globalY)
    {
        int lx = globalX;
        int ly = globalY;

        Widget? current = widget;
        while (current != null && current != this)
        {
            lx -= current.X;
            ly -= current.Y;
            current = current.Parent;
        }

        return (lx, ly);
    }

    private Widget? findHoveredWidget(int x, int y)
    {
        var thisX = (IsTopLevel) ? 0 : this.X;
        var thisY = (IsTopLevel) ? 0 : this.Y;

        int localX = x - thisX;
        int localY = y - thisY;

        if (!HitTest(localX, localY))
            return null;

        foreach (var child in m_children.AsReadOnly().Reverse()) // top-most first
        {
            var result = child.findHoveredWidget(localX, localY);
            if (result != null)
                return result;
        }

        return this;
    }

    /// <summary>
    /// Helper to find topmost widget under point
    /// </summary>
    private Widget? findTopMostWidgetAt(int x, int y)
    {
        // Reverse order so topmost drawn widget checked first
        foreach (var child in m_children.AsEnumerable().Reverse())
        {
            if (!child.Visible)
                continue;

            if (child.HitTest(x - child.X, y - child.Y))
                return child;
        }
        return null;
    }

    // --------------------
    // Native window events
    // --------------------

    private void handleMouseEnter()
    {
        if (!IsHovered)
        {
            IsHovered = true;
            (this as IMouseEnterHandler)?.OnMouseEnter();
        }
    }

    private void handleMouseLeave()
    {
        if (IsHovered)
        {
            IsHovered = false;
            (this as IMouseLeaveHandler)?.OnMouseLeave();
        }
    }

    private void dispatchMouseEvent(int mouseX, int mouseY, MouseEventType type)
    {
        var hovered = findHoveredWidget(mouseX, mouseY);

        // If there's a mouse grabber, it always receives input!
        if (s_mouseGrabber != null)
        {
            int localX = mouseX - s_mouseGrabber.X;
            int localY = mouseY - s_mouseGrabber.Y;

            switch (type)
            {
                case MouseEventType.Move:
                    (s_mouseGrabber as IMouseMoveHandler)?.OnMouseMove(localX, localY);
                    break;
                case MouseEventType.Down:
                    (s_mouseGrabber as IMouseDownHandler)?.OnMouseDown(localX, localY);
                    break;
                case MouseEventType.Up:
                    (s_mouseGrabber as IMouseUpHandler)?.OnMouseUp(localX, localY);

                    if (s_mouseGrabber == hovered)
                    {
                        (s_mouseGrabber as IMouseClickHandler)?.OnMouseClick(localX, localY);
                    }

                    s_mouseGrabber = null;
                    break;
            }

            return;
        }

        // No grabber - do regular hit testing.
        if (hovered != null)
        {
            int localX = mouseX - hovered.X;
            int localY = mouseY - hovered.Y;

            switch (type)
            {
                case MouseEventType.Move:
                    (hovered as IMouseMoveHandler)?.OnMouseMove(localX, mouseY);
                    break;
                case MouseEventType.Down:
                    s_mouseGrabber = hovered;
                    (hovered as IMouseDownHandler)?.OnMouseDown(localX, mouseY);
                    break;
                case MouseEventType.Up:
                    (hovered as IMouseUpHandler)?.OnMouseUp(localX, mouseY);

                    if (s_mouseGrabber == hovered)
                    {
                        (hovered as IMouseClickHandler)?.OnMouseClick(localX, mouseY);
                    }

                    s_mouseGrabber = null;
                    break;
            }
        }
    }

    private void dispatchMouseMove(int x, int y)
    {
        var newHovered = findHoveredWidget(x, y);

        if (newHovered != m_lastHovered)
        {
            m_lastHovered?.handleMouseLeave();
            newHovered?.handleMouseEnter();
            m_lastHovered = newHovered;
        }

        if (newHovered is IMouseMoveHandler moveHandler)
        {
            var (lx, ly) = getLocalPosition(newHovered, x, y);
            moveHandler.OnMouseMove(lx, ly);
        }

        // Cursor shape
        var cursor = newHovered?.CursorShape;
        if (cursor.HasValue)
            MouseCursor.Set(cursor.Value);
    }

    #endregion
}