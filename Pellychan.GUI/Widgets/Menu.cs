using SkiaSharp;

namespace Pellychan.GUI.Widgets;

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

/// <summary>
/// Provides a menu widget for use in menu bars, context menus, and other popup menus.
/// </summary>
public class Menu : Widget, IPaintHandler, IMouseMoveHandler, IMouseEnterHandler, IMouseLeaveHandler,
        IMouseDownHandler
{
    private const int XPadding = 8;

    public string Title;
    public readonly List<MenuItem> Items = [];

    private int m_hoveredIndex = -1;
    private bool m_open = false;
    private bool m_hovering = false;

    private MenuPopup? m_popup;

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

    public void Popup()
    {
        m_popup = new MenuPopup(this)
        {

        };
        m_popup.Show();
    }

    public int MeasureWidth()
    {
        return (int)Application.DefaultFont.MeasureText(Title) + (XPadding * 2);
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

        using var paint = new SKPaint();
        paint.Color = bgColor;
        paint.IsAntialias = roundness > 0;
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, Width, Height), roundness, roundness), paint);

        using var textPaint = new SKPaint();
        textPaint.Color = textColor;
        textPaint.IsAntialias = true;
        canvas.DrawText(Title, XPadding, 16, Application.DefaultFont, textPaint);
    }

    public bool OnMouseMove(int x, int y)
    {
        if (!m_open) return false;

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

        return true;
    }

    public bool OnMouseDown(int x, int y)
    {
        if (!m_open && y < Height)
        {
            m_open = true;

            Popup();

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
            m_popup?.Delete();

            m_open = false;
            TriggerRepaint();
        }

        return true;
    }

    public void OnMouseEnter()
    {
        m_hovering = true;
    }

    public void OnMouseLeave()
    {
        m_hovering = false;
    }

    #endregion
}