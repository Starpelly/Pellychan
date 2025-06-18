namespace Pellychan.GUI.Widgets;

/// <summary>
/// Null widget. It takes up space and does NOT absorb events.
/// Does nothing else.
/// </summary>
public class NullWidget : Widget
{
    public NullWidget(Widget? parent = null) : base(parent)
    {
        CatchCursorEvents = false;
    }
}