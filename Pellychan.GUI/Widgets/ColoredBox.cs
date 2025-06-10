using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class ColoredBox : Widget
{
    private SKColor m_baseColor;
    private SKColor m_currentColor;

    public ColoredBox(Widget? parent, SKColor color, int width, int height) : base(parent)
    {
        m_baseColor = color;
        m_currentColor = color;

        Resize(width, height);
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        using var paint = new SKPaint
        {
            Color = m_currentColor,
            IsAntialias = true
        };
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, Width, Height), 10), paint);
    }

    public override void OnMouseEnter()
    {
        m_currentColor = SKColors.Yellow;
        Update();
    }

    public override void OnMouseLeave()
    {
        m_currentColor = m_baseColor;
        Update();
    }
}