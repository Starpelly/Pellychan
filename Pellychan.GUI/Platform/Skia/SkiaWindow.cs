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

    public SkiaWindow(Widget parent, int width, int height, string title)
    {
        ParentWidget = parent;

        SdlWindow = SDL.SDL_CreateWindow(title,
            SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
            width, height, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

        WindowID = SDL.SDL_GetWindowID(SdlWindow);

        SdlRenderer = SDL.SDL_CreateRenderer(SdlWindow, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

        CreateFrameBuffer(width, height);
    }

    public void CreateFrameBuffer(int w, int h)
    {
        ImageInfo = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);

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
        Surface = SKSurface.Create(ImageInfo, m_pixels, m_pitch);

        Unlock();
    }

    public void Dispose()
    {
        Surface?.Dispose();

        SDL.SDL_DestroyTexture(SdlTexture);
        SDL.SDL_DestroyRenderer(SdlRenderer);
        SDL.SDL_DestroyWindow(SdlWindow);
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
        SDL.SDL_SetWindowTitle(SdlWindow, title);
    }
}