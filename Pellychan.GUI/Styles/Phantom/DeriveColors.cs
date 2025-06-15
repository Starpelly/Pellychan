using SkiaSharp;

namespace Pellychan.GUI.Styles.Phantom;

public static class DeriveColors
{
    public static double Saturate(double x)
    {
        return x switch
        {
            < 0.0 => 0.0,
            > 1.0 => 1.0,
            _ => x
        };
    }

    public static SKColor AdjustLightness(SKColor color, double ld)
    {
        var hsl = SkiaSharpHelpers.ToHsluv(color);
        const double gamma = 3.0;
        hsl.l = Math.Pow(Saturate(Math.Pow(hsl.l, 1.0 / gamma) + ld * 0.8), gamma);
        return SkiaSharpHelpers.FromHsluv(hsl);
    }

    public static SKColor ButtonColor(ColorPalette pal)
    {
        // This is a hack apparently?
        if (pal.Get(ColorGroup.Active, ColorRole.Button) == pal.Get(ColorGroup.Active, ColorRole.Window))
            return AdjustLightness(pal.Get(ColorGroup.Active, ColorRole.Button), 0.01);
        return pal.Get(ColorGroup.Active, ColorRole.Button);
    }

    public static SKColor HighlightedOutlineOf(ColorPalette pal)
    {
        return AdjustLightness(pal.Get(ColorGroup.Active, ColorRole.Highlight), -0.05);
    }
    public static SKColor DividerColor(SKColor underlying)
    {
        return AdjustLightness(underlying, -0.05);
    }
    public static SKColor OutlineOf(ColorPalette pal)
    {
        return AdjustLightness(pal.Get(ColorGroup.Active, ColorRole.Window), -0.1);
    }

    public static SKColor GutterColorOf(ColorPalette pal)
    {
        return AdjustLightness(pal.Get(ColorGroup.Active, ColorRole.Window), -0.03);
    }

    public static SKColor LightShadeOf(SKColor underlying)
    {
        return AdjustLightness(underlying, 0.07);
    }

    public static SKColor DarkShadeOf(SKColor underlying)
    {
        return AdjustLightness(underlying, -0.07);
    }

    public static SKColor OverhangShadowOf(SKColor underlying)
    {
        return AdjustLightness(underlying, -0.05);
    }

    public static SKColor SliderGutterShadowOf(SKColor underlying)
    {
        return AdjustLightness(underlying, -0.01);
    }

    public static SKColor SpecularOf(SKColor underlying)
    {
        return AdjustLightness(underlying, 0.03);
    }

    public static SKColor PressedOf(SKColor color)
    {
        return AdjustLightness(color, -0.02);
    }

    public static SKColor ProgressBarOutlineColorOf(ColorPalette pal)
    {
        // Pretty wasteful
        var hsl0 = SkiaSharpHelpers.ToHsluv(pal.Get(ColorRole.Window));
        var hsl1 = SkiaSharpHelpers.ToHsluv(pal.Get(ColorRole.Highlight));
        hsl1.l = Saturate(Math.Min(hsl0.l - 0.1, hsl1.l - 0.2));
        return SkiaSharpHelpers.FromHsluv(hsl1);
    }
}