using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class Button : Widget, IPaintHandler, IMouseEnterHandler, IMouseLeaveHandler, IMouseDownHandler, IMouseUpHandler, IMouseClickHandler
{
    private const int TextPaddingW = 16;
    private const int TextPaddingH = 16;
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
            updateSize();
        }
    }

    private bool m_hovering = false;
    private bool m_pressed = false;

    public Action? OnClicked;
    public Action? OnPressed;
    public Action? OnReleased;

    public Button(string text, Widget? parent = null) : base(parent)
    {
        Text = text;
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();

        var held = m_pressed && m_hovering;

        // Paint background
        {
            paint.Color = EffectivePalette.Get(ColorGroup.Active, ColorRole.Button);

            if (held)
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
            var labelY = Application.DefaultFont.Size + (TextPaddingH / 2);

            if (held)
            {
                labelY += 1;
            }

            canvas.DrawText(m_text, new SKPoint(labelX, labelY), Application.DefaultFont, paint);
        }
    }

    public void OnMouseEnter()
    {
        m_hovering = true;
        MouseCursor.Set(MouseCursor.CursorType.Hand);

        Invalidate();
    }

    public void OnMouseLeave()
    {
        m_hovering = false;
        MouseCursor.Set(MouseCursor.CursorType.Arrow);

        Invalidate();
    }

    public void OnMouseDown(int x, int y)
    {
        m_pressed = true;
        OnPressed?.Invoke();

        Invalidate();
    }

    public void OnMouseUp(int x, int y)
    {
        m_pressed = false;
        OnReleased?.Invoke();

        Invalidate();
    }

    public void OnMouseClick(int x, int y)
    {
        OnClicked?.Invoke();
    
        Invalidate();
    }

    private void updateSize()
    {
        Width = (int)Application.DefaultFont.MeasureText(m_text) + TextPaddingW;
        Height = (int)Application.DefaultFont.Size + 2 + TextPaddingH;
    }
}