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

        private Menu? m_openedMenu = null;

        private MenuPopup m_stdPopup;
        private bool m_popupOpen = false;

        public MenuBar(Widget? parent = null) : base(parent)
        {
            Height = MenuBarHeight + BorderSize;
            m_stdPopup = new MenuPopup(this);
            m_stdPopup.OnSubmitted = () =>
            {
                m_stdPopup.Hide();
            };
        }

        private void AddMenu(Menu menu)
        {
            if (m_menus.Contains(menu))
                throw new Exception("Please don't add the same menu multiple times!");

            menu.SetPosition(m_nextX, 0);
            m_nextX += menu.Width;
            menu.Height = MenuBarHeight;

            menu.OnHovered = () =>
            {
                if (m_popupOpen && m_openedMenu != menu)
                {
                    m_openedMenu?.Close();
                    menu.Open();
                }
            };
            menu.OnUserOpened = () =>
            {
                m_openedMenu = menu;
                m_popupOpen = true;

                menu.Open();
                m_stdPopup.SetMenu(menu);
                m_stdPopup.SetPosition(menu.X, menu.Height);
                m_stdPopup.Show();
            };
            menu.OnUserClosed = () =>
            {
                if (m_openedMenu != menu) return;

                m_openedMenu = null;
                m_popupOpen = false;

                m_stdPopup.SetMenu(null);
                m_stdPopup.Hide();
            };

            m_menus.Add(menu);
        }

        public Menu AddMenu(string title)
        {
            var t = new Menu(new MenuAction(title), Menu.MenuItemType.MenuBarSubMenu, this);
            AddMenu(t);
            return t;
        }

        public Menu AddMenu(string icon, string title)
        {
            var t = new Menu(new MenuAction(icon, title), Menu.MenuItemType.MenuBarSubMenu, this);
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

        public bool OnMouseDown(MouseEvent evt)
        {
            return true;
        }
    }
}