using Pellychan.GUI.Input;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Platform.Input;
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

public interface IMouseWheelHandler
{
    public void OnMouseScroll(int x, int y, int deltaX, int deltaY);
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

    /// <summary>
    /// If true, no events will be called.
    /// OnResize, OnPaint, etc...
    /// (False by default)
    /// </summary>
    public bool DisableEvents { get; set; } = false;

    private bool m_catchCursorEvents = true;

    /// <summary>
    /// If true, the widget will block other UI from catching cursor events.
    /// This widget will also not catch cursor events.
    /// (True by default)
    /// </summary>
    public bool CatchCursorEvents
    {
        // I don't think I want it to stop ALL children from collecting events...
        /*
        get
        {
            if (m_parent != null)
            {
                if (!m_parent.CatchCursorEvents)
                    return false;
            }
            return m_catchCursorEvents;
        }
        */
        get => m_catchCursorEvents;
        set => m_catchCursorEvents = value;
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

    private bool IsHovered { get; set; } = false;
    private Widget? m_lastHovered = null;

    private static Widget? s_mouseGrabber = null;

    #region Layout
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

    public Action? OnPostLayoutUpdate;
    public Action? OnResized;

    /// <summary>
    /// Gets and sets the margins around the content of the widget.
    /// The margins are used by the layout system, and may be used by subclasses to specify the area to draw in (e.g. excluding the frame).
    /// </summary>
    public Margins ContentsMargins { get; set; } = new(0);

    private SKPointI m_contentPositions = new(0, 0);

    /// <summary>
    /// Gets and sets the position of the content relative to the widget.
    /// Used for positioning stuff that is affected by the layout system (e.g. a <see cref="ScrollArea"/> panning the content.
    /// </summary>
    public SKPointI ContentsPositions
    {
        get => m_contentPositions;
        set
        {
            if (m_contentPositions != value)
            {
                m_contentPositions = value;

                Application.LayoutQueue.Enqueue(this);
            }
        }
    }

    #endregion

    #region Palette

    public ColorPalette Palette => Application.Palette;

    public ColorPalette EffectivePalette => Palette ?? Parent?.EffectivePalette ?? Application.Palette;

    public ColorGroup ColorGroup => Enabled ? ColorGroup.Active : ColorGroup.Disabled;

    #endregion

    // Cursor
    public MouseCursor.CursorType? CursorShape = null;

    #region Cache

    private SKSurface? m_cachedSurface;
    private SKImage? m_cachedImage;
    private int m_cachedWidth;
    private int m_cachedHeight;

    // If top-level, owns a native window
    public bool IsTopLevel => Parent == null;
    internal SkiaWindow? m_nativeWindow;

    private bool m_isDirty = false;
    private bool m_hasDirtyDescendants = false;

    protected virtual bool ShouldCache => false;

    #endregion

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

        if (IsTopLevel && m_nativeWindow != null)
        {
            Application.Instance!.TopLevelWidgets.Add(this);

            m_nativeWindow.Size = new System.Drawing.Size(m_width, m_height);
            m_nativeWindow.CreateFrameBuffer(m_width, m_height);
            m_nativeWindow.Center();
            m_nativeWindow.Show();
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

        // This is fine because a native window can only exist on top level widgets and thus,
        // can't be in a layout!
        if (m_nativeWindow != null)
        {
            m_nativeWindow.Size = new System.Drawing.Size(m_width, m_height);
        }

        dispatchResize();
        CallResizeEvents();
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
        if (m_nativeWindow == null)
            return;

        m_nativeWindow.Title = (title);
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

    #region Virtual methods

    /// <summary>
    /// Called immediately before being updated by the layout engine.
    /// </summary>
    public virtual void OnPreLayout()
    {
    }

    /// <summary>
    /// Called immediately after being updated by the layout engine.
    /// </summary>
    public virtual void OnPostLayout()
    {
    }

    #endregion

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

            // Skia unfortunately doesn't have an API for clearing the clip rect. SO if you have a monitor bigger than this, God help you.
            canvas.ClipRect(new SKRect(0, 0, 40000, 40000));

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

        using var surface = SKSurface.Create(m_nativeWindow.ImageInfo, m_nativeWindow.Pixels, m_nativeWindow.Pitch, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));

        // var surface = m_nativeWindow!.SkiaSurface!;
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.White);

        //var rootStack = new Stack<SKRect>();
        // rootStack.Push(new SKRect(0, 0, m_width, m_height));
        var rootClip = new SKRect(0, 0, m_width, m_height);

        Paint(canvas, rootClip);

        canvas.Flush();

        // canvas.Flush();

        m_nativeWindow!.Unlock();

