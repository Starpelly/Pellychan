﻿using Pellychan.GUI.Framework.Platform;
using Pellychan.GUI.Framework.Platform.SDL3;
using Pellychan.GUI.Framework.Platform.Skia;
using Pellychan.GUI.Styles;
using Pellychan.GUI.Styles.Phantom;
using Pellychan.GUI.Widgets;
using SDL;
using SixLabors.ImageSharp.Formats.Png;
using SkiaSharp;

namespace Pellychan.GUI;

internal static class WindowRegistry
{
    internal struct WidgetWindow
    {
        public SkiaWindow SkiaWindow;
        public Widget Widget;

        public WidgetWindow(SkiaWindow skiaWindow, Widget widget)
        {
            this.SkiaWindow = skiaWindow;
            this.Widget = widget;
        }
    }

    internal static readonly Dictionary<SDL.SDL_WindowID, WidgetWindow> WidgetWindows = [];
    // internal static readonly Dictionary<SDL.SDL_WindowID, IWindow> Windows = [];

    public static void Register(WidgetWindow window)
    {
        WidgetWindows[window.SkiaWindow.SDLWindowID] = window;
        // Windows[window.SkiaWindow.SDLWindowID] = window.SkiaWindow.Window;
    }

    public static void Remove(WidgetWindow window)
    {
        WidgetWindows.Remove(window.SkiaWindow.SDLWindowID);
        // Windows.Remove(window.SkiaWindow.SDLWindowID);
    }

    public static WidgetWindow? Get(SDL.SDL_WindowID id) =>
        WidgetWindows.GetValueOrDefault(id);
}

public class Application : IDisposable
{
    internal static Application? Instance { get; private set; }
    
    internal readonly List<Widget> TopLevelWidgets = [];

    private readonly SKFont m_defaultFont;
    private readonly SKFont m_defaultFontBold;
    private readonly SKFont m_fontIcon;
    private readonly Style m_defaultStyle;
    private ColorPalette m_palette;
    
    public static SKFont DefaultFont => Instance!.m_defaultFont;
    public static SKFont DefaultFontBold => Instance!.m_defaultFontBold;
    public static Style DefaultStyle => Instance!.m_defaultStyle;

    /// <summary>
    /// !!!TEMP!!!
    /// </summary>
    public static SKFont FontIcon => Instance!.m_fontIcon;

    internal static bool DebugDrawing = false;
    internal const bool SupportPaintCaching = false;

    public static bool HeadlessMode { get; set; } = false;
    public static bool HardwareAccel { get; private set; } = true;

    private readonly Clipboard s_clipboard;
    public static Clipboard Clipboard => Instance!.s_clipboard;

    public enum RendererType
    {
        SDL,
        OpenGL
    }

    #region Timing

    internal class DeltaTime
    {
        public UInt64 Now;
        public UInt64 Last;
        public double DT;
    }

    internal static readonly DeltaTime DeltaTiming = new();
    internal static uint CurrentFrame { get; private set; } = 0;

    #endregion

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

        SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO);

        if (HardwareAccel)
        {
            SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_MAJOR_VERSION, 3);
            SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_MAJOR_VERSION, 2);
            SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL_GLProfile.SDL_GL_CONTEXT_PROFILE_CORE);
            SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_ALPHA_SIZE, 8);
        }

        int version = SDL3.SDL_GetVersion();
        Console.WriteLine($@"SDL3 Initialized
                          SDL3 Version: {SDL3.SDL_VERSIONNUM_MAJOR(version)}.{SDL3.SDL_VERSIONNUM_MINOR(version)}.{SDL3.SDL_VERSIONNUM_MICRO(version)}
                          SDL3 Revision: {SDL3.SDL_GetRevision()}
                          SDL3 Video driver: {SDL3.SDL_GetCurrentVideoDriver()}");

        s_clipboard = new SDL3Clipboard(PngFormat.Instance);

        const int dpi = 96;
        const float pixelsPerPoint = dpi / 72.0f;
        const float skiaFontSize = 9 * pixelsPerPoint;

        static SKFont createUIFont(SKFontStyleWeight weight)
        {
            // I kinda like Century Gothic
            using var typeface = SKTypeface.FromFamilyName("Segoe UI", weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

            return new SKFont
            {
                Edging = SKFontEdging.SubpixelAntialias,
                Hinting = SKFontHinting.Slight,
                Subpixel = true,
                Typeface = typeface,
                Size = skiaFontSize
            };
        }

        m_defaultFont = createUIFont(SKFontStyleWeight.Normal);
        m_defaultFontBold = createUIFont(SKFontStyleWeight.Bold);

        using var iconsStream = typeof(Application).Assembly.GetManifestResourceStream("Pellychan.GUI.Resources.MaterialIconsRound-Regular.otf");
        using var iconsTypeface = SKTypeface.FromStream(iconsStream);

        const float iconFontSize = 12 * pixelsPerPoint;

        m_fontIcon = new SKFont
        {
            Edging = SKFontEdging.SubpixelAntialias,
            Hinting = SKFontHinting.Slight,
            Subpixel = true,
            Typeface = iconsTypeface,
            Size = iconFontSize
        };

        m_palette = getDefaultColorPalette();


        // Load default style last
        m_defaultStyle = new PhantomStyle();
    }

    public void Run()
    {
        while (TopLevelWidgets.Count > 0)
        {
            /*
            // @INVESTIGATE
            // For now, we just copy the windows list. Maybe this isn't smart and we should
            // dispatch it until the next frame to actually create the window?
            foreach (var window in WindowRegistry.WidgetWindows.ToList())
            {
                window.Value.SkiaWindow.PollEvents();
            }
            */

            SkiaWindow.PollEvents();

            RunMainLoop();

            foreach (var win in WindowRegistry.WidgetWindows)
            {
                if (win.Value.SkiaWindow.ShouldClose)
                {
                    win.Value.Widget.Dispose();
                    TopLevelWidgets.Remove(win.Value.Widget);
                }
            }

            // Glfw.WaitEvents(); // <- This tells Glfw to wait until the user does something to actually update
            // SDL3.SDL_Delay(16); // ~60fps
        }
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

    #region Internal methods

    internal void RunMainLoop()
    {
        // Flush any pending layout requests
        LayoutQueue.Flush();

        foreach (var win in WindowRegistry.WidgetWindows)
        {
            win.Value.SkiaWindow.RunCommands();
        }

        UpdateDeltaTime();
        RenderAllWindows();

        CurrentFrame++;
    }

    internal void UpdateDeltaTime()
    {
        DeltaTiming.Last = DeltaTiming.Now;
        DeltaTiming.Now = SDL3.SDL_GetPerformanceCounter();

        DeltaTiming.DT = (double)(DeltaTiming.Now - DeltaTiming.Last) / SDL3.SDL_GetPerformanceFrequency();
    }

    internal void RenderAllWindows()
    {
        foreach (var win in WindowRegistry.WidgetWindows)
        {
            var w = win.Value.Widget;
            w.UpdateTopLevel(DeltaTiming.DT);
            w.RenderTopLevel(Application.DebugDrawing);
        }
    }

    #endregion

    #region Private methods

    private struct ThemeColors
    {
        public SKColor window;
        public SKColor text;
        public SKColor disabledText;
        public SKColor brightText;
        public SKColor highlight;
        public SKColor highlightedText;
        public SKColor @base;
        public SKColor alternateBase;
        public SKColor shadow;
        public SKColor button;
        public SKColor disabledButton;
        public SKColor unreadBadge;
        public SKColor unreadBadgeText;
        public SKColor icon;
        public SKColor disabledIcon;
        public SKColor chatTimestampText;

        public readonly ColorPalette ToPalette()
        {
            var pal = new ColorPalette();

            pal.Set(ColorRole.Window, this.window);
            pal.Set(ColorRole.WindowText, this.text);
            pal.Set(ColorRole.Text, this.text);
            pal.Set(ColorRole.ButtonText, this.text);
            // if (this.brightText.isValid())
            pal.Set(ColorRole.BrightText, this.brightText);
            pal.Set(ColorGroup.Disabled, ColorRole.WindowText, this.disabledText);
            pal.Set(ColorGroup.Disabled, ColorRole.Text, this.disabledText);
            pal.Set(ColorGroup.Disabled, ColorRole.ButtonText, this.disabledText);
            pal.Set(ColorRole.Base, this.@base);
            pal.Set(ColorRole.AlternateBase, this.alternateBase);
            // if (this.shadow.isValid())
            pal.Set(ColorRole.Shadow, this.shadow);
            pal.Set(ColorRole.Button, this.button);
            pal.Set(ColorRole.Highlight, this.highlight);
            pal.Set(ColorRole.HighlightedText, this.highlightedText);
            // if (this.disabledButton.isValid())
            pal.Set(ColorGroup.Disabled, ColorRole.Button, this.disabledButton);
            // Used as the shadow text color on disabled menu items
            pal.Set(ColorGroup.Disabled, ColorRole.Light, SKColors.Transparent);

            return pal;
        }
    };

    private ColorPalette getDefaultColorPalette()
    {
        ThemeColors c = new();
        setCarbon(ref c);
        // setPolar(ref c);
        // setStealth(ref c);
        // setSakura(ref c);

        return c.ToPalette();
    }

    private void setCarbon(ref ThemeColors c)
    {
        var window = new SKColor(60, 61, 64);
        var button = new SKColor(74, 75, 80);
        var @base = new SKColor(46, 47, 49);
        var alternateBase = new SKColor(41, 41, 43);
        var text = new SKColor(208, 209, 212);
        var highlight = new SKColor(191, 199, 213);
        var highlightedText = new SKColor(45, 44, 39);
        var disabledText = new SKColor(164, 166, 168);

        disabledText = disabledText.Darker(120); // old
        c.window = window;
        c.text = text;
        c.disabledText = disabledText;
        c.@base = @base;
        c.alternateBase = alternateBase;
        c.shadow = @base;
        c.button = button;
        c.disabledButton = button.Darker(107);
        c.brightText = SKColors.White;
        c.highlight = highlight;
        c.highlightedText = highlightedText;
        c.icon = text;
        c.disabledIcon = c.disabledText;
        c.unreadBadge = c.text;
        c.unreadBadgeText = c.highlightedText;
        c.chatTimestampText = c.@base.Lighter(160);
    }

    private void setPolar(ref ThemeColors c)
    {
        var snow = new SKColor(251, 252, 254);
        var callout = new SKColor(90, 97, 111);
        var bright = new SKColor(237, 236, 241);
        var lessBright = new SKColor(234, 234, 238);
        var dimmer = new SKColor(221, 221, 226);
        var text = new SKColor(18, 18, 24);
        var disabledText = new SKColor(140, 140, 145);
        c.window = bright;
        c.highlight = callout;
        c.highlightedText = new SKColor(255, 255, 255);
        c.@base = snow;
        c.alternateBase = lessBright;
        c.button = bright;
        c.text = text;
        c.disabledText = disabledText;
        c.icon = new SKColor(105, 107, 113);
        c.disabledIcon = c.disabledText.Lighter(125);
        c.unreadBadge = c.highlight;
        c.unreadBadgeText = c.highlightedText;
        c.chatTimestampText = c.@base.Darker(130);
    }

    private void setStealth(ref ThemeColors c)
    {
        var window = new SKColor(30, 31, 32);
        var button = new SKColor(41, 42, 44);
        var @base = new SKColor(23, 24, 25);
        var alternateBase = new SKColor(19, 19, 22);
        var text = new SKColor(212, 209, 208);
        var highlight = new SKColor(211, 210, 208);
        var highlightedText = new SKColor(45, 44, 39);
        var disabledText = new SKColor(164, 166, 168);
        c.window = window;
        c.text = text;
        c.disabledText = disabledText.Darker(150);
        c.@base = @base;
        c.alternateBase = alternateBase;
        c.shadow = @base;
        c.button = button;
        c.disabledButton = button.Darker(107);
        c.brightText = SKColors.White;
        c.highlight = highlight;
        c.highlightedText = highlightedText;
        c.icon = text;
        c.disabledIcon = c.disabledText;
        c.unreadBadge = c.text;
        c.unreadBadgeText = c.highlightedText;
        c.chatTimestampText = c.@base.Lighter(160);
    }

    private void setSakura(ref ThemeColors c)
    {
        var callout = new SKColor(156, 112, 160);
        var bright = new SKColor(252, 234, 243);
        var lessBright = new SKColor(242, 234, 237);
        var dimmer = new SKColor(255, 219, 250);
        var text = new SKColor(24, 18, 18);
        var disabledText = new SKColor(145, 140, 140);
        c.window = bright;
        c.highlight = callout;
        c.highlightedText = new SKColor(255, 255, 255);
        c.@base = new SKColor(255, 247, 252);
        c.alternateBase = lessBright;
        c.button = dimmer;
        c.text = text;
        c.disabledText = disabledText;
        c.icon = new SKColor(120, 100, 112);
        c.disabledIcon = c.disabledText.Lighter(125);
        c.unreadBadge = c.highlight;
        c.unreadBadgeText = c.highlightedText;
        c.chatTimestampText = c.@base.Darker(130);
    }

    #endregion
}