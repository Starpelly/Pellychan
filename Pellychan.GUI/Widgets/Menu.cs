using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public interface IMenu
{

}

public class MenuSeparator : IMenu
{

}

public class MenuAction : IMenu
{
    public string? Icon;
    public string Text;
    public Action? Action;

    public bool IsSeparator = false;

    public MenuAction(string? icon, string text, Action? action = null)
    {
        Icon = icon;
        Text = text;
        Action = action;
    }

    public MenuAction(string text, Action? action = null)
    {
        Text = text;
        Action = action;
    }
}

/// <summary>
/// Provides a menu widget for use in menu bars, context menus, and other popup menus.
/// </summary>
public class Menu : Widget, IPaintHandler, IMouseMoveHandler, IMouseEnterHandler, IMouseLeaveHandler,
        IMouseDownHandler
{
    internal enum MenuItemType
    {
        MenuBarSubMenu,
        SubMenu,
        MenuAction,
        Separator,
        Widget,
    }

    private const int XPadding = 8;
    private const int IconWidth = 20;
    private const int IconSpacing = 4;

    private static int p_iconWidth => IconWidth + IconSpacing;

    private int m_hoveredIndex = -1;
    private bool m_open = false;
    private bool m_hovering = false;

    private readonly MenuItemType m_itemType;

    private MenuPopup? m_popup;
    private bool m_ownsPopup = false;

    private MenuAction m_action;

    internal readonly List<MenuAction> Actions = [];

    #region Internal events

    /// <summary>
    /// Used to close the popup that hosts this menu.
    /// </summary>
    internal Action? OnSubmitted;

    /// <summary>
    /// Used to move a popup when hovering over a new menu.
    /// </summary>
    internal Action? OnHovered;

    internal Action? OnUserOpened;
    internal Action? OnUserClosed;

    #endregion

    public Menu(Widget? parent = null) : base(parent)
    {
    }

    internal Menu(MenuAction action, MenuItemType type, Widget? parent = null) : base(parent)
    {
        m_action = action;

        Width = MeasureWidth();
        if (type == MenuItemType.Separator)
            Height = 1;
        else
            Height = 24;

        m_itemType = type;
    }

    public MenuAction AddAction(MenuAction action)
    {
        Actions.Add(action);
        return action;
    }

    public MenuAction AddAction(string icon, string text, Action action)
    {
        return AddAction(new MenuAction(icon, text, action));
    }

    public MenuAction AddAction(string text, Action action)
    {
        var n = new MenuAction(text, action);
        Actions.Add(n);
        return n;
    }

    public void AddSeparator()
    {
        var n = new MenuAction("", null);
        n.IsSeparator = true;
        Actions.Add(n);
    }

    internal void Popup(MenuPopup? popup)
    {
        if (popup == null)
        {
            m_popup = new MenuPopup(this)
            {
            };
            m_popup.Show();
            m_ownsPopup = true;
        }
        else
        {
            m_popup = popup;
            popup.SetMenu(this);
            popup.SetPosition(this.X, this.Height);

            popup.Show();
            m_ownsPopup = false;
        }
    }

    public int MeasureWidth()
    {
        var a = 0;
        var b = 0;

        if (m_itemType == MenuItemType.MenuBarSubMenu)
        {
            a = (int)Application.DefaultFont.MeasureText(m_action.Text) + (XPadding * 2);
        }
        else
        {
            a = p_iconWidth + (int)Application.DefaultFont.MeasureText(m_action.Text) + (XPadding * 2);
            b = (m_itemType == MenuItemType.MenuAction) ? p_iconWidth : !string.IsNullOrEmpty(m_action.Icon) ? IconSpacing : 0; // Idk, this looks nicer
        }

        return a + b;
    }

    #region Events

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();

        if (m_action.IsSeparator)
        {
            paint.Color = Application.DefaultStyle.GetFrameColor().Lighter(1.1f);
            canvas.DrawLine(IconWidth + IconSpacing + XPadding, 0, Width - XPadding, 0, paint);

            return;
        }

        var active = m_open || m_hovering;

        var bgColor = active
            ? EffectivePalette.Get(ColorRole.Highlight)
            : EffectivePalette.Get(ColorRole.Window).WithAlpha(0);
        var textColor = active
            ? EffectivePalette.Get(ColorRole.HighlightedText)
            : EffectivePalette.Get(ColorRole.Text);

        int roundness = 0;
        var labelXOffset = m_itemType == MenuItemType.MenuBarSubMenu ? 0 : IconWidth + IconSpacing;


        paint.Color = bgColor;
        paint.IsAntialias = roundness > 0;
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, Width, Height), roundness, roundness), paint);

        // Draw label
        using var textPaint = new SKPaint();
        textPaint.Color = textColor;
        textPaint.IsAntialias = true;
        canvas.DrawText(m_action.Text, labelXOffset + XPadding, 16, Application.DefaultFont, textPaint);

        // Draw icon
        if (!string.IsNullOrEmpty(m_action.Icon))
        {
            canvas.DrawText(m_action.Icon, XPadding, 16 + 4, Application.FontIcon, textPaint);
        }
    }

    public bool OnMouseMove(int x, int y)
    {
        if (!m_open) return false;

        int itemIndex = (y - Height) / 24;
        if (itemIndex >= 0 && itemIndex < Actions.Count)
        {
            m_hoveredIndex = itemIndex;
            TriggerRepaint();
        }
        else if (m_hoveredIndex != -1)
        {
            m_hoveredIndex = -1;
            TriggerRepaint();
        }

        return true;
    }

    public bool OnMouseDown(MouseEvent evt)
    {
        if (!m_open)
        {
            if (OnUserOpened != null)
                OnUserOpened?.Invoke();
            else
                Open(null);
        }
        else
        {
            UserClose();
        }

        return true;
    }

    public void OnMouseEnter()
    {
        m_hovering = true;
        OnHovered?.Invoke();
    }

    public void OnMouseLeave()
    {
        m_hovering = false;
    }

    #endregion

    #region Internal methods

    internal void Open(MenuPopup? popup)
    {
        if (m_open) return;

        m_open = true;

        switch (m_itemType)
        {
            case MenuItemType.SubMenu:
            case MenuItemType.MenuBarSubMenu:
                Popup(popup);
                break;
            case MenuItemType.MenuAction:
                m_action.Action?.Invoke();
                OnSubmitted?.Invoke();
                break;
        }

        TriggerRepaint();
    }

    internal void Close()
    {
        if (!m_open) return;

        m_open = false;

        if (m_ownsPopup)
        {
            m_popup?.Delete();
        }
        m_popup = null;

        TriggerRepaint();
    }

    internal void UserClose()
    {
        OnUserClosed?.Invoke();
        m_popup?.Hide();
        Close();
    }

    #endregion
}