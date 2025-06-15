using System.Runtime.CompilerServices;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace Pellychan.GUI.Styles.Phantom;

using Dc = DeriveColors;

public struct PHSwatch
{
    public SKPaint[] Paints = new SKPaint[(int)SwatchColor.Num];

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

        Paints[(int)SwatchColor.None] = new();

        for (var i = (int)SwatchColor.None + 1; i < (int)SwatchColor.Num; ++i)
        {
            Paints[i] = new();
            Paints[i].Color = colors[i];
        }
    }
}

public class PhantomStyle : Style
{
    private PHSwatch m_swatch = new();
    
    #region Adjustments

    private const float PushButton_Rounding = 2.0f;

    #endregion

    public PhantomStyle()
    {
        m_swatch.LoadFromPalette(Application.Palette);
    }

    public override void DrawPushButton(SKCanvas canvas, PushButton button, StyleOptionButton option)
    {
        using var paint = new SKPaint();

        var isDefault = false;
        var isOn = option.State.HasFlag(StateFlag.On);
        var isDown = option.State.HasFlag(StateFlag.Sunken);
        var hasFocus = option.State.HasFlag(StateFlag.HasFocus);

        var outline = SwatchColor.Window_Outline;
        var fill = SwatchColor.Button;
        var specular = SwatchColor.Button_Specular;

        // Paint background
        {
            if (isDown)
            {
                fill = SwatchColor.Button_Pressed;
                specular = SwatchColor.Button_Pressed_Specular;
            }
            else if (isOn)
            {
                fill = SwatchColor.ScrollbarGutter;
                specular = SwatchColor.Button_Pressed_Specular;
            }
            if (hasFocus || isDefault)
            {
                outline = SwatchColor.Highlight_Outline;
            }

            paint.IsAntialias = true;

            // Fill
            paint.Color = m_swatch.GetColor(fill);
            canvas.DrawRoundRect(new SKRect(0, 0, button.Width, button.Height), new SKSize(PushButton_Rounding + 1, PushButton_Rounding + 1), paint);

            // Stroke
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.0f;
            paint.Color = m_swatch.GetColor(outline);

            var inset = paint.StrokeWidth / PushButton_Rounding;
            canvas.DrawRoundRect(new SKRect(inset, inset, button.Width - inset, button.Height - inset), new SKSize(PushButton_Rounding, PushButton_Rounding), paint);

            // Specular
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.0f;
            paint.Color = m_swatch.GetColor(specular);

            inset += 1;
            canvas.DrawRoundRect(new SKRect(inset, inset, button.Width - inset, button.Height - inset), new SKSize(PushButton_Rounding, PushButton_Rounding), paint);
        }

        // Paint label
        {
            paint.Reset();
            paint.Color = m_swatch.GetColor(SwatchColor.Text);

            var labelX = button.Width / 2 - Application.DefaultFont.MeasureText(option.Text) / 2;
            var labelY = Application.DefaultFont.Size + PushButton.TextPaddingH / 2;

            if (isDown)
            {
                labelY += 1;
            }

            canvas.DrawText(option.Text, new SKPoint(labelX, labelY), Application.DefaultFont, paint);
        }
    }
}