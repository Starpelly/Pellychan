using Pellychan.GUI.Widgets;

namespace Pellychan.Widgets;

public class PreferencesWindow : MainWindow
{
    public PreferencesWindow(Widget? parent = null) : base(parent)
    {
        Resize(400, 400);
    }

    public override void OnShown()
    {
        SetWindowTitle("Preferences");
    }
}