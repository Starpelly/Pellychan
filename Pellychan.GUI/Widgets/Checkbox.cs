using SkiaSharp;
using System.Threading;

namespace Pellychan.GUI.Widgets;

public class Checkbox : Widget
{
    public bool IsChecked { get; private set; }
    public string Label { get; set; } = "Checkbox";

    private bool m_hovered;

    public Checkbox(Widget? widget = null) : base(widget)
    {
        Width = 100;
        Height = 20;
    }

    public override void OnPaint(SKCanvas canvas)
    {
        var boxSize = Height - 4;
        var boxRect = new SKRect(2, 2, 2 + boxSize, 2 + boxSize);

        // Draw checkbox background
        /*
        using var boxPaint = new SKPaint
        {
            Color = m_hovered ? SKColors.LightGray : SKColors.White,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        canvas.DrawRect(boxRect, boxPaint);
        */


        // Draw border
        using var borderPaint = new SKPaint
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true,
        };
        canvas.DrawRoundRect(boxRect, 1, 1, borderPaint);

        // Draw checkmark if checked
        if (IsChecked)
        {
            using var checkPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                IsAntialias = true
            };
            canvas.DrawLine(boxRect.Left + 3, boxRect.MidY,
                            boxRect.MidX, boxRect.Bottom - 3, checkPaint);
            canvas.DrawLine(boxRect.MidX, boxRect.Bottom - 3,
                            boxRect.Right - 3, boxRect.Top + 3, checkPaint);
        }

        // Draw label
        using var skFont = new SKFont
        {
            Edging = SKFontEdging.Antialias,
            Subpixel = true,
            Hinting = SKFontHinting.Normal,
            Typeface = SKTypeface.Default,
            Size = 13
        };
        using var labelPaint = new SKPaint
        {
            Color = SKColors.White
        };
        canvas.DrawText(Label, new SKPoint(boxRect.Right + 8, (Height / 2) + (skFont.Size / 2) - 3), skFont, labelPaint);
    }

    public override void OnMouseEnter()
    {
        m_hovered = true;
        Update();
    }

    public override void OnMouseLeave()
    {
        m_hovered = false;
        Update();
    }

    public override void OnMouseDown(int x, int y)
    {
        IsChecked = !IsChecked;
        Update();
    }
}