using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class ToolTip : Widget, IPaintHandler
{
    public void OnPaint(SKCanvas canvas)
    {
        /*
        if (!_visible) return;

        var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true, TextSize = 14 };
        var padding = 6;
        var textBounds = new SKRect();
        paint.MeasureText(_text, ref textBounds);

        var width = textBounds.Width + 2 * padding;
        var height = textBounds.Height + 2 * padding;

        // background
        canvas.DrawRoundRect(new SKRect(0, 0, width, height), 4, 4, new SKPaint { Color = SKColors.LightYellow });

        // text
        canvas.DrawText(_text, padding, padding - textBounds.Top, paint);
        */
    }
}