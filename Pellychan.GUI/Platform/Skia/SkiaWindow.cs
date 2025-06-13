using Pellychan.GUI.Input;
using Pellychan.GUI.Widgets;
using SDL2;
using SkiaSharp;

namespace Pellychan.GUI.Platform.Skia;

internal class SkiaWindow
{
    public uint WindowID { get; private set; }

    public Widget ParentWidget { get; private set; } 

    public readonly IntPtr SdlWindow;
    public readonly IntPtr SdlRenderer;
    public IntPtr SdlTexture;

    public SKImageInfo ImageInfo { get; private set; }
    public SKSurface? Surface { get; private set; }

    public bool ShouldClose = false;

    private IntPtr m_pixels;
    private int m_pitch;

    private MouseCursor.CursorType? m_currentCursor = null;
    private MouseCursor.CursorType? m_lastCursorShape = null;

    public string Title { get; private set; } = string.Empty;

    private bool m_windowCreated = false;

    #region Events

    public delegate void OnWindowResizeHandler(int w, int h);
    public delegate void OnWindowCloseHandler();
    public delegate void OnMouseMovedHandler(int x, int y);
    public delegate void OnMouseEventHandler(int x, int y, MouseEventType type);

    public event OnWindowResizeHandler? OnWindowResize;
    public event OnWindowCloseHandler? OnWindowClose;
    public event OnMouseMovedHandler? OnMouseMoved;
    public event OnMouseEventHandler? OnMouseEvent;

    #endregion

    public SkiaWindow(Widget parent, int width, int height, string title)
    {
        ParentWidget = parent;

        SdlWindow = SDL.SDL_CreateWindow(title,
            SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
            width, height, SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

        WindowID = SDL.SDL_GetWindowID(SdlWindow);

        SdlRenderer = SDL.SDL_CreateRenderer(SdlWindow, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

        m_windowCreated = true;
    }

    public void CreateFrameBuffer(int w, int h)
    {
        ImageInfo = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());

        if (SdlTexture != IntPtr.Zero)
        {
            SDL.SDL_DestroyTexture(SdlTexture);
        }

        // Create SDL texture as the drawing target
        SdlTexture = SDL.SDL_CreateTexture(SdlRenderer,
            SDL.SDL_PIXELFORMAT_ARGB8888,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            w, h);

        Lock();

        Surface?.Dispose();
        Surface = SKSurface.Create(ImageInfo, m_pixels, m_pitch, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));

        Unlock();
    }

    public void Dispose()
    {
        Surface?.Dispose();

        SDL.SDL_DestroyTexture(SdlTexture);
        SDL.SDL_DestroyRenderer(SdlRenderer);
        SDL.SDL_DestroyWindow(SdlWindow);

        m_windowCreated = false;
    }

    public void Lock()
    {
        SDL.SDL_LockTexture(SdlTexture, IntPtr.Zero, out m_pixels, out m_pitch);
    }

    public void Unlock()
    {
        SDL.SDL_UnlockTexture(SdlTexture);
    }

    public void Present()
    {
        SDL.SDL_RenderClear(SdlRenderer);
        SDL.SDL_RenderCopy(SdlRenderer, SdlTexture, IntPtr.Zero, IntPtr.Zero);
        SDL.SDL_RenderPresent(SdlRenderer);
    }

    public void SetTitle(string title)
    {
        Title = title;
        
        if (m_windowCreated)
        {
            SDL.SDL_SetWindowTitle(SdlWindow, title);
        }
    }

    public void HandleEvent(SDL.SDL_Event e)
    {
        switch (e.type)
        {
            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                OnMouseEvent?.Invoke(e.button.x, e.button.y, MouseEventType.Down);
                break;

            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                // dispatchMouseEvent(e.button.x, e.button.y, MouseEventType.Up);
                OnMouseEvent?.Invoke(e.button.x, e.button.y, MouseEventType.Up);
                break;

            case SDL.SDL_EventType.SDL_MOUSEMOTION:
                OnMouseMoved?.Invoke(e.motion.x, e.motion.y);
                break;

            case SDL.SDL_EventType.SDL_WINDOWEVENT:

                switch (e.window.windowEvent)
                {
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                        {
                            OnWindowResize?.Invoke(e.window.data1, e.window.data2);
                        }
                        break;

                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                        {
                        }
                        break;


                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                        {
                            OnWindowClose?.Invoke();
                            ShouldClose = true;
                        }
                        break;
                }

                break;
        }
    }

    /// <summary>
    /// Shows the window, if it's hidden.
    /// </summary>
    public void Show()
    {
        SDL.SDL_ShowWindow(SdlWindow);
    }

    /// <summary>
    /// Resizes the window.
    /// </summary>
    public void Resize(int width, int height)
    {
        SDL.SDL_SetWindowSize(SdlWindow, width, height);
    }

    /// <summary>
    /// Centers the window.
    /// </summary>
    public void Center()
    {
        // Get the window's current display index
        int displayIndex = SDL.SDL_GetWindowDisplayIndex(SdlWindow);
        if (displayIndex < 0)
        {
            throw new InvalidOperationException($"Failed to get window display index: {SDL.SDL_GetError()}");
        }

        // Get the bounds of the display
        SDL.SDL_Rect displayBounds;
        if (SDL.SDL_GetDisplayBounds(displayIndex, out displayBounds) != 0)
        {
            throw new InvalidOperationException($"Failed to get display bounds: {SDL.SDL_GetError()}");
        }

        // Get the window size
        int windowWidth, windowHeight;
        SDL.SDL_GetWindowSize(SdlWindow, out windowWidth, out windowHeight);

        // Calculate the centered position
        int centeredX = displayBounds.x + (displayBounds.w - windowWidth) / 2;
        int centeredY = displayBounds.y + (displayBounds.h - windowHeight) / 2;

        // Set the window position
        SDL.SDL_SetWindowPosition(SdlWindow, centeredX, centeredY);
    }
}