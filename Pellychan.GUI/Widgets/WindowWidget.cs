using Pellychan.GUI.Framework.Platform;

namespace Pellychan.GUI.Widgets;

public class WindowWidget : Widget
{
    protected WindowFlags WindowFlags = WindowFlags.None;

    /// <summary>
    /// The title of the window.
    /// </summary>
    public string Title
    {
        get => m_nativeWindow?.Window.Title ?? string.Empty;
        set
        {
            if (m_nativeWindow != null)
            {
                m_nativeWindow.Window.Title = value;
            }
        }
    }

    public WindowWidget(WindowType type, Widget? parent = null) : base(parent, type)
    {
        CreateWinID();
    }
}