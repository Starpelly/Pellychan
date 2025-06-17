using Pellychan.GUI.Input;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Platform.Skia;
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
            m_globalPosition = getGlobalPosition();
        }
    }
    public int Y
    {
        get => m_y;
        set
        {
            m_y = value;
            m_globalPosition = getGlobalPosition();
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
    
    // @NOTE
    // I'm not completely sure how well this is would work if used.
    // We might need a better caching system for this...?
    private SKPoint m_globalPosition;
    
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
            if (m_visible != value)
            {
                m_visible = value;
                InvalidateLayout();
                NotifyLayoutChange();
            }
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
            if (m_enabled != value)
            {
                m_enabled = value;
                // Invalidate();
                // NotifyLayoutChange();
            }
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

    private FitPolicy m_fitPolicy = FitPolicy.FixedPolicy;
    public FitPolicy Fitting
    {
        get => m_fitPolicy;
        set
        {
            if (m_fitPolicy != value)
            {
                m_fitPolicy = value;
                InvalidateLayout();
                NotifyLayoutChange();
            }
        }
    }

    private SizePolicy m_sizePolicy = SizePolicy.FixedPolicy;
    public SizePolicy Sizing
    {
        get => m_sizePolicy;
        set
        {
            if (m_sizePolicy != value)
            {
                m_sizePolicy = value;
                InvalidateLayout();
                NotifyLayoutChange();
            }
        }
    }

    public virtual SKSizeI SizeHint => Layout?.SizeHint(this) ?? new(m_width, m_height);
    public virtual SKSizeI MinimumSizeHint => new(0, 0);

    public int MinimumWidth { get; set; } = 0;
    public int MaximumWidth { get; set; } = int.MaxValue;

    public int MinimumHeight { get; set; } = 0;
    public int MaximumHeight { get; set; } = int.MaxValue;

    public Action? OnLayoutResize;
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
        m_visible = true;

        if (IsTopLevel)
        {
            Application.Instance!.TopLevelWidgets.Add(this);

            m_nativeWindow?.Resize(m_width, m_height);
            m_nativeWindow?.CreateFrameBuffer(m_width, m_height);
            m_nativeWindow?.Center();
            m_nativeWindow?.Show();
        }

        InvalidateLayout(true);
        NotifyLayoutChange();

        /*
        void invalidateChildren(Widget parent)
        {
            foreach (var child in parent.m_children)
            {
                child.InvalidateLayout();

                invalidateChildren(child);
            }
        }

        invalidateChildren(this);
        //NotifyLayoutChange();
        */
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

        if (m_parent != null)
        {
            m_parent.Children.Remove(this);
            if (m_parent.Layout != null)
                m_parent.InvalidateLayout();
        }

        m_parent = parent;

        if (m_parent != null)
        {
            m_parent.Children.Add(this);

            if (m_parent.Layout != null && m_parent.Visible)
                m_parent.InvalidateLayout();

            if (Layout != null)
                InvalidateLayout();
        }

        NotifyLayoutChange(); // In case grandparent needs to update layout too
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

    /// <summary>
    /// Forces this widget and its layout hierarchy to update sizes and positions.
    /// Should be called when geometry-affecting state changes (e.g. size policy, size hint).
    /// </summary>
    public void UpdateGeometry()
    {
        InvalidateLayout();
        NotifyLayoutChange();
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

    internal void Paint(SKCanvas canvas, SKRect clipRect)
    {
        if (m_height <= 0 || m_height <= 0 || !m_visible)
            return;

        var globalPos = getGlobalPosition();
        
        var thisRect = new SKRect(globalPos.X, globalPos.Y, globalPos.X + m_width, globalPos.Y + m_height);
        var currentClip = SKRect.Intersect(clipRect, thisRect);

        if (currentClip.IsEmpty)
            return;

        /*
        foreach (var clip in clipStack)
        {
            var a = this;
            if (!clip.IntersectsWith(thisRect))
                return;
        }
        
        clipStack.Push(thisRect);
        */

        canvas.Save();
        canvas.Translate(m_x, m_y);
        canvas.ClipRect(new(0, 0, m_width, m_height));

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
                if (!child.m_visible)
                    continue;

                child.Paint(canvas, clipRect);
            }
        }

        // Debug shit
        if (Application.DebugDrawing)
        {
            canvas.Save();
            canvas.ResetMatrix();
            canvas.ClipRect(new SKRect(0, 0, 2000, 2000));

            using var debugPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Red };
            canvas.DrawRect(new SKRect(globalPos.X, globalPos.Y, globalPos.X + (m_width - 1), globalPos.Y + (m_height - 1)), debugPaint);

            canvas.Restore();
        }

        // clipStack.Pop();
        canvas.Restore();

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

            //var rootStack = new Stack<SKRect>();
            // rootStack.Push(new SKRect(0, 0, m_width, m_height));
            var rootClip = new SKRect(0, 0, m_width, m_height);

            Paint(canvas, rootClip);

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

    internal void PerformUpdateLayout()
    {
        Layout?.FitSizingPass(this);
        Layout?.GrowSizingPass(this);
        Layout?.PositionsPass(this);
        // Layout?.PerformLayout(this);

        if (Layout != null)
        {
            /*
            var sizeHint = SizeHint;
            var newWidth = m_width;
            var newHeight = m_height;

            if (SizePolicy.Horizontal == SizePolicy.Policy.Preferred)
                newWidth = sizeHint.Width;
            if (SizePolicy.Vertical == SizePolicy.Policy.Preferred)
                newHeight = sizeHint.Height;

            Resize(newWidth, newHeight);
            */
        }

        OnLayoutUpdate?.Invoke();
    }

    internal void InvalidateLayout(bool doChildrenAnyway = false)
    {
        if (!Visible)
            return;
        if (Layout == null)
        {
            if (doChildrenAnyway)
            {
                goto Children;
            }
            return;
        }

        Application.LayoutQueue.Enqueue(this);

    Children:
        foreach (var child in m_children)
        {
            child.InvalidateLayout(doChildrenAnyway);
        }
    }

    /// <summary>
    /// Tells all parents to update layouts (if they have layouts).
    /// </summary>
    internal void NotifyLayoutChange()
    {
        var p = Parent;
        if (p != null)
        {
            if (p.Layout != null)
            {
                p.InvalidateLayout();
                // break;
            }
            // Commented out because we only want to go
            // one node upwards
            // p = p.Parent;
        }
    }

    /// <summary>
    /// Actually resizes the widget, SHOULD only be called by the <see cref="Application.LayoutQueue"/>.
    /// </summary>
    internal void CatchResizeEvent()
    {
        Console.WriteLine($"Caught resize of type: {GetType().Name}");

        OnLayoutResize?.Invoke();
        m_nativeWindow?.Resize(m_width, m_height);

        (this as IResizeHandler)?.OnResize(m_width, m_height);
    }

    #endregion

    #region Private Methods

    private SKPoint getGlobalPosition()
    {
        var x = m_x;
        var y = m_y;

        Widget? current = m_parent;
        while (current != null)
        {
            x += current.m_x;
            y += current.m_y;
            current = current.m_parent;
        }

        return new(x, y);
    }

    private void dispatchResize()
    {
        if (!Application.LayoutQueue.IsFlusing)
        {
            if (Layout != null)
            {
                InvalidateLayout();
            }
            NotifyLayoutChange();
        }
    }

    private void initializeIfTopLevel()
    {
        if (!IsTopLevel) return;

        Console.WriteLine($"Initialized top level widget of type: {GetType().Name}");

        m_nativeWindow = new(this, m_height, m_height, GetType().Name);
        WindowRegistry.Register(m_nativeWindow);

        m_nativeWindow.OnWindowResize += onNativeWindowResizeEvent;
        m_nativeWindow.OnMouseEvent += onNativeWindowMouseEvent;
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

    private void onNativeWindowMouseEvent(int mouseX, int mouseY, MouseEventType type)
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

    private void onNativeWindowResizeEvent(int w, int h)
    {
        m_nativeWindow!.CreateFrameBuffer(w, h);
        Resize(w, h);

        Invalidate();
    }

    #endregion
}