using Pellychan.GUI.Layouts;
using Pellychan.GUI.Styles;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class ScrollBar : Widget, IPaintHandler, IMouseDownHandler, IMouseMoveHandler, IMouseUpHandler
{
    public enum SubControl
    {
        None,
        AddLine,
        SubLine,
        Groove,
        Slider
    }

    private readonly Dictionary<SubControl, SKRectI> m_subControlRects = [];
    private SubControl m_hovered = SubControl.None;
    private SubControl m_pressed = SubControl.None;

    private int m_value = 0;
    private int m_minimum = 0;
    private int m_maxiumum = 99;
    private int m_pageStep = 1;

    public int Minimum
    {
        get => m_minimum;
        set => m_minimum = value;
    }

    public int Maximum
    {
        get => m_maxiumum;
        set
        {
            if (value < 0)
                throw new Exception("Maximum must NOT be less than 0!");

            m_maxiumum = value;
        }
    }

    public int Value
    {
        get => m_value;
        set
        {
            var oldValue = m_value;
            m_value = value;
            if (oldValue != value)
            {
                OnValueChanged?.Invoke(value);
                TriggerRepaint();
            }
        }
    }

    public int PageStep
    {
        get => m_pageStep;
        set => m_pageStep = value;
    }

    private bool m_dragging = false;
    private float m_dragOffset = 0;

    /// <summary>
    /// Invoked when the scroll bar's value has changed.
    /// </summary>
    public Action<int>? OnValueChanged;

    /// <summary>
    /// Invoked when the user drags the slider.
    /// </summary>
    public Action? OnSliderMoved;

    public enum ScrollOrientation
    {
        Horizontal,
        Vertical
    }

    public ScrollOrientation Orientation { get; set; } = ScrollOrientation.Vertical;

    public ScrollBar(Widget? parent = null) : base(parent)
    {
        Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Minimum);
    }

    public void OnPaint(SKCanvas canvas)
    {
        layoutSubControls();
        
        var option = new StyleOptionScrollBar
        {
            Hovered = m_hovered,
            Pressed = m_pressed,
        };
        option.InitFrom(this);

        if (m_pressed != SubControl.None)
        {
            option.ActiveSubControls |= m_pressed;
            option.State |= Style.StateFlag.Sunken;
        }

        foreach (var kv in m_subControlRects)
            option.SubControlRects[kv.Key] = kv.Value;

        Application.DefaultStyle.DrawScrollBar(canvas, this, option);
    }

    private void layoutSubControls()
    {
        int btnSize = 14;

        var subLine = new SKRectI(0, 0, Width, btnSize);
        var addLine = new SKRectI(0, Height - btnSize, Width, Height);

        var groove = new SKRectI(0, btnSize, Width, Height - btnSize);

        float sliderHeight = Math.Max((float)PageStep / (Maximum - Minimum + PageStep) * groove.Height, 10);
        float sliderY = groove.Top + ((float)(Value - Minimum) / (Maximum - Minimum)) * (groove.Height - sliderHeight);
        var slider = new SKRectI(0, (int)sliderY, Width, (int)sliderY + (int)sliderHeight + 1);

        m_subControlRects[SubControl.SubLine] = subLine;
        m_subControlRects[SubControl.AddLine] = addLine;
        m_subControlRects[SubControl.Groove] = groove;
        m_subControlRects[SubControl.Slider] = slider;
    }

    private SubControl hitTest(int x, int y)
    {
        foreach (var kv in m_subControlRects.AsEnumerable().Reverse())
        {
            if (kv.Value.Contains(x, y))
                return kv.Key;
        }
        return SubControl.None;
    }

    public void OnMouseDown(int x, int y)
    {
        m_pressed = hitTest(x, y);
        if (m_pressed == SubControl.Slider)
        {
            m_dragging = true;
            m_dragOffset = y - m_subControlRects[SubControl.Slider].Top;
        }
        else if (m_pressed == SubControl.SubLine)
        {
            Value = Math.Max(Minimum, Value - PageStep);
        }
        else if (m_pressed == SubControl.AddLine)
        {
            Value = Math.Min(Maximum, Value + PageStep);
        }
        TriggerRepaint();
    }

    public void OnMouseMove(int x, int y)
    {
        if (m_dragging)
        {
            var groove = m_subControlRects[SubControl.Groove];
            float sliderHeight = m_subControlRects[SubControl.Slider].Height;

            // @Crash - There's an exception thrown here when the bottom of the scrollbar is less than the top?
            // Investigate by fullscreening on the 2k monitor.

            // This is hack I think to fix the above....
            var min = Math.Min(groove.Top, groove.Bottom - sliderHeight);
            var max = Math.Max(groove.Top, groove.Bottom - sliderHeight);
            float newTop = Math.Clamp(y - m_dragOffset, min, max);

            float ratio = (newTop - groove.Top) / (groove.Height - sliderHeight);
            Value = Minimum + (int)(ratio * (Maximum - Minimum));
            TriggerRepaint();
        }
        else
        {
            m_hovered = hitTest(x, y);
        }
    }

    public void OnMouseUp(int x, int y)
    {
        m_pressed = SubControl.None;
        m_dragging = false;
        TriggerRepaint();
    }
}