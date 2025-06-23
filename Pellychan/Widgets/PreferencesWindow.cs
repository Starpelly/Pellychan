using Pellychan.GUI.Widgets;

namespace Pellychan.Widgets;

public class PreferencesWindow : MainWindow
{
    public PreferencesWindow(Widget? parent = null) : base(parent)
    {
        Resize(400, 400);

        new PushButton("Save", this)
        {
            X = 16,
            Y = 16,
            Width = 64
        };
    }

    public override void OnShown()
    {
        SetWindowTitle("Preferences");
    }
}