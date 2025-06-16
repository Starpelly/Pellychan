namespace Pellychan.GUI.Widgets;

/// <summary>
/// Null widget. It takes up space and absorbs events.
/// Does nothing else.
/// </summary>
public class NullWidget : Widget
{
    public NullWidget(Widget? parent = null) : base(parent)
    {
    }
}