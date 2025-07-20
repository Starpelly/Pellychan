using Pellychan.GUI.Layouts;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class MenuPopup : Widget, IPaintHandler
{
    private Menu? m_menu = null;
    private readonly List<Widget> m_widgetItems = [];

    /// <summary>
    /// By default, the popup will close & delete itself once a MenuAction is triggered.
    /// You can override this behavior by setting this.
    /// </summary>
    public Action? OnSubmitted = null;

    public Menu? Menu => m_menu;

    public MenuPopup(Widget? parent = null) : base(parent, WindowType.Popup)
    {
        // I assume the reason opening a popup takes so long is because of OpenGL initialization bullshit (maybe)

        Layout = new VBoxLayout
        {
        };
        ContentsMargins = new(0);
        AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit);
    }

    public void SetMenu(Menu? menu)
    {
        m_menu = menu;

        foreach (var m in m_widgetItems)
        {
            m.Delete();
        }
        m_widgetItems.Clear();

        if (m_menu != null)
        {
            foreach (var item in m_menu.Actions)
            {
                var newMenu = new Menu(item, item.IsSeparator ? Menu.MenuItemType.Separator : Menu.MenuItemType.MenuAction, this)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    OnSubmitted = () =>
                    {
                        m_menu.UserClose();
                        Submit();
                    }
                };
                m_widgetItems.Add(newMenu);
            }
        }
        fitContent();
        Show();
    }

    public void OnPaint(SKCanvas canvas)
    {
        const bool rounded = false;

        using var paint = new SKPaint();
        if (rounded)
            paint.IsAntialias = true;

        // Background
        paint.Color = Palette.Get(ColorRole.Window);

        if (rounded)
            canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, Width, Height), 8, 8), paint);
        else
            canvas.DrawRect(0, 0, Width, Height, paint);

        // Border
        paint.IsStroke = true;
        paint.Color = Application.DefaultStyle.GetFrameColor().Lighter(1.1f);

        if (rounded)
            canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, Width, Height), 8, 8), paint);
        else
            canvas.DrawRect(0, 0, Width - 1, Height - 1, paint);

        if (rounded)
        {
            canvas.ClipRoundRect(new SKRoundRect(new SKRect(0, 0, Width, Height), 8, 8), SKClipOperation.Intersect, true);
        }

        // 

        /*

        paint.IsStroke = false;
        paint.Color = EffectivePalette.Get(ColorRole.Text);
        for (var i = 0; i < m_menu.Items.Count; i++)
        {
            var item = m_menu.Items[i];
            canvas.DrawText(item.Text, new SKPoint(8, (i * ItemHeight) + (Application.DefaultFont.Size)), Application.DefaultFont, paint);
        }*/

    }

    #region Internal methods

    internal void Submit()
    {
        if (OnSubmitted != null)
            OnSubmitted.Invoke();
        else
            this.Delete();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>True if it will close</returns>
    internal bool RequestClose()
    {
        Submit();

        return true;
    }

    #endregion

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