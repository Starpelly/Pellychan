using SkiaSharp;

namespace Pellychan.GUI.Widgets
{
    public class Rect : Widget, IPaintHandler, IMouseEnterHandler, IMouseLeaveHandler
    {
        private readonly SKPaint m_paint = new()
        {
            Color = SKColors.Red
        };

        public SKColor Color { get; set; } = SKColors.Black;

        public Rect(SKColor color)
        {
            Color = color;
            m_paint.Color = color;
        }

        public void OnPaint(SKCanvas canvas)
        {
            canvas.DrawRect(new SKRect(0, 0, Width, Height), m_paint);
        }

        public void OnMouseEnter()
        {
            m_paint.Color = SKColors.White;
            MouseCursor.Set(MouseCursor.CursorType.Hand);

            Invalidate();
        }

        public void OnMouseLeave()
        {
            m_paint.Color = Color;
            MouseCursor.Set(MouseCursor.CursorType.Arrow);

            Invalidate();
        }
    }
}
