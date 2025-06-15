using SkiaSharp;

namespace Pellychan.GUI.Widgets
{
    public class Rect : Widget, IPaintHandler, IMouseEnterHandler, IMouseLeaveHandler, IMouseClickHandler
    {
        private readonly SKPaint m_paint = new()
        {
            Color = SKColors.Red,
            IsAntialias = true,
        };

        public SKColor Color { get; set; } = SKColors.Black;

        public Rect(SKColor color, Widget? parent = null) : base(parent)
        {
            Color = color;
            m_paint.Color = color;
        }

        public void OnPaint(SKCanvas canvas)
        {
            var roundness = 0;
            if (roundness == 0)
            {
                canvas.DrawRect(new SKRect(0, 0, Width, Height), m_paint);
            }
            else
            {
                canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, Width, Height), roundness), m_paint);
            }
        }

        public void OnMouseEnter()
        {
            m_paint.Color = SKColors.White;
            // MouseCursor.Set(MouseCursor.CursorType.Hand);

            Invalidate();
        }

        public void OnMouseLeave()
        {
            m_paint.Color = Color;
            // MouseCursor.Set(MouseCursor.CursorType.Arrow);

            Invalidate();
        }

        public void OnMouseDown(int x, int y)
        {
        }

        public void OnMouseClick(int x, int y)
        {
            Console.WriteLine(Color);
        }
    }
}
