using SkiaSharp;

namespace Pellychan.GUI.Widgets
{
    public class MenuItem
    {
        public string Text;
        public Action? OnClick;

        public MenuItem(string text, Action? onClick = null)
        {
            Text = text;
            OnClick = onClick;
        }
    }

    public class Menu : Widget, IPaintHandler, IMouseMoveHandler, IMouseEnterHandler, IMouseLeaveHandler,
        IMouseDownHandler
    {
        private const int XPadding = 8;

        public string Title;
        public readonly List<MenuItem> Items = [];

        private int m_hoveredIndex = -1;
        private bool m_open = false;
        private bool m_hovering = false;

        public Menu(string title, Widget? parent = null) : base(parent)
        {
            Title = title;
            Width = (int)Application.DefaultFont.MeasureText(title) + (XPadding * 2);
            Height = 24;
        }

        public void AddItem(MenuItem item)
        {
            Items.Add(item);
        }

        public void AddItem(string text, Action? onClick = null)
        {
            Items.Add(new MenuItem(text, onClick));
        }

        public void OnPaint(SKCanvas canvas)
        {
            var active = m_open || m_hovering;

            var bgColor = active
                ? EffectivePalette.Get(ColorRole.Highlight)
                : EffectivePalette.Get(ColorRole.Window);
            var textColor = active
                ? EffectivePalette.Get(ColorRole.HighlightedText)
                : EffectivePalette.Get(ColorRole.Text);

            using var paint = new SKPaint();
            paint.Color = bgColor;
            canvas.DrawRect(0, 0, Width, Height, paint);

            using var textPaint = new SKPaint();
            textPaint.Color = textColor;
            textPaint.IsAntialias = true;
            canvas.DrawText(Title, XPadding, 16, Application.DefaultFont, textPaint);

            if (m_open)
            {
                int y = Height;
                for (int i = 0; i < Items.Count; i++)
                {
                    var item = Items[i];
                    var isHovered = i == m_hoveredIndex;

                    var itemBg = isHovered ? SKColors.LightBlue : SKColors.White;
                    paint.Color = itemBg;
                    canvas.DrawRect(0, y, Width, 24, paint);

                    canvas.DrawText(item.Text, 5, y + 16, Application.DefaultFont, textPaint);
                    y += 24;
                }
            }
        }

        public void OnMouseMove(int x, int y)
        {
            if (!m_open) return;

            int itemIndex = (y - Height) / 24;
            if (itemIndex >= 0 && itemIndex < Items.Count)
            {
                m_hoveredIndex = itemIndex;
                TriggerRepaint();
            }
            else if (m_hoveredIndex != -1)
            {
                m_hoveredIndex = -1;
                TriggerRepaint();
            }
        }

        public void OnMouseDown(int x, int y)
        {
            if (!m_open && y < Height)
            {
                m_open = true;
                TriggerRepaint();
            }
            else if (m_open && y >= Height)
            {
                int itemIndex = (y - Height) / 24;
                if (itemIndex >= 0 && itemIndex < Items.Count)
                {
                    Items[itemIndex].OnClick?.Invoke();
                    m_open = false;
                    TriggerRepaint();
                }
            }
            else
            {
                m_open = false;
                TriggerRepaint();
            }
        }

        public void OnMouseEnter()
        {
            m_hovering = true;
        }

        public void OnMouseLeave()
        {
            m_hovering = false;
        }
    }

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