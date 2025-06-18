using Pellychan.GUI.Platform.SDL3;
using Pellychan.GUI.Widgets;
using SDL;
using SkiaSharp;
using System.Runtime.InteropServices;
using static SDL.SDL3;

namespace Pellychan.GUI.Platform.Skia;

internal unsafe class SkiaWindow : SDL3Window
{
    public Widget ParentWidget { get; private set; } 

    public SDL_Renderer* SDLRenderer;
    public SDL_Texture* SDLTexture;

    public SKImageInfo ImageInfo { get; private set; }

    public bool ShouldClose { get; private set; }

    private IntPtr m_pixels;
    private int m_pitch;

    public IntPtr Pixels => m_pixels;
    public int Pitch => m_pitch;

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

    public SkiaWindow(Widget parent, string title) : base()
    {
        Create();
        Center();

        this.ExitRequested += delegate ()
        {
            ShouldClose = true;
        };

        ParentWidget = parent;

        Title = title;

        SDLRenderer = SDL_CreateRenderer(SDLWindowHandle, (byte*)null);
    }

    public void CreateFrameBuffer(int w, int h)
    {
        ImageInfo = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());

        if (SDLTexture != null)
        {
            SDL_DestroyTexture(SDLTexture);
        }

        // Create SDL texture as the drawing target
        SDLTexture = SDL_CreateTexture(SDLRenderer,
            SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888,
            SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            w, h);
    }

    public new void Dispose()
    {
        // SkiaSurface?.Dispose();

        SDL_DestroyTexture(SDLTexture);
        SDL_DestroyRenderer(SDLRenderer);
        
        base.Dispose();
    }

    public void Lock()
    {
        fixed (nint* pixelsPtr = &m_pixels)
        fixed (int* pitchPtr = &m_pitch)
        {
            SDL_LockTexture(SDLTexture, null, pixelsPtr, pitchPtr);
        }
    }

    public void Unlock()
    {
        SDL_UnlockTexture(SDLTexture);
    }

    public void Present()
    {
        SDL_RenderClear(SDLRenderer);
        SDL_RenderTexture(SDLRenderer, SDLTexture, null, null);

        SDL_RenderPresent(SDLRenderer);

        SDL_SetRenderDrawColor(popupRenderer, 255, 0, 0, 255);
        SDL_RenderPresent(popupRenderer);
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