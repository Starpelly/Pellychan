using SkiaSharp;

namespace Pellychan.GUI.Widgets
{
    public class Rect : Widget, IPaintHandler
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
    }
}
