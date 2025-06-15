using Pellychan.GUI.Platform.Skia;
using Pellychan.GUI.Styles;
using Pellychan.GUI.Styles.Phantom;
using Pellychan.GUI.Widgets;
using Pellychan.Resources;
using SDL2;
using SkiaSharp;

namespace Pellychan.GUI;

internal static class WindowRegistry
{
    private static readonly Dictionary<uint, SkiaWindow> Windows = [];

    public static void Register(SkiaWindow window)
    {
        Windows[window.WindowID] = window;
    }

    public static SkiaWindow? Get(uint id) =>
        Windows.GetValueOrDefault(id);
}

public class Application : IDisposable
{
    internal static Application? Instance { get; private set; }
    internal readonly List<Widget> TopLevelWidgets = [];

    private readonly SKFont m_defaultFont;
    private readonly Style m_defaultStyle;
    private ColorPalette m_palette;
    
    public static SKFont DefaultFont => Instance!.m_defaultFont;
    public static Style DefaultStyle => Instance!.m_defaultStyle;

    /// <summary>
    /// 
    /// </summary>
    public static ColorPalette Palette => Instance!.m_palette;

    public Application()
    {
        if (Instance != null)
        {
            throw new InvalidOperationException("Application already exists.");
        }
        Instance = this;
        
        SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

        using var fontStream = PellychanResources.ResourceAssembly.GetManifestResourceStream("Pellychan.Resources.Fonts.lucidagrande.ttf");
        // using var typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        using var typeface = SKTypeface.FromStream(fontStream);

        const int dpi = 96;
        const float pixelsPerPoint = dpi / 72.0f;
        const float skiaFontSize = 9 * pixelsPerPoint;

        m_defaultFont = new SKFont
        {
            Edging = SKFontEdging.SubpixelAntialias,
            Hinting = SKFontHinting.Slight,
            Subpixel = true,
            Typeface = typeface,
            Size = skiaFontSize
        };
        m_palette = ColorPalette.Default;

        // Load default style last
        m_defaultStyle = new PhantomStyle();
    }

    public void Run()
    {
        while (TopLevelWidgets.Count > 0)
        {
            pumpEvents();

            foreach (var w in TopLevelWidgets.ToArray())
            {
                if (w.ShouldClose())
                {
                    w.Dispose();
                    TopLevelWidgets.Remove(w);
                }
                else
                {
                    w.RenderTopLevel();
                    // Glfw.WaitEvents(); // <- This tells Glfw to wait until the user does something to actually update
                }
            }

            // SDL.SDL_Delay(16); // ~60fps
        }
    }

    public void Dispose()
    {
        foreach (var w in TopLevelWidgets)
        {
            w.Dispose();
        }
        
        MouseCursor.Cleanup();

        SDL.SDL_Quit();
        GC.SuppressFinalize(this);
    }

    private static void pumpEvents()
    {
        while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
        {
            uint id = e.type switch
            {
                SDL.SDL_EventType.SDL_MOUSEMOTION => e.motion.windowID,
                SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN or SDL.SDL_EventType.SDL_MOUSEBUTTONUP => e.button.windowID,
                SDL.SDL_EventType.SDL_WINDOWEVENT => e.window.windowID,
                _ => 0
            };

            if (WindowRegistry.Get(id) is { } win)
            {
                win.HandleEvent(e);
            }
        }
    }
}