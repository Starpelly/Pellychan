using Pellychan.GUI.Input;
using Pellychan.GUI.Layouts;
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
    public List<Widget> Children => m_children;

    private int m_x = 0;
    private int m_y = 0;
    private int m_width = 0;
    private int m_height = 0;

    public int X
    {
        get => m_x;
        set
        {
            m_x = value;
        }
    }
    public int Y
    {
        get => m_y;
        set
        {
            m_y = value;
        }
    }
    public int Width
    {
        get => m_width;
        set
        {
            m_width = value;
            dispatchResize();
        }
    }
    public int Height
    {
        get => m_height;
        set
        {
            m_height = value;
            dispatchResize();
        }
    }
    
    public SKRect Rect => new(m_x, m_y, m_x + m_width, m_y + m_height);

    private bool m_visible = true;
    public bool Visible
    {
        get
        {
            if (m_parent != null)
            {
                if (!m_parent.Visible)
                    return false;
            }
            return m_visible;
        }
        set
        {
            m_visible = value;
        }
    }

    private bool m_enabled = true;
    public bool Enabled
    {
        get
        {
            if (m_parent != null)
            {
                if (!m_parent.Enabled)
                    return false;
            }
            return m_enabled;
        }
        set
        {
            m_enabled = value;
        }
    }

    protected virtual bool ShouldCache => false;

    private bool IsHovered { get; set; } = false;
    private Widget? m_lastHovered = null;

    private static Widget? s_mouseGrabber = null;

    private bool m_isDirty = false;
    private bool m_hasDirtyDescendants = false;

    // Layout
    public Layout? Layout { get; set; }

    public SizePolicy SizePolicy { get; set; } = SizePolicy.FixedPolicy;

    public virtual SKSizeI SizeHint => Layout?.SizeHint(this) ?? new(m_width, m_height);
    public virtual SKSizeI MinimumSizeHint => new(0, 0);

    public int MinimumWidth { get; set; } = 0;
    public int MaximumWidth { get; set; } = int.MaxValue;

    public int MinimumHeight { get; set; } = 0;
    public int MaximumHeight { get; set; } = int.MaxValue;

    public bool AutoResizeToFit { get; set; } = false;

    public Action? OnResize;
    public Action? OnLayoutUpdate;

    // Palette
    public ColorPalette Palette => Application.Palette;

    public ColorPalette EffectivePalette => Palette ?? Parent?.EffectivePalette ?? ColorPalette.Default;

    public ColorGroup ColorGroup => Enabled ? ColorGroup.Active : ColorGroup.Disabled;

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
            Visible = false;
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

        foreach (var child in m_children)
        {
            child.Show();
        }

        if (IsTopLevel)
        {
            Application.Instance!.TopLevelWidgets.Add(this);

            m_nativeWindow?.Resize(m_width, m_height);
            m_nativeWindow?.CreateFrameBuffer(m_width, m_height);
            m_nativeWindow?.Center();
            m_nativeWindow?.Show();
        }

        updateLayout();
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

        if (m_parent != null)
        {
            if (m_parent.Visible)
                m_parent.updateLayout();
        }
    }

    public void SetPosition(int x, int y)
    {
        m_x = x;
        m_y = y;
    }

    public void SetRect(int x, int y, int width, int height)
    {
        m_x = x;
        m_y = y;
        m_width = width;
        m_height = height;

        dispatchResize();
    }

    public void Resize(int width, int height)
    {
        // No point in dispatching anything in this case!
        if (m_width == width && m_height == height)
            return;

        m_width = width;
        m_height = height;

        dispatchResize();
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
        return Visible && (x >= 0 && y >= 0 && x < m_width && y < m_height);
    }

    public virtual void Dispose()
    {
        m_nativeWindow?.Dispose();
        m_cachedSurface?.Dispose();

        foreach (var child in m_children)
            child.Dispose();

        GC.SuppressFinalize(this);
    }

    #region Virtual methods

    private void updateLayout()
    {
        if (AutoResizeToFit && Layout != null)
        {
            var hint = Layout.SizeHint(this);
            Resize(hint.Width, hint.Height);
        }

        Layout?.PerformLayout(this);
        OnLayoutUpdate?.Invoke();
    }

    #endregion

    #region Internal methods

    internal void Paint(SKCanvas canvas)
    {
        if (m_height <= 0 || m_height <= 0 || !Visible)
            return;

        if (ShouldCache)
        {
            SKSurface? GetPaintSurface() => (IsTopLevel) ? canvas.Surface : m_cachedSurface!;

            if (m_isDirty)
            {
                m_isDirty = false;

                if (!IsTopLevel)
                {
                    if (m_cachedSurface == null || m_height != m_cachedWidth || m_height != m_cachedHeight)
                    {
                        m_cachedSurface?.Dispose();
                        m_cachedSurface = SKSurface.Create(new SKImageInfo(m_height, m_height), new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
                        m_cachedWidth = m_height;
                        m_cachedHeight = m_height;
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
                canvas.DrawImage(m_cachedImage, m_x, m_y);
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
                //if (!child.Rect.IntersectsWith(Rect))
                //    continue;

                canvas.Save();

                // Clip to the child's bounds relative to the parent
                // canvas.ClipRect(new SKRect(child.m_x, child.m_y, child.m_x + child.m_width, child.m_y + child.m_height));
                canvas.Translate(child.m_x, child.m_y);

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
        if (m_height <= 0 || m_height <= 0)
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
                if (m_cachedSurface == null || m_height != m_cachedWidth || m_height != m_cachedHeight)
                {
                    m_cachedSurface?.Dispose();

                    m_cachedSurface = SKSurface.Create(new SKImageInfo(m_height, m_height), new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
                    m_cachedWidth = m_height;
                    m_cachedHeight = m_height;
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
            canvas.DrawImage(m_cachedImage, m_x, m_y);
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
        if (m_height == 0 || m_height == 0) return;

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

    private void dispatchResize()
    {
        m_nativeWindow?.Resize(m_width, m_height);

        (this as IResizeHandler)?.OnResize(m_width, m_height);

        OnResize?.Invoke();

        updateLayout();
    }

    private void initializeIfTopLevel()
    {
        if (!IsTopLevel) return;

        Console.WriteLine($"Initialized top level widget of type: {GetType().Name}");

        m_nativeWindow = new(this, m_height, m_height, GetType().Name);
        WindowRegistry.Register(m_nativeWindow);

        m_nativeWindow.OnWindowResize += delegate (int w, int h)
        {
            m_nativeWindow!.CreateFrameBuffer(w, h);
            Resize(w, h);

            Invalidate();
        };
        m_nativeWindow.OnMouseEvent += dispatchMouseEvent;
        // m_nativeWindow.OnMouseMoved += dispatchMouseMove;
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
            lx -= current.m_x;
            ly -= current.m_y;
            current = current.Parent;
        }

        return (lx, ly);
    }

    private Widget? findHoveredWidget(int x, int y)
    {
        var thisX = (IsTopLevel) ? 0 : this.m_x;
        var thisY = (IsTopLevel) ? 0 : this.m_y;

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

            if (child.HitTest(x - child.m_x, y - child.m_y))
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

        if (hovered != m_lastHovered)
        {
            m_lastHovered?.handleMouseLeave();
            hovered?.handleMouseEnter();
            m_lastHovered = hovered;
        }

        // If there's a mouse grabber, it always receives input!
        if (s_mouseGrabber != null)
        {
            if (s_mouseGrabber.Enabled)
            {
                var localPos = getLocalPosition(s_mouseGrabber, mouseX, mouseY);
                int localX = localPos.Item1;
                int localY = localPos.Item2;

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
            }

            return;
        }

        // No grabber - do regular hit testing.
        if (hovered != null)
        {
            if (hovered.Enabled)
            {
                var localPos = getLocalPosition(hovered, mouseX, mouseY);
                int localX = localPos.Item1;
                int localY = localPos.Item2;

                switch (type)
                {
                    case MouseEventType.Move:
                        (hovered as IMouseMoveHandler)?.OnMouseMove(localX, localY);
                        break;
                    case MouseEventType.Down:
                        s_mouseGrabber = hovered;
                        (hovered as IMouseDownHandler)?.OnMouseDown(localX, localY);
                        break;
                    case MouseEventType.Up:
                        (hovered as IMouseUpHandler)?.OnMouseUp(localX, localY);

                        if (s_mouseGrabber == hovered)
                        {
                            (hovered as IMouseClickHandler)?.OnMouseClick(localX, localY);
                        }

                        s_mouseGrabber = null;
                        break;
                }
            }
        }
    }

    #endregion
}