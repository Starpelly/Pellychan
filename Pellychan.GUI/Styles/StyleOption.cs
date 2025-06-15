using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace Pellychan.GUI.Styles;

public class StyleOption
{
    public enum OptionType
    {
        Button,
        TitleBar
    }
    
    public Style.StateFlag State { get; set; }
}

public class StyleOptionComplex : StyleOption
{

}

public class StyleOptionButton : StyleOption
{
    public string Text { get; set; } = string.Empty;
}

public class StyleOptionScrollBar : StyleOptionComplex
{
    public ScrollBar.SubControl Hovered { get; set; }
    public ScrollBar.SubControl Pressed { get; set; }

    public ScrollBar.SubControl ActiveSubControls { get; set; }

    public Dictionary<ScrollBar.SubControl, SKRectI> SubControlRects { get; } = [];
}