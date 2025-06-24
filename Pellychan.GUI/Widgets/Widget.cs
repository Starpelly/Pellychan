using Pellychan.GUI.Input;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Platform;
using Pellychan.GUI.Platform.Input;
using Pellychan.GUI.Platform.Skia;
using SDL;
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

public class Widget : IDisposable
{
    private string m_name = string.Empty;
    public string Name
    {
        get => m_name;
        set => m_name = value;
    }

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
            if (m_width != value)
            {
                m_width = value;
                dispatchResize();
            }
        }
    }
    public int Height
    {
        get => m_height;
        set
        {
            if (m_height != value)
            {
                m_height = value;
                dispatchResize();
            }
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
                TriggerRepaint();
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

    private SizePolicy m_autoSizePolicy = SizePolicy.FixedPolicy;
    public SizePolicy AutoSizing
    {
        get => m_autoSizePolicy;
        set
        {
            if (m_autoSizePolicy != value)
            {
                m_autoSizePolicy = value;
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

                LayoutQueue.Enqueue(this, LayoutFlushType.Position);
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
    private SKBitmap? m_cachedBitmap;
    private SKPicture? m_cachedPicture;
    private int m_cachedWidth;
    private int m_cachedHeight;
    private unsafe SDL_Texture* m_cachedRenderTexture;

    // If top-level, owns a native window
    internal SkiaWindow? m_nativeWindow;

    private bool m_isDirty = false;
    private bool m_hasDirtyDescendants = false;

    private uint m_lastPaintFrame = 0;

    private bool m_shouldCache = false;
    public bool ShouldCache
    {
        get
        {
            return m_shouldCache && Application.SupportPaintCaching && !Application.HardwareAccel;
        }
        set
        {
            m_shouldCache = value;
        }
    }

    #endregion

    internal bool IsTopLevel => Parent == null && IsWindow;
    internal bool IsWindow => m_windowType == WindowType.Window || m_windowType == WindowType.Popup;
    /// <summary>
    /// Difference between this and <see cref="Visible"/> is this also checks if this is just a normal widget.
    /// </summary>
    internal bool VisibleWidget => m_visible && m_windowType == WindowType.Widget;

    private WindowType m_windowType = WindowType.Widget;
    public enum WindowType
    {
        /// <summary>
        /// The default type for <see cref="Widgets.Widget"/>. Widgets of this type are child widgets if they have a parent,
        /// and independent windows if they have no parent.
        /// </summary>
        Widget = 0,

        /// <summary>
        /// Indicates that the window is a window, usually with a window system frame and a title bar, irrespective of
        /// whether the widget has a parent or not. Note that it's not possible to unset this flag if the widget does
        /// not have a parent.
        /// </summary>
        Window,

        /// <summary>
        /// Indicates that the widget is a window that should be decorated as a dialog (i.e., typically no maximize and
        /// minimize buttons in the title bar).
        /// </summary>
        Dialog,

        /// <summary>
        /// Indicates that the widget is a pop-up top-level window, i.e. that it is a modal, but has a window system frame
        /// appropriate for pop-up menus.
        /// </summary>
        Popup,

        /// <summary>
        /// Indicates that the widget is a tool window.
        /// </summary>
        Tool,

        /// <summary>
        /// Indicates that the widget is a tooltip.
        /// </summary>
        ToolTip
    }

    public Widget(Widget? parent = null, WindowType winType = WindowType.Widget)
    {
        if (parent == this)
            throw new Exception("Cannot parent a Widget to itself!");

        m_name = GetType().Name;
        m_windowType = winType;

        if (parent != null)
        {
            SetParent(parent);
        }
        else
        {
            // All top level widgets are windows
            if (winType == WindowType.Widget)
            {
                m_windowType |= WindowType.Window;
            }
        }

        if (IsTopLevel)
        {
            if (!Application.HeadlessMode)
            {
                Application.Instance!.TopLevelWidgets.Add(this);
            }
            Visible = false;
        }

        TriggerRepaint();
    }

    /// <summary>
    /// Shows the widget and its child widgets.
    /// 
    /// For child windows, this is the equivalent to calling `<see cref="Visible"/> = true`.
    /// </summary>
    public void Show()
    {
        m_visible = true;

        if (IsWindow)
        {
            CreateWinID();
        }

        if (m_nativeWindow != null)
        {
            m_nativeWindow.Window.Size = new System.Drawing.Size(m_width, m_height);
            m_nativeWindow.CreateFrameBuffer(m_width, m_height);

            m_nativeWindow.Window.Position = new System.Drawing.Point(X, Y);
            if (m_windowType == WindowType.Window)
            {
                // @HACK
                m_nativeWindow.Center();
            }
            if (m_nativeWindow.ParentWindow != null)
            {
                // Inherit the parent window's icon by default
                m_nativeWindow.Window.CopyIconFromWindow(m_nativeWindow.ParentWindow.Window);
            }
            m_nativeWindow.Window.Show();
        }

        TriggerRepaint();
        InvalidateLayout(true);
        NotifyLayoutChange();

        OnShown();

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

    public void Hide()
    {
        m_visible = false;
        m_nativeWindow?.Window.Hide();
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
            m_parent.TriggerRepaint();
        }

        m_parent = parent;

        if (m_parent != null)
        {
            m_parent.Children.Add(this);

            if (m_parent.Layout != null && m_parent.Visible)
                m_parent.InvalidateLayout();

            if (Layout != null)
                InvalidateLayout();

            m_parent.TriggerRepaint();
        }

        TriggerRepaint();
        NotifyLayoutChange(); // In case grandparent needs to update layout too
    }

    public void SetPosition(int x, int y)
    {
        m_x = x;
        m_y = y;

        if (m_nativeWindow != null)
            m_nativeWindow.Window.Position = new System.Drawing.Point(m_x, m_y);
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
            m_nativeWindow.Window.Size = new System.Drawing.Size(m_width, m_height);
        }

        dispatchResize();
        CallResizeEvents();
    }

    public void TriggerRepaint()
    {
        triggerRepaint();
    }

    /// <summary>
    /// Sets the title of the window (if this is a top level widget).
    /// </summary>
    public void SetWindowTitle(string title)
    {
        if (m_nativeWindow == null)
            return;

        m_nativeWindow!.Window.Title = (title);
    }

    /// <summary>
    /// Forces the window to be created.
    /// Usually, this is deferred until Show() if the widget is top level.
    /// 
    /// ONLY call this method if the widget is a window type!
    /// </summary>
    public void CreateWinID()
    {
        if (!IsWindow)
            throw new Exception("Widget is not of a window type.");

        if (m_nativeWindow == null)
            initializeWindow();
    }

    public bool HitTest(int x, int y)
    {
        return (Visible) && (x >= 0 && y >= 0 && x < m_width && y < m_height);
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

    /// <summary>
    /// Deletes the widget from the hierarchy and disposes anything it may have allocated.
    /// </summary>
    public void Delete()
    {
        Dispose();
    }

    public virtual void Dispose()
    {
        Application.Instance!.TopLevelWidgets.Remove(this);
        if (m_nativeWindow != null)
        {
            WindowRegistry.Remove(new(m_nativeWindow, this));
        }

        m_parent?.m_children.Remove(this);
        m_parent = null;

        m_nativeWindow?.Dispose();
        m_cachedSurface?.Dispose();

        foreach (var child in m_children.ToList())
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

    public virtual void OnShown()
    {
    }

    public virtual void OnUpdate(double dt)
    {
    }

    #endregion

    #region Internal methods

    internal void Paint(SKCanvas canvas, SKRect clipRect, SkiaWindow window)
    {
        if (ShouldCache)
        {
            paintCache(canvas, clipRect, window);
        }
        else
        {
            paintNoCache(canvas, clipRect, window);
        }
        m_hasDirtyDescendants = false;
    }


    /// <summary>
    /// Paints to the cache canvas, then paints the cache to the canvas.
    /// </summary>
    private void paintCache(SKCanvas canvas, SKRect clipRect, SkiaWindow window)
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

        if (m_isDirty || m_hasDirtyDescendants)
        {
            m_lastPaintFrame = Application.CurrentFrame;
            m_isDirty = false;

            bool recreateTexture = false;

            if (!IsTopLevel)
            {
                /*
                if (m_cachedBitmap != null || m_width != m_cachedWidth || m_height != m_cachedHeight)
                {
                    m_cachedBitmap?.Dispose();
                    m_cachedBitmap = new SKBitmap(m_width, m_height);
                    m_cachedWidth = m_width;
                    m_cachedHeight = m_height;
                }
                */

                if (m_width != m_cachedWidth || m_height != m_cachedHeight)
                {
                    recreateTexture = true;
                    m_cachedBitmap?.Dispose();
                    m_cachedBitmap = new SKBitmap(m_width, m_height);

                    m_cachedWidth = m_width;
                    m_cachedHeight = m_height;
                }
            }

            // using var recorder = new SKPictureRecorder();
            // var paintCanvas = recorder.BeginRecording(new SKRect(0, 0, m_width, m_height));

            using (var paintCanvas = new SKCanvas(m_cachedBitmap))
            {
                Console.WriteLine("OnPaint");
                paintCanvas.Clear(SKColors.Transparent);
                (this as IPaintHandler)?.OnPaint(paintCanvas);

                if (m_children.Count > 0)
                {
                    foreach (var child in m_children)
                    {
                        if (!child.VisibleWidget)
                            continue;

                        child.Paint(paintCanvas, clipRect, window);
                    }
                }
            }

            // m_cachedPicture = recorder.EndRecording();

            unsafe
            {
                if (recreateTexture)
                {
                    var surface = SDL3.SDL_CreateSurfaceFrom(m_width, m_height, SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888, m_cachedBitmap!.GetPixels(), m_cachedBitmap.RowBytes);

                    if (m_cachedRenderTexture != null)
                    {
                        SDL3.SDL_DestroyTexture(m_cachedRenderTexture);
                    }
                    m_cachedRenderTexture = SDL3.SDL_CreateTextureFromSurface(window.SDLRenderer, surface);

                    SDL3.SDL_DestroySurface(surface);
                }
                else
                {
                    SDL3.SDL_UpdateTexture(m_cachedRenderTexture, null, m_cachedBitmap!.GetPixels(), m_cachedBitmap.RowBytes);
                }
            }
        }

        if (m_cachedBitmap != null)
        {
            // canvas.DrawBitmap(m_cachedBitmap, m_x, m_y);
        }
        if (m_cachedPicture != null)
        {
            // m_cachedPicture.Playback(canvas);
            // canvas.DrawPicture(m_cachedPicture, 0, 0);
        }

        canvas.Restore();
    }

    /// <summary>
    /// Paints to the canvas directly.
    /// </summary>
    private void paintNoCache(SKCanvas canvas, SKRect clipRect, SkiaWindow window)
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
        if (!IsWindow)
        {
            // @INVESTIGATE
            // This should be acknolwedged at least, the position probably shouldn't change if the widget is a window?
            canvas.Translate(m_x, m_y);
        }
        canvas.ClipRect(new(0, 0, m_width, m_height));

        (this as IPaintHandler)?.OnPaint(canvas);

        if (m_children.Count > 0)
        {
            foreach (var child in m_children)
            {
                if (!child.VisibleWidget)
                    continue;


                child.Paint(canvas, clipRect, window);
            }
        }

        (this as IPostPaintHandler)?.OnPostPaint(canvas);

        // clipStack.Pop();

        canvas.Restore();
    }

    static SKPaint s_debugPaint = new()
    {
        Style = SKPaintStyle.Stroke,
        IsAntialias = false
    };

    internal void DrawDebug(SKCanvas canvas)
    {
        if (m_height <= 0 || m_height <= 0 || !m_visible)
            return;

        // Cache debug mode?
        // Multiple debug modes?
        // Idk yet...
        //if (!ShouldCache)
        //    return;

        var globalPos = getGlobalPosition();

        canvas.Save();
        canvas.ResetMatrix();

        SKColor Lerp(SKColor from, SKColor to, float t)
        {
            // Clamp t between 0 and 1
            t = Math.Clamp(t, 0f, 1f);

            byte r = (byte)(from.Red + (to.Red - from.Red) * t);
            byte g = (byte)(from.Green + (to.Green - from.Green) * t);
            byte b = (byte)(from.Blue + (to.Blue - from.Blue) * t);
            byte a = (byte)(from.Alpha + (to.Alpha - from.Alpha) * t);

            return new SKColor(r, g, b, a);
        }

        var framesSinceLastPaint = Application.CurrentFrame - m_lastPaintFrame;
        var maxCounter = 60;

        s_debugPaint.Color = (ShouldCache ? Lerp(SKColors.Green, SKColors.Red, (float)framesSinceLastPaint / maxCounter) : SKColors.Blue);

        canvas.DrawRect(new SKRect(globalPos.X, globalPos.Y, globalPos.X + (m_width - 1), globalPos.Y + (m_height - 1)), s_debugPaint);

        canvas.Restore();

        if (m_children.Count > 0)
        {
            foreach (var child in m_children)
            {
                if (!child.VisibleWidget)
                    continue;

                child.DrawDebug(canvas);
            }
        }
    }

    internal void UpdateTopLevel(double dt)
    {
        if (m_nativeWindow == null)
            throw new Exception("Native window isn't set!");

        Update(dt);
    }

    internal void Update(double dt)
    {
        OnUpdate(dt);

        if (m_children.Count > 0)
        {
            foreach (var child in m_children)
            {
                if (!child.Enabled)
                    continue;

                child.Update(dt);
            }
        }
    }

    internal void RenderTopLevel(bool debug)
    {
        if (m_height == 0 || m_height == 0)
            return;
        if (m_nativeWindow == null)
            throw new Exception("Native window isn't set!");

        m_nativeWindow.BeginPresent();

        var rootClip = new SKRect(0, 0, m_width, m_height);

        // Paint!
        unsafe
        {
            SKSurface surface;
            if (Application.HardwareAccel)
            {
                surface = SKSurface.Create(m_nativeWindow.GRContext, m_nativeWindow.RenderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Bgra8888, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
            }
            else
            {
                surface = SKSurface.Create(m_nativeWindow.ImageInfo, m_nativeWindow.SDLSurface->pixels, m_nativeWindow.SDLSurface->pitch, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
            }
            var canvas = surface.Canvas;

            // canvas.Clear(SKColors.Magenta);
            canvas.Clear(SKColors.Transparent);

            // var rootStack = new Stack<SKRect>();
            // rootStack.Push(new SKRect(0, 0, m_width, m_height));

            Paint(canvas, rootClip, m_nativeWindow);

            if (debug)
            {
                DrawDebug(canvas);
            }
            canvas.Flush();

            m_nativeWindow.GRContext?.Flush();
            surface.Dispose();
        }

        if (!Application.HardwareAccel)
        {
            unsafe
            {
                // SDL3.SDL_UnlockTexture(m_nativeWindow.SDLTexture);
                if (!SDL3.SDL_UpdateTexture(m_nativeWindow.SDLTexture, null, m_nativeWindow.SDLSurface->pixels, m_nativeWindow.SDLSurface->pitch))
                {
                    Console.WriteLine(SDL3.SDL_GetError());
                }
            }
        }


        if (!Application.HardwareAccel)
        {
            unsafe
            {
                renderWidget(m_nativeWindow.SDLRenderer, m_x, m_y, rootClip);
            }
        }

        m_nativeWindow.EndPresent();
    }

    private unsafe void renderWidget(SDL_Renderer* renderer, int x, int y, SKRect clipRect)
    {
        var newX = m_x + x;
        var newY = m_y + y;

        var thisRect = new SKRect(newX, newY, newX + m_width, newY + m_height);
        var currentClip = SKRect.Intersect(clipRect, thisRect);

        if (currentClip.IsEmpty)
            return;

        if (m_cachedRenderTexture != null)
        {
            if (Application.HardwareAccel)
            {

            }
            else
            {
                SDL.SDL3.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
                var destRect = new SDL_FRect
                {
                    x = newX,
                    y = newY,
                    w = m_width,
                    h = m_height
                };
                /*

                SDL.SDL3.SDL_RenderRect(renderer, &test);
                */

                SDL3.SDL_RenderTexture(renderer, m_cachedRenderTexture, null, &destRect);
            }
        }

        foreach (var child in m_children)
        {
            if (!child.Visible)
                continue;

            unsafe
            {
                child.renderWidget(renderer, newX, newY, currentClip);
            }
        }
    }

    internal bool ShouldClose()
    {
        return m_nativeWindow?.ShouldClose ?? false;
    }

    public void PerformLayoutUpdate(LayoutFlushType type)
    {
        if (Layout != null)
        {
            var oldSize = (Width, Height);

            OnPreLayout();

            Layout.Start();
            switch (type)
            {
                case LayoutFlushType.All:
                    Layout.FitSizingPass(this);
                    Layout.GrowSizingPass(this);
                    Layout.PositionsPass(this);
                    break;
                case LayoutFlushType.Position:
                    Layout.PositionsPass(this);
                    break;
                case LayoutFlushType.Size:
                    Layout.FitSizingPass(this);
                    Layout.GrowSizingPass(this);
                    break;
            }
            Layout.End();

            OnPostLayout();
            OnPostLayoutUpdate?.Invoke();

            TriggerRepaint();


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

            LayoutQueue.Enqueue(this, LayoutFlushType.All);
            shouldInvalidateChildren = true;
        }

        if (shouldInvalidateChildren)
        {
            foreach (var child in m_children)
            {
                if (child == null)
                    continue;
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
        if (IsWindow)
            return new(0, 0);

        var x = m_x;
        var y = m_y;

        Widget? current = m_parent;
        while (current != null)
        {
            if (current.IsWindow)
                break;

            x += current.m_x;
            y += current.m_y;
            current = current.m_parent;
        }

        return new(x, y);
    }

    private void dispatchResize()
    {
        var isFlusing = LayoutQueue.IsFlusing;

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

        TriggerRepaint();
    }

    private void initializeWindow()
    {
        if (m_nativeWindow != null)
            return;

        Console.WriteLine($"Initialized top level widget of type: {GetType().Name}");

        WindowFlags flags = WindowFlags.None;
        if (m_windowType == WindowType.Popup)
        {
            flags |= WindowFlags.PopupMenu;
        }
        SkiaWindow? parentWindow = null;
        var parentWidgetCheck = m_parent;
        while (parentWindow == null && parentWidgetCheck != null)
        {
            parentWindow = parentWidgetCheck.m_nativeWindow;
            parentWidgetCheck = parentWidgetCheck.Parent;
        }

        m_nativeWindow = new(this, GetType().Name, flags, parentWindow);
        WindowRegistry.Register(new(m_nativeWindow, this));

        m_nativeWindow.Window.Resized += delegate ()
        {
            onNativeWindowResizeEvent(m_nativeWindow.Window.Size.Width, m_nativeWindow.Window.Size.Height);
        };
        m_nativeWindow.Window.MouseMove += delegate (System.Numerics.Vector2 pos)
        {
            onNativeWindowMouseEvent((int)pos.X, (int)pos.Y, MouseEventType.Move);
        };
        m_nativeWindow.Window.MouseDown += delegate(System.Numerics.Vector2 pos, MouseButton button)
        {
            onNativeWindowMouseEvent((int)pos.X, (int)pos.Y, MouseEventType.Down);
        };
        m_nativeWindow.Window.MouseUp += delegate (System.Numerics.Vector2 pos, MouseButton button)
        {
            onNativeWindowMouseEvent((int)pos.X, (int)pos.Y, MouseEventType.Up);
        };
        m_nativeWindow.Window.MouseWheel += delegate (System.Numerics.Vector2 pos, System.Numerics.Vector2 delta, bool precise)
        {
            onNativeWindowMouseEvent((int)pos.X, (int)pos.Y, MouseEventType.Wheel, (int)delta.X, (int)delta.Y);
        };
        m_nativeWindow.Window.MouseEntered += delegate()
        {
            // I don't know what I'd use this for right now
        };
        m_nativeWindow.Window.MouseLeft += delegate ()
        {
            // Simulate mouse exiting
            m_lastHovered?.handleMouseLeave();
            m_lastHovered = null;
        };
    }

    private void triggerRepaint()
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
        var thisX = (IsWindow) ? 0 : this.m_x;
        var thisY = (IsWindow) ? 0 : this.m_y;

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
            if (!child.VisibleWidget)
                continue;

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
            if (!child.VisibleWidget)
                continue;

            if (child.HitTest(x - child.m_x, y - child.m_y))
                return child;
        }
        return null;
    }

    #endregion

    #region Native window events

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

        // Console.WriteLine($"{this.Name}, {hovered?.Name}, ({mouseX}, {mouseY})");

        // If there's a mouse grabber, it always receives input!
        if (s_mouseGrabber != null && s_mouseGrabber.Enabled)
        {
            var (lx, ly) = getLocalPosition(s_mouseGrabber, mouseX, mouseY);

            switch (type)
            {
                case MouseEventType.Move:
                    (s_mouseGrabber as IMouseMoveHandler)?.OnMouseMove(lx, ly);
                    break;

                case MouseEventType.Down:
                    (s_mouseGrabber as IMouseDownHandler)?.OnMouseDown(lx, ly);
                    break;

                case MouseEventType.Up:
                    (s_mouseGrabber as IMouseUpHandler)?.OnMouseUp(lx, ly);

                    if (s_mouseGrabber == hovered)
                    {
                        (s_mouseGrabber as IMouseClickHandler)?.OnMouseClick(lx, ly);
                    }

                    s_mouseGrabber = null;
                    break;

                case MouseEventType.Wheel:
                    (s_mouseGrabber as IMouseWheelHandler)?.OnMouseScroll(lx, ly, deltaX, deltaY);
                    break;
            }

            return;
        }

        // No grabber - do regular hit testing.
        if (hovered != null)
        {
            switch (type)
            {
                case MouseEventType.Down:
                    if (bubbleMouseEvent(hovered, type, mouseX, mouseY, deltaX, deltaY))
                    {
                        s_mouseGrabber = hovered;
                    }
                    break;

                case MouseEventType.Up:
                    bool upHandled = bubbleMouseEvent(hovered, type, mouseX, mouseY, deltaX, deltaY);

                    if (s_mouseGrabber == hovered && upHandled)
                    {
                        var (lx, ly) = getLocalPosition(hovered, mouseX, mouseY);
                        (hovered as IMouseClickHandler)?.OnMouseClick(lx, ly);
                    }

                    s_mouseGrabber = null;
                    break;

                default:
                    bubbleMouseEvent(hovered, type, mouseX, mouseY, deltaX, deltaY);
                    break;
            }
        }
    }

    private bool bubbleMouseEvent(Widget widget, MouseEventType type, int globalX, int globalY, int dx = 0, int dy = 0)
    {
        while (widget != null)
        {
            if (!widget.Enabled)
            {
                widget = widget.Parent!;
                continue;
            }

            var (lx, ly) = getLocalPosition(widget, globalX, globalY);

            bool handled = type switch
            {
                MouseEventType.Move => (widget as IMouseMoveHandler)?.OnMouseMove(lx, ly) ?? false,
                MouseEventType.Down => (widget as IMouseDownHandler)?.OnMouseDown(lx, ly) ?? false,
                MouseEventType.Up => (widget as IMouseUpHandler)?.OnMouseUp(lx, ly) ?? false,
                MouseEventType.Wheel => (widget as IMouseWheelHandler)?.OnMouseScroll(lx, ly, dx, dy) ?? false,
                _ => false
            };

            if (handled) return true;
            if (widget.IsWindow) return false;

            widget = widget.Parent!;
        }

        return false;
    }

    private void onNativeWindowResizeEvent(int w, int h)
    {
        Resize(w, h);
        m_nativeWindow!.CreateFrameBuffer(w, h);

        TriggerRepaint();

        LayoutQueue.Flush();
        RenderTopLevel(Application.DebugDrawing);
    }

    #endregion
}