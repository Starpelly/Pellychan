﻿using Pellychan.GUI.Framework.Platform;
using Pellychan.GUI.Framework.Platform.Skia;
using Pellychan.GUI.Input;
using Pellychan.GUI.Layouts;
using SDL;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public partial class Widget : IDisposable
{
    private Guid m_uid = Guid.NewGuid();
    public Guid Guid => m_uid;

    private string m_name = string.Empty;
    public string Name
    {
        get => m_name;
        set => m_name = value;
    }

    /// <summary>
    /// Marked for disposing.
    /// </summary>
    private bool m_deleting = false;
    private bool m_disposed = false;
    private bool m_hovered = false;

    internal bool IsDeleting => m_deleting;

    private Widget? m_lastHovered = null;
    private static Widget? s_mouseGrabber = null;

    static readonly SKPaint s_debugPaint = new()
    {
        Style = SKPaintStyle.Stroke,
        IsAntialias = false
    };

    #region Geometry

    private int m_x = 0;
    private int m_y = 0;
    private int m_width = 0;
    private int m_height = 0;

    public bool DisableResizeEvents = false;

    /// <summary>
    /// X position of the widget relative to its parent, in pixels.
    /// </summary>
    public int X
    {
        get => m_x;
        set
        {
            m_x = value;
        }
    }

    /// <summary>
    /// Y position of the widget relative to its parent, in pixels.
    /// </summary>
    public int Y
    {
        get => m_y;
        set
        {
            m_y = value;
        }
    }

    /// <summary>
    /// Width of the widget, in pixels.
    /// </summary>
    public int Width
    {
        get => m_width;
        set
        {
            if (m_width != value)
            {
                m_width = value;
                dispatchResize();

                if (!DisableResizeEvents)
                    this.NotifyLayoutChange();
            }
        }
    }

    /// <summary>
    /// Height of the widget, in pixels.
    /// </summary>
    public int Height
    {
        get => m_height;
        set
        {
            if (m_height != value)
            {
                m_height = value;
                dispatchResize();

                if (!DisableResizeEvents)
                    this.NotifyLayoutChange();
            }
        }
    }

    public SKSizeI Size => new(m_width, m_height);

    public SKRect Rect => new(m_x, m_y, m_x + m_width, m_y + m_height);

    #endregion

    #region Tree

    private Widget? m_parent;
    public Widget? Parent
    {
        get => m_parent;
    }
    private readonly List<Widget> m_children = [];
    public List<Widget> Children => m_children;

    #endregion

    #region Attributes

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
                EnqueueLayout();
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

    internal bool ShouldDraw => Visible && !m_deleting;

    /// <summary>
    /// Doesn't look up the tree to see if the widget's visible.
    /// </summary>
    internal bool ShouldDrawFast => m_visible && !m_deleting;

    private bool m_catchCursorEvents = true;

    /// <summary>
    /// If true, the widget will block other UI from catching cursor events.
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

    #endregion

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
                EnqueueLayout();
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
                EnqueueLayout();
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

    #region Cursor

    public MouseCursor.CursorType? CursorShape = null;

    #endregion

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

    #region Windowing

    internal bool IsTopLevel => Parent == null && IsWindow;
    internal bool IsWindow => m_windowType == WindowType.Window || m_windowType == WindowType.Popup;

    /// <summary>
    /// Difference between this and <see cref="Visible"/> is this also checks if this is just a normal widget.
    /// </summary>
    internal bool VisibleWidget => m_windowType == WindowType.Widget && ShouldDrawFast;

    private readonly WindowType m_windowType = WindowType.Widget;

    #endregion

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
                Application.Instance!.AddTopLevel(this);
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

        if (m_windowType == WindowType.Popup)
        {
            s_openPopupMenu = this;
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

        // We also enqueue all the children because we didn't queue anything the first time because the widget holding
        // the children wasn't visible yet. Maybe this should change?
        EnqueueLayout(true);
        // NotifyLayoutChange();

        OnShown();
    }

    public void Hide()
    {
        m_visible = false;
        m_nativeWindow?.Window.Hide();

        if (s_openPopupMenu == this)
        {
            s_openPopupMenu = null;
        }
    }

    public void SetModal(bool modal)
    {

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
                m_parent.EnqueueLayout();
            m_parent.TriggerRepaint();
        }

        m_parent = parent;

        if (m_parent != null)
        {
            m_parent.Children.Add(this);

            /*
            if (m_parent.Layout != null && m_parent.ShouldDraw)
                m_parent.EnqueueLayout();

            if (Layout != null)
                EnqueueLayout();
            */

            m_parent.TriggerRepaint();
        }

        TriggerRepaint();
        NotifyLayoutChange();
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
        callResizeEvents();
    }

    public void TriggerRepaint()
    {
        if (m_isDirty) return;

        m_isDirty = true;
        Parent?.markChildDirty();
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
        return (ShouldDraw) && (x >= 0 && y >= 0 && x < m_width && y < m_height);
    }

    internal void RequestWindowClose()
    {
        Delete();
    }

    /// <summary>
    /// Deletes the widget from the hierarchy and disposes anything it may have allocated.
    /// </summary>
    public void Delete()
    {
        m_deleting = true;
        Application.Instance!.EnqueueWidgetForDeletion(this);
        // Dispose();
    }

    public virtual void Dispose()
    {
        if (IsTopLevel)
        {
            Application.Instance!.RemoveTopLevel(this);
        }

        // Dispose children first
        foreach (var child in m_children.ToList())
        {
            child.Dispose();
        }

        m_parent?.m_children.Remove(this);
        m_parent = null;

        m_cachedSurface?.Dispose();
        m_nativeWindow?.Dispose();

        m_disposed = true;

        if (s_openPopupMenu == this)
            s_openPopupMenu = null;

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
                renderDebug(canvas);
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

    public void PerformLayoutUpdate(LayoutFlushType type)
    {
        if (Layout != null)
        {
            var oldSize = (Width, Height);

            OnPreLayout();

            if (Name == "PostWidgetContainer")
            {
                var a = 0;
            }
            if (Name == "Posts Lists Holder")
            {
                var a = 0;
            }

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

            if (Width != oldSize.Width || Height != oldSize.Height)
                dispatchResize();

            OnPostLayout();
            OnPostLayoutUpdate?.Invoke();


            TriggerRepaint();
        }
    }

    public void EnqueueLayout(bool doChildren = false)
    {
        if (!ShouldDraw)
            return;

        if (Layout != null)
        {
            if (!Layout.PerformingPasses)
            {
                LayoutQueue.Enqueue(this, LayoutFlushType.All);
            }
        }

        if (doChildren)
        {
            foreach (var child in m_children)
            {
                if (child == null)
                    continue;
                child.EnqueueLayout(doChildren);
            }
        }
    }

    /// <summary>
    /// Tells all parents to invalidate layouts (if they have layouts).
    /// </summary>
    public void NotifyLayoutChange()
    {
        var p = Parent;
        while (p != null)
        {
            if (p.Layout != null)
            {
                p.EnqueueLayout();
            }
            else
            {
                break;
            }

            p = p.Parent;
        }
    }

    internal void callResizeEvents()
    {
        // Console.WriteLine($"Calling resize events for type: {GetType().Name}");

        (this as IResizeHandler)?.OnResize(m_width, m_height);
    }

    #endregion

    #region Private Methods

    private static (int, int) getGlobalPosition(Widget widget)
    {
        if (widget.IsWindow)
            return (0, 0);

        var x = widget.m_x;
        var y = widget.m_y;

        Widget? current = widget.m_parent;
        while (current != null)
        {
            if (current.IsWindow)
                break;

            x += current.m_x;
            y += current.m_y;
            current = current.m_parent;
        }

        return (x, y);
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

    private void dispatchResize()
    {
        if (DisableResizeEvents)
            return;

        var isFlusing = LayoutQueue.IsFlusing;

        if (Layout != null)
            isFlusing = Layout.PerformingPasses;

        if (!isFlusing)
        {
            if (Layout != null)
            {
                EnqueueLayout();
            }
            else
            {
                callResizeEvents();
            }

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

        m_nativeWindow.Window.Resized += delegate ()
        {
            onNativeWindowResizeEvent(m_nativeWindow.Window.Size.Width, m_nativeWindow.Window.Size.Height);
        };
        m_nativeWindow.Window.MouseMove += delegate (System.Numerics.Vector2 pos)
        {
            onNativeWindowMouseEvent((int)pos.X, (int)pos.Y, MouseEventType.Move, MouseButton.None);
        };
        m_nativeWindow.Window.MouseDown += delegate(System.Numerics.Vector2 pos, MouseButton button)
        {
            onNativeWindowMouseEvent((int)pos.X, (int)pos.Y, MouseEventType.Down, button);
        };
        m_nativeWindow.Window.MouseUp += delegate (System.Numerics.Vector2 pos, MouseButton button)
        {
            onNativeWindowMouseEvent((int)pos.X, (int)pos.Y, MouseEventType.Up, button);
        };
        m_nativeWindow.Window.MouseWheel += delegate (System.Numerics.Vector2 pos, System.Numerics.Vector2 delta, bool precise)
        {
            onNativeWindowMouseEvent((int)pos.X, (int)pos.Y, MouseEventType.Wheel, MouseButton.None, (int)delta.X, (int)delta.Y);
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

    private bool isFocusedHack()
    {
        if (s_openPopupMenu != null && this is not MenuBar)
        {
            if (s_openPopupMenu != this)
            {
                return false;
            }
        }

        return true;
    }

    private void markChildDirty()
    {
        if (m_hasDirtyDescendants) return;
        m_hasDirtyDescendants = true;
        Parent?.markChildDirty();
    }

    private Widget? findHoveredWidget(int x, int y, bool checkRaycast, bool inFocused = false)
    {
        var thisX = (IsWindow) ? 0 : this.m_x;
        var thisY = (IsWindow) ? 0 : this.m_y;

        int localX = x - thisX;
        int localY = y - thisY;

        bool canCatchEvents = true;
        if (checkRaycast)
        {
            if (!inFocused)
            {
                if (isFocusedHack())
                {
                    inFocused = true;
                }
            }

            if (!CatchCursorEvents || !inFocused)
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

            var result = child.findHoveredWidget(localX, localY, checkRaycast, inFocused);
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
}