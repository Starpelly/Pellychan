using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class PushButton : Widget
{
    public bool Hovering { get; private set; } = false;

    public string Label { get; set; } = string.Empty;

    public PushButton(Widget? parent = null) : base(parent)
    {
        CursorShape = MouseCursor.CursorType.Hand;
    }
    
    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        // Background
        {
            using var paint = new SKPaint();
            paint.Color = Hovering ? SKColors.DarkGray : SKColors.DimGray;
            paint.IsAntialias = true;

            canvas.DrawRoundRect(new SKRect(0, 0, Width, Height), 6, 6, paint);
        }
        
        // Draw label
        {
            using var skFont = new SKFont();
            skFont.Edging = SKFontEdging.Antialias;
            skFont.Subpixel = true;
            skFont.Hinting = SKFontHinting.Normal;
            skFont.Typeface = SKTypeface.Default;
            skFont.Size = 13;
        
            using var labelPaint = new SKPaint();
            labelPaint.Color = SKColors.White;
        
            canvas.DrawText(Label, new SKPoint(Width * 0.5f, Y * 0.5f), SKTextAlign.Center, skFont, labelPaint);
        }
    }

    public override void OnMouseEnter()
    {
        base.OnMouseEnter();

        Hovering = true;
        
        Update();
    }

    public override void OnMouseLeave()
    {
        base.OnMouseLeave();
        
        Hovering = false;
        
        Update();
    }
}