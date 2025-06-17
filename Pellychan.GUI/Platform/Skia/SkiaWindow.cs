using Pellychan.GUI.Input;
using Pellychan.GUI.Widgets;
using SDL;
using SkiaSharp;

namespace Pellychan.GUI.Platform.Skia;

internal unsafe class SkiaWindow
{
    public SDL_WindowID WindowID { get; private set; }

    public Widget ParentWidget { get; private set; } 

    public readonly SDL_Window* SdlWindow;
    public SDL_Renderer* SdlRenderer;
    public SDL_Texture* SdlTexture;

    public SKImageInfo ImageInfo { get; private set; }

    public bool ShouldClose = false;

    private IntPtr m_pixels;
    private int m_pitch;

    public IntPtr Pixels => m_pixels;
    public int Pitch => m_pitch;

    private MouseCursor.CursorType? m_currentCursor = null;
    private MouseCursor.CursorType? m_lastCursorShape = null;

    public string Title { get; private set; } = string.Empty;

    private bool m_windowCreated = false;

    #region Events

    public delegate void OnWindowResizeHandler(int w, int h);
    public delegate void OnWindowCloseHandler();
    // public delegate void OnMouseMovedHandler(int x, int y);
    public delegate void OnMouseEventHandler(int x, int y, MouseEventType type);

    public event OnWindowResizeHandler? OnWindowResize;
    public event OnWindowCloseHandler? OnWindowClose;
    // public event OnMouseMovedHandler? OnMouseMoved;
    public event OnMouseEventHandler? OnMouseEvent;

    #endregion

    public SkiaWindow(Widget parent, int width, int height, string title, bool tooltip, SkiaWindow? parentWindow = null)
    {
        ParentWidget = parent;

        var flags = SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;
        if (tooltip)
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS | SDL.SDL_WindowFlags.SDL_WINDOW_TOOLTIP;
        }
        else
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        }
        
        SdlWindow = SDL3.SDL_CreateWindow(title, width, height, flags);
        WindowID = SDL3.SDL_GetWindowID(SdlWindow);

        SdlRenderer = SDL3.SDL_CreateRenderer(SdlWindow, (byte*)null);

        m_windowCreated = true;
    }

    public void CreateFrameBuffer(int w, int h)
    {
        ImageInfo = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());

        if (SdlTexture != null)
        {
            SDL3.SDL_DestroyTexture(SdlTexture);
        }

        // Create SDL texture as the drawing target
        SdlTexture = SDL3.SDL_CreateTexture(SdlRenderer,
            SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888,
            SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            w, h);
    }

    public void Dispose()
    {
        // SkiaSurface?.Dispose();

        SDL3.SDL_DestroyTexture(SdlTexture);
        SDL3.SDL_DestroyRenderer(SdlRenderer);
        SDL3.SDL_DestroyWindow(SdlWindow);

        m_windowCreated = false;
    }

    public void Lock()
    {
        fixed (nint* pixelsPtr = &m_pixels)
        fixed (int* pitchPtr = &m_pitch)
        {
            SDL3.SDL_LockTexture(SdlTexture, null, pixelsPtr, pitchPtr);
        }
    }

    public void Unlock()
    {
        SDL3.SDL_UnlockTexture(SdlTexture);
    }

    public void Present()
    {
        SDL3.SDL_RenderClear(SdlRenderer);
        SDL3.SDL_RenderTexture(SdlRenderer, SdlTexture, null, null);

        SDL3.SDL_RenderPresent(SdlRenderer);
    }

    public void SetTitle(string title)
    {
        Title = title;
        
        if (m_windowCreated)
        {
            SDL3.SDL_SetWindowTitle(SdlWindow, title);
        }
    }

    public void HandleEvent(SDL.SDL_Event e)
    {
        switch (e.Type)
        {
            case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                OnMouseEvent?.Invoke((int)e.button.x, (int)e.button.y, MouseEventType.Down);
                break;

            case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                // dispatchMouseEvent(e.button.x, e.button.y, MouseEventType.Up);
                OnMouseEvent?.Invoke((int)e.button.x, (int)e.button.y, MouseEventType.Up);
                break;

            case SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                OnMouseEvent?.Invoke((int)e.motion.x, (int)e.motion.y, MouseEventType.Move);
                break;

            case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                {
                    OnWindowResize?.Invoke(e.window.data1, e.window.data2);
                }
                break;

            case SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                {
                    OnWindowClose?.Invoke();
                    ShouldClose = true;
                }
                break;
        }
    }

    /// <summary>
    /// Shows the window, if it's hidden.
    /// </summary>
    public void Show()
    {
        SDL3.SDL_ShowWindow(SdlWindow);
    }

    /// <summary>
    /// Resizes the window.
    /// </summary>
    public void Resize(int width, int height)
    {
        SDL3.SDL_SetWindowSize(SdlWindow, width, height);
    }

    /// <summary>
    /// Centers the window.
    /// </summary>
    public void Center()
    {
        // Get the window's current display index
        var displayIndex = SDL3.SDL_GetDisplayForWindow(SdlWindow);
        if (displayIndex < 0)
        {
            throw new InvalidOperationException($"Failed to get window display index: {SDL3.SDL_GetError()}");
        }

        // Get the bounds of the display
        SDL.SDL_Rect displayBounds;
        if (SDL3.SDL_GetDisplayBounds(displayIndex, &displayBounds) != true)
        {
            throw new InvalidOperationException($"Failed to get display bounds: {SDL3.SDL_GetError()}");
        }

        // Get the window size
        int windowWidth, windowHeight;
        SDL3.SDL_GetWindowSize(SdlWindow, &windowWidth, &windowHeight);

        // Calculate the centered position
        int centeredX = displayBounds.x + (displayBounds.w - windowWidth) / 2;
        int centeredY = displayBounds.y + (displayBounds.h - windowHeight) / 2;

        // Set the window position
        SDL3.SDL_SetWindowPosition(SdlWindow, centeredX, centeredY);
    }
}