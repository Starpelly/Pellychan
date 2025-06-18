using Pellychan.GUI.Layouts;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class ScrollArea : Widget, IMouseWheelHandler
{
    public NullWidget ContentFrame { get; private set; }
    public ScrollBar VerticalScrollbar { get; private set; }

    private Widget? m_childWidget;
    public Widget? ChildWidget
    {
        get => m_childWidget;
        set
        {
            setWidget(value);
        }
    }

    public ScrollArea(Widget? parent = null) : base(parent)
    {
        Layout = new HBoxLayout
        {
            Spacing = 0,
        };

        ContentFrame = new NullWidget(this)
        {
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Expanding)
        };

        VerticalScrollbar = new ScrollBar(this)
        {
            X = 400,
            Y = 16,
            Width = 16,
            Height = 400,
            Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding)
        };
        VerticalScrollbar.OnValueChanged += delegate (int value)
        {
            if (m_childWidget != null)
            {
                var minY = 0;

                if (ContentFrame.Layout != null)
                {
                    minY = -ContentFrame.Layout.Padding.Top;
                    value += minY;
                }

                m_childWidget.Y = -value;
            }
        };

        validate();
    }

    private void setWidget(Widget? newChildWidget)
    {
        if (m_childWidget != null)
        {
            m_childWidget.OnResized -= validate;
        }
        if (newChildWidget == null)
            return;

        m_childWidget = newChildWidget;
        newChildWidget.SetParent(ContentFrame);

        m_childWidget.OnResized += validate;

        validate();
    }

    public void OnMouseScroll(int x, int y, int deltaX, int deltaY)
    {
        VerticalScrollbar.Value -= deltaY * VerticalScrollbar.PageStep / 2;
        VerticalScrollbar.Value = Math.Clamp(VerticalScrollbar.Value, VerticalScrollbar.Minimum, VerticalScrollbar.Maximum);
    }

    public override void OnPostLayout()
    {
        validate();
    }

    #region Private methods

    private void validate()
    {
        // Console.WriteLine("Validate");
        fitScrollbarsToContent();
    }

    private void fitScrollbarsToContent()
    {
        if (m_childWidget == null)
        {

            return;
        }

        var maxY = 0;

        if (ContentFrame.Layout != null)
        {
            maxY = ContentFrame.Layout.Padding.Bottom * 2;
        }

        VerticalScrollbar.Minimum = 0;
        VerticalScrollbar.Maximum = Math.Max(0, (m_childWidget.Height - ContentFrame.Height) + maxY);
        VerticalScrollbar.PageStep = ContentFrame.Height;

        VerticalScrollbar.Value = Math.Clamp(VerticalScrollbar.Value, VerticalScrollbar.Minimum, VerticalScrollbar.Maximum);
        VerticalScrollbar.Enabled = VerticalScrollbar.Maximum > 0;

        // So the reason it looks as if the list scrolls back up to the top when the window is resized (or equivalent)-
        // is because the layout for m_mainContentWidget is setting the position of the list in the Layout?.PositionsPass().
        // Dunno what to do about that, maybe create a flag or something?
    }

    #endregion
}