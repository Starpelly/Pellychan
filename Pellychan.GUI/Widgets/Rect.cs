using SkiaSharp;

namespace Pellychan.GUI.Widgets
{
    public class Rect : Widget
    {
        private readonly SKPaint m_paint = new()
        {
            Color = SKColors.Red
        };

        public override void OnPaint(SKCanvas canvas)
        {
            base.OnPaint(canvas);

            canvas.DrawRect(new SKRect(0, 0, Width, Height), m_paint);
        }

        public override void OnMouseEnter()
        {
            base.OnMouseEnter();

            m_paint.Color = SKColors.Blue;
            MouseCursor.Set(MouseCursor.CursorType.Hand);

            Invalidate();
        }

        public override void OnMouseLeave()
        {
            base.OnMouseLeave();

            m_paint.Color = SKColors.Red;
            MouseCursor.Set(MouseCursor.CursorType.Arrow);

            Invalidate();
        }
    }
}
