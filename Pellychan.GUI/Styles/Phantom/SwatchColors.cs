namespace Pellychan.GUI.Styles.Phantom;

public enum SwatchColor : int
{
    None = 0,
    Window,
    Button,
    Base,
    Text,
    WindowText,
    Highlight,
    HighlightedText,
    ScrollbarGutter,
    Window_Outline,
    Window_Specular,
    Window_Divider,
    Window_Lighter,
    Window_Darker,
    Button_Specular,
    Button_Pressed,
    Button_Pressed_Specular,
    Base_Shadow,
    Base_Divider,
    WindowText_Disabled,
    Highlight_Outline,
    Highlight_Specular,
    ProgressBar_Outline,
    Indicator_Current,
    Indicator_Disabled,
    ScrollbarGutter_Disabled,

    Num,

    // Aliases
    ProgressBar = Highlight,
    ProgressBar_Specular = Highlight_Specular,
    TabFrame = Window,
    TabFrame_Specular = Window_Specular
}