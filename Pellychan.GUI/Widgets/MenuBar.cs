using SkiaSharp;

namespace Pellychan.GUI.Widgets
{
    public class MenuBar : Widget, IPaintHandler
    {
        public enum Orientation
        {
            Top,
            Left,
            Right,
            Bottom
        }

        public Orientation ScreenPosition { get; set; }

        private const int MenuBarHeight = 24;

        private const int BorderSize = 1;
        private const bool DrawBorder = BorderSize > 0;

        private int m_nextX = 0;

        public MenuBar(Widget? parent = null) : base(parent)
        {
            Height = MenuBarHeight + BorderSize;
        }

        public void AddMenu(Menu menu)
        {
            menu.SetPosition(m_nextX, 0);
            m_nextX += menu.Width;
            menu.Height = MenuBarHeight;
        }

        public void AddMenu(string title)
        {
            AddMenu(new Menu(title, this));
        }

        public void OnPaint(SKCanvas canvas)
        {
            using var paint = new SKPaint();

            if (DrawBorder)
            {
                paint.Color = new SKColor(42, 42, 45);
                canvas.DrawRect(0, 0, Width, Height, paint);
            }

            paint.Color = EffectivePalette.Get(ColorGroup.Active, ColorRole.Window);
            canvas.DrawRect(0, 0, Width, Height - BorderSize, paint);
        }
    }
}