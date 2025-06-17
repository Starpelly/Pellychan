using Pellychan.GUI.Platform.Skia;
using Pellychan.GUI.Styles;
using Pellychan.GUI.Styles.Phantom;
using Pellychan.GUI.Widgets;
using Pellychan.Resources;
using SDL;
using SkiaSharp;

namespace Pellychan.GUI;

internal static class WindowRegistry
{
    private static readonly Dictionary<SDL.SDL_WindowID, SkiaWindow> Windows = [];

    public static void Register(SkiaWindow window)
    {
        Windows[window.WindowID] = window;
    }

    public static SkiaWindow? Get(SDL.SDL_WindowID id) =>
        Windows.GetValueOrDefault(id);
}

public class Application : IDisposable
{
    internal static Application? Instance { get; private set; }
    
    internal readonly List<Widget> TopLevelWidgets = [];

    internal ToolTip ToolTip;
    
    private readonly SKFont m_defaultFont;
    private readonly Style m_defaultStyle;
    private ColorPalette m_palette;
    
    public static SKFont DefaultFont => Instance!.m_defaultFont;
    public static Style DefaultStyle => Instance!.m_defaultStyle;

    internal static bool DebugDrawing = false;

    /// <summary>
    /// 
    /// </summary>
    public static ColorPalette Palette => Instance!.m_palette;

    internal static class LayoutQueue
    {
        private static readonly HashSet<Widget> s_dirtyWidgets = [];
        public static bool IsFlusing { get; private set; } = false;

        public static void Enqueue(Widget widget)
        {
            // if (IsFlusing)
            //    return;

            if (widget.Layout == null)
                return;

            Console.WriteLine($"Enqued: {widget.GetType().Name}");

            s_dirtyWidgets.Add(widget);
        }

        public static void Flush()
        {
            IsFlusing = true;
            while (s_dirtyWidgets.Count > 0)
            {
                Console.WriteLine("==================Layout Flush Start==================");

                foreach (var widget in s_dirtyWidgets.ToList())
                {
                    widget.PerformLayoutUpdate();
                    s_dirtyWidgets.Remove(widget);
                }

                Console.WriteLine("===================Layout Flush End===================");
            }
            s_dirtyWidgets.Clear();
            IsFlusing = false;
        }
    }

    public Application()
    {
        if (Instance != null)
        {
            throw new InvalidOperationException("Application already exists.");
        }
        Instance = this;

        SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO);

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

            // Flush any pending layout requests
            LayoutQueue.Flush();

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
                }
            }

            // Glfw.WaitEvents(); // <- This tells Glfw to wait until the user does something to actually update
            // SDL3.SDL_Delay(16); // ~60fps
        }
        
        // ToolTip.Show();
    }

    public void Dispose()
    {
        foreach (var w in TopLevelWidgets)
        {
            w.Dispose();
        }
        
        MouseCursor.Cleanup();

        SDL3.SDL_Quit();
        GC.SuppressFinalize(this);
    }

    private static void pumpEvents()
    {
        unsafe
        {
            SDL_Event e;
            while (SDL3.SDL_PollEvent(&e) != false)
            {
                /*

                */

                if (e.Type == SDL.SDL_EventType.SDL_EVENT_KEY_DOWN)
                {
                    if (e.key.scancode == SDL.SDL_Scancode.SDL_SCANCODE_F2)
                    {
                        Application.DebugDrawing = !Application.DebugDrawing;
                    }
                }

                SDL_WindowID id = e.Type switch
                {
                    SDL_EventType.SDL_EVENT_MOUSE_MOTION => e.motion.windowID,
                    SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN or SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP => e.button.windowID,


                    SDL_EventType.SDL_EVENT_WINDOW_RESIZED => e.window.windowID,
                    SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED => e.window.windowID,
                    _ => 0
                };

                if (WindowRegistry.Get(id) is { } win)
                {
                    win.HandleEvent(e);
                }
            }
        }
    }
}