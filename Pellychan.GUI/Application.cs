using Pellychan.GUI.Platform.Skia;
using Pellychan.GUI.Widgets;
using Pellychan.Resources;
using SDL2;
using SkiaSharp;

namespace Pellychan.GUI;

internal static class WindowRegistry
{
    private static readonly Dictionary<uint, SkiaWindow> s_windows = [];

    public static void Register(SkiaWindow window)
    {
        s_windows[window.WindowID] = window;
    }

    public static SkiaWindow? Get(uint id) =>
        s_windows.TryGetValue(id, out var win) ? win : null;
}

public class Application : IDisposable
{
    public static SKFont DefaultFont { get; set; }

    internal static Application? Instance { get; private set; } = null;
    
    internal readonly List<Widget> TopLevelWidgets = [];

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
        
        DefaultFont = new SKFont
        {
            Edging = SKFontEdging.SubpixelAntialias,
            Hinting = SKFontHinting.Full,
            Subpixel = true,
            Typeface = typeface,
            Size = 13
        };
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

    private void pumpEvents()
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