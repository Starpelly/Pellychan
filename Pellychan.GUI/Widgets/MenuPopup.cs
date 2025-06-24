using Pellychan.GUI.Layouts;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class MenuPopup : Widget, IPaintHandler
{
    private Menu? m_menu;
    private readonly List<Menu> m_menus = [];

    public MenuPopup(Widget? parent = null) : base(parent, WindowType.Popup)
    {
        // I assume the reason opening a popup takes so long is because of OpenGL initialization bullshit (maybe)

        Layout = new VBoxLayout
        {
        };
        ContentsMargins = new(1);
        AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit);
    }

    internal void SetMenu(Menu menu)
    {
        m_menu = menu;

        X = m_menu.X - 1;
        Y = m_menu.Height;
        SetPosition(X, Y);

        foreach (var m in m_menus)
        {
            m.Delete();
        }
        m_menus.Clear();

        foreach (var item in m_menu.Actions)
        {
            var newMenu = new Menu(item.Text, item.Icon, Menu.MenuItemType.MenuAction, item.Action, this)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                OnSubmitted = () =>
                {
                    m_menu.UserClose();
                }
            };
            m_menus.Add(newMenu);
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
        foreach (var item in m_menus)
        {
            var iw = item.MeasureWidth();
            if (iw > maxWidth)
                maxWidth = iw;
        }
        Width = (int)maxWidth + ContentsMargins.Left + ContentsMargins.Right;
    }

    #endregion
}