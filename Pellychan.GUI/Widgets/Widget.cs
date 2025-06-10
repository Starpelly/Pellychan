using Pellychan.GUI.Input;
using Pellychan.GUI.Platform.Skia;
using SDL2;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class Widget : IDisposable
{
    public Widget? Parent { get; private set; }
    private readonly List<Widget> m_children = [];

    public int X = 0;
    public int Y = 0;
    public int Width;
    public int Height;

    public bool Visible = true;

    private bool IsHovered { get; set; } = false;
    private Widget? m_lastHovered = null;

    private bool m_isDirty = false;
    private bool m_hasDirtyDescendants = false;

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
        Parent = parent;
        parent?.AddChild(this);
        Update();

        if (IsTopLevel)
        {
            Application.Instance!.TopLevelWidgets.Add(this);
            initializeIfTopLevel();
        }
    }

    private void initializeIfTopLevel()
    {
        if (!IsTopLevel) return;

        m_nativeWindow = new(this, Width, Height, GetType().Name);
        WindowRegistry.Register(m_nativeWindow);

        m_nativeWindow.OnWindowResize += delegate (int w, int h)
        {
            m_nativeWindow!.CreateFrameBuffer(w, h);
            Resize(w, h);
            
            Update();
        };
        m_nativeWindow.OnMouseEvent += dispatchMouseEvent;
        m_nativeWindow.OnMouseMoved += handleMouseMove;
    }

    public void UpdateAndRender()
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

    /// <summary>
    /// Shows top level widgets, will automatically show any children along with it.
    /// </summary>
    public void Show()
    {
        Visible = true;

        if (IsTopLevel)
        {
            m_nativeWindow?.CreateFrameBuffer(Width, Height);
            m_nativeWindow?.Center();
            m_nativeWindow?.Show();
        }
    }

    public void AddChild(Widget child, int x = 0, int y = 0)
    {
        child.Parent = this;
        child.X = x;
        child.Y = y;
        m_children.Add(child);
    }

    public void RemoveChild(Widget child)
    {
        child.Parent = null;
        m_children.Remove(child);
    }

    public void SetPosition(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;

        m_nativeWindow?.Resize(width, height);

        OnResize(width, height);
    }

    public void SetRect(int x, int y, int width, int height)
    {
        this.X = x;
        this.Y = y;
        Width = width;
        Height = height;
    }

    public void Paint(SKCanvas canvas)
    {
        if (Width <= 0 || Height <= 0)
            return;

        canvas.Save();
        canvas.Translate(X, Y);

        if (m_isDirty)
        {
            m_isDirty = false;

            // We'll only recreate the surface when the widget is resized
            // AND only if we're not a top level widget
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

            var surface = (IsTopLevel) ? m_nativeWindow!.Surface! : m_cachedSurface!;

            var sc = surface.Canvas;
            sc.Clear(SKColors.Transparent);
            OnPaint(sc);

            // We can recreate the image every paint, that's fine.
            m_cachedImage?.Dispose();
            m_cachedImage = surface.Snapshot();
        }

        // Draw the cached image to the real canvas
        if (m_cachedImage != null)
        {
            canvas.DrawImage(m_cachedImage, 0, 0);
        }

        foreach (var child in m_children)
        {
            child.Paint(canvas);
        }

        canvas.Restore();

        m_hasDirtyDescendants = false;
    }

    public bool ShouldClose()
    {
        return IsTopLevel && m_nativeWindow!.ShouldClose;
    }

    public virtual void Dispose()
    {
        m_nativeWindow?.Dispose();
        m_cachedSurface?.Dispose();

        foreach (var child in m_children)
            child.Dispose();

        GC.SuppressFinalize(this);
    }

    public void Update()
    {
        invalidate();
    }

    public bool HitTest(int x, int y)
    {
        return Visible && (x >= 0 && y >= 0 && x < Width && y < Height);
    }

    /// <summary>
    /// Sets the title of the window.
    /// </summary>
    public void SetWindowTitle(string title)
    {
        m_nativeWindow?.SetTitle(title);
    }

    #region Events

    public virtual void OnPaint(SKCanvas canvas)
    {
    }

    public virtual void OnResize(int width, int height)
    {
    }

    public virtual void OnMouseEnter() { }
    public virtual void OnMouseLeave() { }
    public virtual void OnMouseMove(int x, int y) { }
    public virtual void OnMouseDown(int x, int y) { }
    public virtual void OnMouseUp(int x, int y) { }

    #endregion

    #region Private Methods

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

    private Widget? findHoveredWidget(int x, int y)
    {
        if (x < X || y < Y || x > X + Width || y > Y + Height)
            return null;

        foreach (var child in m_children.AsReadOnly().Reverse()) // top to bottom
        {
            var localX = x - child.X;
            var localY = y - child.Y;
            var hit = child.findHoveredWidget(x, y);
            if (hit != null)
                return hit;
        }

        return this;
    }

    private void handleMouseEnter()
    {
        if (!IsHovered)
        {
            IsHovered = true;
            OnMouseEnter();
        }
    }

    private void handleMouseLeave()
    {
        if (IsHovered)
        {
            IsHovered = false;
            OnMouseLeave();
        }
    }

    private void handleMouseMove(int x, int y)
    {
        var newHovered = findHoveredWidget((int)x, (int)y);
        
        var newCursorShape = newHovered?.CursorShape;

        if (newCursorShape.HasValue)
        {
            MouseCursor.Set(newCursorShape.Value);
        }

        if (m_lastHovered != newHovered)
        {
            m_lastHovered?.handleMouseLeave();
            newHovered?.handleMouseEnter();
        }

        newHovered?.OnMouseMove((int)x - X, (int)y - Y);
        m_lastHovered = newHovered;
    }

    private void dispatchMouseEvent(int mouseX, int mouseY, MouseEventType type)
    {
        foreach (var widget in m_children.Reverse<Widget>()) // top-most first
        {
            if (!widget.Visible)
                continue;

            if (widget.HitTest(mouseX - widget.X, mouseY - widget.Y))
            {
                switch (type)
                {
                    case MouseEventType.Move:
                        widget.OnMouseMove(mouseX - widget.X, mouseY - widget.Y);
                        break;
                    case MouseEventType.Down:
                        widget.OnMouseDown(mouseX - widget.X, mouseY - widget.Y);
                        break;
                    case MouseEventType.Up:
                        widget.OnMouseUp(mouseX - widget.X, mouseY - widget.Y);
                        break;
                }
                break; // stop at first hit
            }
        }
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

    #endregion
}