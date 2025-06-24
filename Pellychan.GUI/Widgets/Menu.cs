using Pellychan.GUI.Platform.Windows.Native;
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

    public MenuAction(string icon, string text, Action? action = null)
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
        SubMenu,
        MenuAction,
        Separator,
        Widget,
    }

    private const int XPadding = 8;
    private const int IconWidth = 20;
    private const int IconSpacing = 4;

    private int p_iconWidth => !string.IsNullOrEmpty(Icon) ? IconWidth + IconSpacing : 0;

    private int m_hoveredIndex = -1;
    private bool m_open = false;
    private bool m_hovering = false;

    private readonly MenuItemType m_itemType;

    private MenuPopup? m_popup;
    private bool m_ownsPopup = false;

    private readonly Action? m_onClick;

    internal readonly List<MenuAction> Actions = [];
    public string Title { get; set; }
    public string? Icon { get; set; }

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

    internal Menu(string title, string? icon, MenuItemType type, Action? onClick, Widget? parent = null) : base(parent)
    {
        Title = title;
        Icon = icon;

        Width = MeasureWidth();
        Height = 24;

        m_itemType = type;
        m_onClick = onClick;
    }

    public MenuAction AddAction(MenuAction action)
    {
        Actions.Add(action);
        return action;
    }

    public MenuAction AddAction(string text, Action action)
    {
        var n = new MenuAction(text, action);
        Actions.Add(n);
        return n;
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
            popup.Show();
            m_ownsPopup = false;
        }
    }

    public int MeasureWidth()
    {
        var a = p_iconWidth + (int)Application.DefaultFont.MeasureText(Title) + (XPadding * 2);
        var b = (m_itemType == MenuItemType.MenuAction) ? p_iconWidth : !string.IsNullOrEmpty(Icon) ? IconSpacing : 0; // Idk, this looks nicer

        return a + b;
    }

    #region Events

    public void OnPaint(SKCanvas canvas)
    {
        var active = m_open || m_hovering;

        var bgColor = active
            ? EffectivePalette.Get(ColorRole.Highlight)
            : EffectivePalette.Get(ColorRole.Window);
        var textColor = active
            ? EffectivePalette.Get(ColorRole.HighlightedText)
            : EffectivePalette.Get(ColorRole.Text);

        int roundness = 0;
        var labelXOffset = p_iconWidth;

        using var paint = new SKPaint();
        paint.Color = bgColor;
        paint.IsAntialias = roundness > 0;
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, Width, Height), roundness, roundness), paint);

        // Draw label
        using var textPaint = new SKPaint();
        textPaint.Color = textColor;
        textPaint.IsAntialias = true;
        canvas.DrawText(Title, labelXOffset + XPadding, 16, Application.DefaultFont, textPaint);

        // Draw icon
        if (!string.IsNullOrEmpty(Icon))
        {
            canvas.DrawText(Icon, XPadding, 16 + 4, Application.FontIcon, textPaint);
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

    public bool OnMouseDown(int x, int y)
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
                Popup(popup);
                break;
            case MenuItemType.MenuAction:
                m_onClick?.Invoke();
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