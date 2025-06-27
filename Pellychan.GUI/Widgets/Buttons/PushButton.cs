using Pellychan.GUI.Styles;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class PushButton : Widget, IPaintHandler, IMouseEnterHandler, IMouseLeaveHandler, IMouseDownHandler, IMouseUpHandler, IMouseClickHandler
{
    public const int TextPaddingW = 16;
    public const int TextPaddingH = 16;

    private string m_text = string.Empty;
    public string Text
    {
        get
        {
            return m_text;
        }
        set
        {
            m_text = value;
            updateSize();
        }
    }

    private bool m_hovering = false;
    private bool m_pressed = false;

    public Action? OnClicked;
    public Action? OnPressed;
    public Action? OnReleased;

    public PushButton(string text, Widget? parent = null) : base(parent)
    {
        Text = text;
    }

    public void OnPaint(SKCanvas canvas)
    {
        var option = new StyleOptionButton
        {
            Text = Text
        };
        option.InitFrom(this);

        if (m_hovering && m_pressed)
            option.State |= Style.StateFlag.Sunken;
        
        Application.DefaultStyle.DrawPushButton(canvas, this, option);
    }

    public void OnMouseEnter()
    {
        m_hovering = true;
        MouseCursor.Set(MouseCursor.CursorType.Hand);

        TriggerRepaint();
    }

    public void OnMouseLeave()
    {
        m_hovering = false;
        MouseCursor.Set(MouseCursor.CursorType.Arrow);

        TriggerRepaint();
    }

    public bool OnMouseDown(MouseEvent evt)
    {
        m_pressed = true;
        OnPressed?.Invoke();

        TriggerRepaint();

        return true;
    }

    public bool OnMouseUp(MouseEvent evt)
    {
        m_pressed = false;
        OnReleased?.Invoke();

        TriggerRepaint();

        return true;
    }

    public bool OnMouseClick(MouseEvent evt)
    {
        OnClicked?.Invoke();
    
        TriggerRepaint();

        return true;
    }

    private void updateSize()
    {
        Resize((int)Application.DefaultFont.MeasureText(m_text) + TextPaddingW, (int)Application.DefaultFont.Size + 2 + TextPaddingH);
    }
}