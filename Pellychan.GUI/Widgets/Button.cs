using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class Button : Widget, IPaintHandler, IMouseEnterHandler, IMouseLeaveHandler
{
    private const int TextPadding = 16;
    private const float ButtonRounding = 4.0f;

    private string m_text = string.Empty;
    public string Text
    {
        get
        {
            return m_text;
        }
        set
        {
            m_text = value;
        }
    }

    private bool m_hovering = false;

    public Button(string text)
    {
        m_text = text;
        Width = (int)Application.DefaultFont.MeasureText(text) + TextPadding;
        Height = (int)Application.DefaultFont.Size + 2 + TextPadding;
    }

    public void OnMouseEnter()
    {
        m_hovering = true;
        MouseCursor.Set(MouseCursor.CursorType.Hand);
    }

    public void OnMouseLeave()
    {
        m_hovering = false;
        MouseCursor.Set(MouseCursor.CursorType.Arrow);
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();

        // Paint background
        {
            paint.Color = EffectivePalette.Get(ColorGroup.Active, ColorRole.Button);
            paint.IsAntialias = true;

            // Fill
            canvas.DrawRoundRect(new SKRect(0, 0, Width, Height), new SKSize(ButtonRounding, ButtonRounding), paint);

            // Stroke
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.0f;
            paint.Color = EffectivePalette.Get(ColorGroup.Active, ColorRole.Highlight);

            float inset = paint.StrokeWidth / 2.0f;
            canvas.DrawRoundRect(new SKRect(inset, inset, Width - inset, Height - inset), new SKSize(ButtonRounding, ButtonRounding), paint);
        }

        // Paint label
        {
            paint.Reset();
            paint.Color = EffectivePalette.Get(ColorGroup.Active, ColorRole.ButtonText);
            canvas.DrawText(m_text, new SKPoint(TextPadding / 2, Application.DefaultFont.Size + (TextPadding / 2)), Application.DefaultFont, paint);
        }
    }
}