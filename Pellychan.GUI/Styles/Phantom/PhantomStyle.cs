using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace Pellychan.GUI.Styles.Phantom;

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