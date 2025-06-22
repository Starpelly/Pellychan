using Pellychan.GUI.Platform.SDL3;
using Pellychan.GUI.Platform.Windows;
using Pellychan.GUI.Utils;
using Pellychan.GUI.Widgets;
using SDL;
using SkiaSharp;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static SDL.SDL3;

namespace Pellychan.GUI.Platform.Skia;

internal unsafe class SkiaWindow
{
    internal Widget ParentWidget { get; private set; }

    internal IWindow Window { get; private set; }

    internal SDL_Window* SDLWindowHandle => ((SDL3Window)Window).SDLWindowHandle;
    internal SDL_WindowID SDLWindowID => ((SDL3Window)Window).SDLWindowID;

    // Hardware acceleration
    internal SDL_GLContextState* SDLGLContext { get; private set; }
    internal GRGlInterface? InterfaceGL { get; private set; }
    internal GRContext? GRContext { get; private set; }
    internal GRBackendRenderTarget RenderTarget { get; private set; }

    // Software rendering mode
    internal SDL_Renderer* SDLRenderer { get; private set; }
    internal SDL_Texture* SDLTexture { get; private set; }
    internal SDL_Surface* SDLSurface { get; private set; }

    internal SKImageInfo ImageInfo { get; private set; }

    internal bool ShouldClose { get; private set; }

    private MouseCursor.CursorType? m_currentCursor = null;
    private MouseCursor.CursorType? m_lastCursorShape = null;

    // Win32 constants
    const int GWL_STYLE = -16;
    const int GWL_EXSTYLE = -20;
    const uint WS_POPUP = 0x80000000;
    const uint WS_EX_TOOLWINDOW = 0x00000080;
    const uint WS_EX_NOACTIVATE = 0x08000000;

    // Win32 APIs
    [DllImport("user32.dll")]
    static extern uint GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    static extern uint SetWindowLongPtr(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("dwmapi.dll")]
    static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [StructLayout(LayoutKind.Sequential)]
    struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    public SkiaWindow(Widget parent, string title, WindowFlags flags, SkiaWindow? parentWindow)
    {
        switch (RuntimeInfo.OS)
        {
            case RuntimeInfo.Platform.Windows:
                Debug.Assert(OperatingSystem.IsWindows());
                Window = new SDL3WindowsWindow();
                break;
            default:
                throw new InvalidOperationException($"Could not find a suitable window for the selected operating system ({RuntimeInfo.OS})");
        }

        Window.Create(parentWindow?.Window ?? null, flags);
        Center();

        Window.ExitRequested += delegate ()
        {
            ShouldClose = true;
        };

        ParentWidget = parent;

        Window.Title = title;

        if (Application.HardwareAccel)
        {
            SDLGLContext = SDL_GL_CreateContext(SDLWindowHandle);
            SDL_GL_MakeCurrent(SDLWindowHandle, SDLGLContext);

            InterfaceGL = GRGlInterface.Create();
            GRContext = GRContext.CreateGl(InterfaceGL);
        }
        else
        {
            SDLRenderer = SDL_CreateRenderer(SDLWindowHandle, (byte*)null);
        }
    }

    public void CreateFrameBuffer(int w, int h)
    {
        ImageInfo = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());

        if (Application.HardwareAccel)
        {
            RenderTarget?.Dispose();

            SDL_GetWindowSizeInPixels(SDLWindowHandle, &w, &h);
            // SDL_GL_GetIntegerv(SDL.SDL.GLAttribute.FramebufferBinding, out int framebuffer);

            var glInfo = new GRGlFramebufferInfo((uint)0, SKColorType.Rgba8888.ToGlSizedFormat());
            RenderTarget = new GRBackendRenderTarget(w, h, 0, 0, glInfo);
        }
        else
        {
            if (SDLTexture != null)
            {
                SDL_DestroyTexture(SDLTexture);
            }

            // Create SDL texture as the drawing target
            SDLTexture = SDL_CreateTexture(SDLRenderer,
                SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888,
                SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
                w, h);

            if (SDLSurface != null)
            {
                SDL_DestroySurface(SDLSurface);
            }
            // SDLSurface = SDL_CreateSurface(w, h, SDL_GetPixelFormatForMasks(32, 0, 0, 0, 0));
            SDLSurface = SDL_CreateSurface(w, h, SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888);
        }
    }

    public void Dispose()
    {
        // SkiaSurface?.Dispose();

        GRContext?.Dispose();
        InterfaceGL?.Dispose();

        if (SDLSurface != null)
        {
            SDL_DestroySurface(SDLSurface);
        }
        if (SDLTexture != null)
        {
            SDL_DestroyTexture(SDLTexture);
        }
        if (SDLGLContext != null)
        {
            SDL_GL_DestroyContext(SDLGLContext);
        }
        if (SDLRenderer != null)
        {
            SDL_DestroyRenderer(SDLRenderer);
        }

        Window.Dispose();
    }

    public void BeginPresent()
    {
        if (Application.HardwareAccel)
        {
            SDL_GL_MakeCurrent(SDLWindowHandle, SDLGLContext);
        }
    }

    public void EndPresent()
    {
        if (Application.HardwareAccel)
        {
            SDL_GL_SwapWindow(SDLWindowHandle);
        }
        else
        {
            SDL_RenderTexture(SDLRenderer, SDLTexture, null, null);
            SDL_RenderPresent(SDLRenderer);
            // SDL_RenderPresent(popupRenderer);
        }
    }

    internal static void PollEvents()
    {
        SDL3Window.pollSDLEvents();
    }

    SDL_Renderer* popupRenderer;

    /// <summary>
    /// Centers the window.
    /// </summary>
    public void Center()
    {
        // Get the window's current display index
        var displayIndex = SDL_GetDisplayForWindow(SDLWindowHandle);
        if (displayIndex < 0)
        {
            throw new InvalidOperationException($"Failed to get window display index: {SDL_GetError()}");
        }

        // Get the bounds of the display
        SDL_Rect displayBounds;
        if (SDL_GetDisplayBounds(displayIndex, &displayBounds) != true)
        {
            throw new InvalidOperationException($"Failed to get display bounds: {SDL_GetError()}");
        }

        // Get the window size
        int windowWidth, windowHeight;
        SDL_GetWindowSize(SDLWindowHandle, &windowWidth, &windowHeight);

        // Calculate the centered position
        int centeredX = displayBounds.x + (displayBounds.w - windowWidth) / 2;
        int centeredY = displayBounds.y + (displayBounds.h - windowHeight) / 2;

        // Set the window position
        SDL_SetWindowPosition(SDLWindowHandle, centeredX, centeredY);
    }

    #region Private methods

    private static void createWindowShadowForBorderless(SDL_Window* window)
    {
        // Creates a shadow frame outside for borderless windows, might be useful?
        fixed (byte* ptr = SDL_PROP_WINDOW_WIN32_HWND_POINTER)
        {
            var hwnd = SDL_GetPointerProperty(SDL_GetWindowProperties(window), ptr, 0);

            var shadow = new MARGINS()
            {
                cxLeftWidth = 1,
                cxRightWidth = 1,
                cyTopHeight = 1,
                cyBottomHeight = 1
            };
            DwmExtendFrameIntoClientArea(hwnd, ref shadow);
        }
    }

    #endregion
}