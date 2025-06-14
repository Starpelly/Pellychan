using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class Button : Widget, IPaintHandler, IMouseEnterHandler, IMouseLeaveHandler, IMouseDownHandler, IMouseUpHandler
{
    private const int TextPadding = 16;
    private const float ButtonRounding = 2.0f;

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
    private bool m_pressed = false;

    public Button(string text, Widget? parent = null) : base(parent)
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

            if (m_pressed)
            {
                paint.Color = new SKColor(69, 70, 75);
            }

            paint.IsAntialias = true;

            // Fill
            canvas.DrawRoundRect(new SKRect(0, 0, Width, Height), new SKSize(ButtonRounding + 1, ButtonRounding + 1), paint);

            // Stroke
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.0f;
            paint.Color = new SKColor(42, 42, 45);

            float inset = paint.StrokeWidth / ButtonRounding;
            canvas.DrawRoundRect(new SKRect(inset, inset, Width - inset, Height - inset), new SKSize(ButtonRounding, ButtonRounding), paint);

            // Stroke
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.0f;
            paint.Color = new SKColor(82, 83, 89);

            inset += 1;
            canvas.DrawRoundRect(new SKRect(inset, inset, Width - inset, Height - inset), new SKSize(ButtonRounding, ButtonRounding), paint);
        }

        // Paint label
        {
            paint.Reset();
            paint.Color = EffectivePalette.Get(ColorGroup.Active, ColorRole.ButtonText);

            var labelX = (Width / 2) - (Application.DefaultFont.MeasureText(m_text) / 2);
            var labelY = Application.DefaultFont.Size + (TextPadding / 2);

            if (m_pressed)
            {
                labelY += 1;
            }

            canvas.DrawText(m_text, new SKPoint(labelX, labelY), Application.DefaultFont, paint);
        }
    }

    public void OnMouseDown(int x, int y)
    {
        m_pressed = true;
        Invalidate();
    }

    public void OnMouseUp(int x, int y)
    {
        m_pressed = false;
        Invalidate();
    }
}