using Pellychan.GUI.Layouts;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class MenuPopup : Widget, IPaintHandler
{
    private Menu? m_menu;
    private readonly List<Widget> m_widgetItems = [];

    public MenuPopup(Widget? parent = null) : base(parent, WindowType.Popup)
    {
        // I assume the reason opening a popup takes so long is because of OpenGL initialization bullshit (maybe)

        Layout = new VBoxLayout
        {
        };
        ContentsMargins = new(0);
        AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit);
    }

    public void SetMenu(Menu menu)
    {
        m_menu = menu;

        foreach (var m in m_widgetItems)
        {
            m.Delete();
        }
        m_widgetItems.Clear();

        foreach (var item in m_menu.Actions)
        {
            var newMenu = new Menu(item, item.IsSeparator ? Menu.MenuItemType.Separator : Menu.MenuItemType.MenuAction, this)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                OnSubmitted = () =>
                {
                    m_menu.UserClose();
                    this.Delete();
                }
            };
            m_widgetItems.Add(newMenu);
        }

        fitContent();
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();
        paint.Color = Palette.Get(ColorRole.Window);
        canvas.DrawRect(0, 0, Width, Height, paint);

        paint.IsStroke = true;
        paint.Color = Application.DefaultStyle.GetFrameColor().Lighter(1.1f);
        canvas.DrawRect(0, 0, Width - 1, Height - 1, paint);

        /*

        paint.IsStroke = false;
        paint.Color = EffectivePalette.Get(ColorRole.Text);
        for (var i = 0; i < m_menu.Items.Count; i++)
        {
            var item = m_menu.Items[i];
            canvas.DrawText(item.Text, new SKPoint(8, (i * ItemHeight) + (Application.DefaultFont.Size)), Application.DefaultFont, paint);
        }*/

    }

    #region Private methods

    private void fitContent()
    {
        var maxWidth = 10f;
        foreach (var item in m_widgetItems)
        {
            if (item is Menu menu)
            {
                var iw = menu.MeasureWidth();
                if (iw > maxWidth)
                    maxWidth = iw;
            }
        }
        Width = (int)maxWidth + ContentsMargins.Left + ContentsMargins.Right;
    }

    #endregion
}