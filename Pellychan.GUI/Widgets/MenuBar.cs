using SkiaSharp;

namespace Pellychan.GUI.Widgets
{
    public class MenuBar : Widget, IPaintHandler, IMouseDownHandler
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

        private List<Menu> m_menus = [];

        public MenuBar(Widget? parent = null) : base(parent)
        {
            Height = MenuBarHeight + BorderSize;
        }

        private void AddMenu(Menu menu)
        {
            if (m_menus.Contains(menu))
                throw new Exception("Please don't add the same menu multiple times!");

            menu.SetPosition(m_nextX, 0);
            m_nextX += menu.Width;
            menu.Height = MenuBarHeight;

            m_menus.Add(menu);
        }

        public Menu AddMenu(string title)
        {
            var t = new Menu(title, null, Menu.MenuItemType.SubMenu, null, this);
            AddMenu(t);
            return t;
        }

        public Menu AddMenu(string icon, string title)
        {
            var t = new Menu(title, icon, Menu.MenuItemType.SubMenu, null, this);
            AddMenu(t);
            return t;
        }

        public void OnPaint(SKCanvas canvas)
        {
            using var paint = new SKPaint();

            if (DrawBorder)
            {
                paint.Color = Application.DefaultStyle.GetFrameColor();
                canvas.DrawRect(0, 0, Width, Height, paint);
            }

            paint.Color = EffectivePalette.Get(ColorRole.Window);
            canvas.DrawRect(0, 0, Width, Height - BorderSize, paint);
        }

        public bool OnMouseDown(int x, int y)
        {
            return true;
        }
    }
}