        m_nativeWindow!.Present();
    }

    internal bool ShouldClose()
    {
        return IsTopLevel && m_nativeWindow!.ShouldClose;
    }

    internal void PerformLayoutUpdate()
    {
        if (Layout != null)
        {
            var oldSize = (Width, Height);

            OnPreLayout();

            Layout.Start();
            Layout.FitSizingPass(this);
            Layout.GrowSizingPass(this);
            Layout.PositionsPass(this);
            Layout.End();

            OnPostLayout();
            OnPostLayoutUpdate?.Invoke();

            if (Width != oldSize.Width || Height != oldSize.Height)
                dispatchResize();
        }
    }

    internal void InvalidateLayout(bool doChildrenAnyway = false)
    {
        if (!Visible)
            return;

        bool shouldInvalidateChildren = doChildrenAnyway;

        if (Layout == null)
        {
            if (!doChildrenAnyway)
                return;
        }
        else
        {
            if (Layout.PerformingPasses)
                return;

            Application.LayoutQueue.Enqueue(this);
            shouldInvalidateChildren = true;
        }

        if (shouldInvalidateChildren)
        {
            foreach (var child in m_children)
            {
                child.InvalidateLayout(doChildrenAnyway);
            }
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
            // Commented out because we only want to go one node upwards
            // p = p.Parent;
        }
    }

    internal void CallResizeEvents()
    {
        // Console.WriteLine($"Calling resize events for type: {GetType().Name}");

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
        var isFlusing = Application.LayoutQueue.IsFlusing;

        if (Layout != null)
            isFlusing = Layout.PerformingPasses;

        if (!isFlusing)
        {
            if (Layout != null)
            {
                InvalidateLayout();
            }
            else
            {
                CallResizeEvents();
            }
            NotifyLayoutChange();

            OnResized?.Invoke();
        }
    }

    private void initializeIfTopLevel()
    {
        if (!IsTopLevel) return;

        Console.WriteLine($"Initialized top level widget of type: {GetType().Name}");

        m_nativeWindow = new(this, GetType().Name);
        WindowRegistry.Register(m_nativeWindow);

        m_nativeWindow.Resized += delegate ()
        {
            onNativeWindowResizeEvent(m_nativeWindow.Size.Width, m_nativeWindow.Size.Height);
        };
        m_nativeWindow.MouseMove += delegate (System.Numerics.Vector2 pos)
        {
            onNativeWindowMouseEvent((int)pos.X, (int)pos.Y, MouseEventType.Move);
        };
        m_nativeWindow.MouseDown += delegate(System.Numerics.Vector2 pos, MouseButton button)
        {
            onNativeWindowMouseEvent((int)pos.X, (int)pos.Y, MouseEventType.Down);
        };
        m_nativeWindow.MouseUp += delegate (System.Numerics.Vector2 pos, MouseButton button)
        {
            onNativeWindowMouseEvent((int)pos.X, (int)pos.Y, MouseEventType.Up);
        };
        m_nativeWindow.MouseWheel += delegate (System.Numerics.Vector2 pos, System.Numerics.Vector2 delta, bool precise)
        {
            onNativeWindowMouseEvent((int)pos.X, (int)pos.Y, MouseEventType.Wheel, (int)delta.X, (int)delta.Y);
        };
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

    private Widget? findHoveredWidget(int x, int y, bool checkRaycast)
    {
        var thisX = (IsTopLevel) ? 0 : this.m_x;
        var thisY = (IsTopLevel) ? 0 : this.m_y;

        int localX = x - thisX;
        int localY = y - thisY;

        bool canCatchEvents = true;
        if (checkRaycast)
        {
            if (!CatchCursorEvents)
            {
                canCatchEvents = false;
            }
        }

        if (canCatchEvents)
        if (!HitTest(localX, localY))
            return null;

        // If we can't catch any events, skip the hit test and skip immediately to the children
        foreach (var child in m_children.AsReadOnly().Reverse()) // top-most first
        {
            var result = child.findHoveredWidget(localX, localY, checkRaycast);
            if (result != null)
                return result;
        }

        return canCatchEvents ? this : null;
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

    private void onNativeWindowMouseEvent(int mouseX, int mouseY, MouseEventType type, int deltaX = 0, int deltaY = 0)
    {
        var hovered = findHoveredWidget(mouseX, mouseY, true);

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
                    case MouseEventType.Wheel:
                        (s_mouseGrabber as IMouseWheelHandler)?.OnMouseScroll(localX, localY, deltaX, deltaY);
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
                    case MouseEventType.Wheel:
                        (hovered as IMouseWheelHandler)?.OnMouseScroll(localX, localY, deltaX, deltaY);
                        break;
                }
            }
        }
    }

    private void onNativeWindowResizeEvent(int w, int h)
    {
        Resize(w, h);
        m_nativeWindow!.CreateFrameBuffer(w, h);

        // Invalidate();

        Application.LayoutQueue.Flush();
        RenderTopLevel();
    }

    #endregion
}