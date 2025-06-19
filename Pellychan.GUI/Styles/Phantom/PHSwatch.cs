using SkiaSharp;
using System.Runtime.CompilerServices;

namespace Pellychan.GUI.Styles.Phantom;

using Dc = DeriveColors;

public struct PHSwatch
{
    public readonly SKPaint[] Paints = new SKPaint[(int)SwatchColor.Num];
    public readonly SKColor[] ScrollbarShadowColors = new SKColor[PhantomStyle.Num_ShadowSteps];

    private class SwatchColorMap
    {
        private readonly SKColor[] m_colors = new SKColor[(int)SwatchColor.Num];

        public SKColor this[int color] => m_colors[color];

        public SKColor this[SwatchColor color]
        {
            get => m_colors[(int)color];
            set => m_colors[(int)color] = value;
        }
    }

    public PHSwatch()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly SKColor GetColor(SwatchColor color) => Paints[(int)color].Color;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly SKPaint GetPaint(SwatchColor color) => Paints[(int)color];

    public void LoadFromPalette(ColorPalette palette)
    {
        const bool isEnabled = true;

        var colors = new SwatchColorMap();

        SKColor getPal(ColorRole role) => palette.Get(ColorGroup.Active, role);

        colors[SwatchColor.None] = SKColors.Transparent;

        colors[SwatchColor.Window] = getPal(ColorRole.Window);
        colors[SwatchColor.Button] = getPal(ColorRole.Button);

        if (colors[SwatchColor.Button] == colors[SwatchColor.Window])
            colors[SwatchColor.Button] = Dc.AdjustLightness(colors[SwatchColor.Button], 0.01);

        colors[SwatchColor.Base] = getPal(ColorRole.Base);
        colors[SwatchColor.Text] = getPal(ColorRole.Text);
        colors[SwatchColor.WindowText] = getPal(ColorRole.WindowText);
        colors[SwatchColor.Highlight] = getPal(ColorRole.Highlight);
        colors[SwatchColor.HighlightedText] = getPal(ColorRole.HighlightedText);

        colors[SwatchColor.ScrollbarGutter] = Dc.GutterColorOf(palette);

        colors[SwatchColor.Window_Outline] = Dc.AdjustLightness(colors[SwatchColor.Window], isEnabled ? -0.1 : -0.07);
        colors[SwatchColor.Window_Specular] = isEnabled ? Dc.SpecularOf(colors[SwatchColor.Window]) : colors[SwatchColor.Window];
        colors[SwatchColor.Window_Divider] = Dc.DividerColor(colors[SwatchColor.Window]);
        colors[SwatchColor.Window_Lighter] = Dc.LightShadeOf(colors[SwatchColor.Window]);
        colors[SwatchColor.Window_Darker] = Dc.DarkShadeOf(colors[SwatchColor.Window]);

        colors[SwatchColor.Button_Specular] = isEnabled ? Dc.SpecularOf(colors[SwatchColor.Button]) : colors[SwatchColor.Button];
        colors[SwatchColor.Button_Pressed] = Dc.PressedOf(colors[SwatchColor.Button]);
        colors[SwatchColor.Button_Pressed_Specular] =
            isEnabled ? Dc.SpecularOf(colors[SwatchColor.Button_Pressed])
                      : colors[SwatchColor.Button_Pressed];

        colors[SwatchColor.Base_Shadow] = Dc.OverhangShadowOf(colors[SwatchColor.Base]);
        colors[SwatchColor.Base_Divider] = Dc.DividerColor(colors[SwatchColor.Base]);

        colors[SwatchColor.WindowText_Disabled] = palette.Get(ColorGroup.Disabled, ColorRole.WindowText);
        colors[SwatchColor.Highlight_Outline] = Dc.AdjustLightness(colors[SwatchColor.Highlight], -0.05);
        colors[SwatchColor.Highlight_Specular] =
            isEnabled ? Dc.SpecularOf(colors[SwatchColor.Highlight]) : colors[SwatchColor.Highlight];

        colors[SwatchColor.ProgressBar_Outline] = Dc.ProgressBarOutlineColorOf(palette);

        // Qt has something called "current" groups but they're not actual groups???
        colors[SwatchColor.Indicator_Current] = Dc.IndicatorColorOf(palette, ColorGroup.Active);
        colors[SwatchColor.Indicator_Disabled] = Dc.IndicatorColorOf(palette, ColorGroup.Disabled);

        colors[SwatchColor.ScrollbarGutter_Disabled] = colors[SwatchColor.Window];

        Paints[(int)SwatchColor.None] = new();

        for (var i = (int)SwatchColor.None + 1; i < (int)SwatchColor.Num; ++i)
        {
            Paints[i] = new()
            {
                Color = colors[i]
            };
        }

        var gutterGrad = new Grad(Dc.SliderGutterShadowOf(colors[SwatchColor.ScrollbarGutter]),
                colors[SwatchColor.ScrollbarGutter]);
        for (int i = 0; i < PhantomStyle.Num_ShadowSteps; ++i)
        {
            ScrollbarShadowColors[i] =
                gutterGrad.Sample((double)i / (double)PhantomStyle.Num_ShadowSteps);
        }
    }
}
