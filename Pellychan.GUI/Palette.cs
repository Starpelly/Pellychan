using SkiaSharp;

namespace Pellychan.GUI;

public enum ColorGroup
{
    /// <summary>
    /// Used for the window that has keyboard focus.
    /// </summary>
    Active,

    /// <summary>
    /// Used for other windows.
    /// </summary>
    Inactive,

    /// <summary>
    /// Used for widgets (not windows) that are disabled for some reason.
    /// </summary>
    Disabled,
}

public enum ColorRole
{
    /// <summary>
    /// A general background color.
    /// </summary>
    Window,

    /// <summary>
    /// A general foreground color.
    /// </summary>
    WindowText,

    /// <summary>
    /// Used mostly as the background color for text entry widgets,
    /// but can also be used for other painting such as the background of combobox
    /// drop down lists and toolbar handles. It is usually white or another light color. 
    /// </summary>
    Base,

    /// <summary>
    /// Used as the alternate background color in views with alternating row colors.
    /// </summary>
    AlternateBase,

    /// <summary>
    /// The foreground color used with <see cref="Base"/>. This is usually the same
    /// as the <see cref="WindowText"/>, in which case it must provide good contrast with
    /// <see cref="Window"/> and <see cref="Base"/>.
    /// </summary>
    Text,

    /// <summary>
    /// The general button background color. This background can be different from <see cref="Window"/>
    /// as some styles require a different background color for buttons.
    /// </summary>
    Button,

    /// <summary>
    /// A foreground color used with the <see cref="Button"/> color.
    /// </summary>
    ButtonText,

    /// <summary>
    /// A text color that is very different from <see cref="WindowText"/>
    /// </summary>
    BrightText,

    /// <summary>
    /// A color to indicate a selected item or the current item.
    /// </summary>
    Highlight,

    /// <summary>
    /// A text color that contrasts with <see cref="Highlight"/>. By default, the highlighted
    /// text color is White.
    /// </summary>
    HighlightedText,

    /// <summary>
    /// A text color used for unvisited hyperlinks.
    /// </summary>
    Link,

    /// <summary>
    /// A text color used for already visited hyperlinks.
    /// </summary>
    LinkVisited,

    /// <summary>
    /// Lighter than <see cref="Button"/> color.
    /// </summary>
    Light,

    /// <summary>
    /// Between <see cref="Button"/> and <see cref="Light"/>.
    /// </summary>
    Midlight,

    /// <summary>
    /// Darker than <see cref="Button"/>.
    /// </summary>
    Dark,

    /// <summary>
    /// Between <see cref="Button"/> and <see cref="Dark"/>.
    /// </summary>
    Mid,

    /// <summary>
    /// A very dark color. By default, the shadow color is black.
    /// </summary>
    Shadow
}

public class ColorPalette
{
    private readonly SKColor[,] m_colors;

    private ColorGroup m_currentGroup = ColorGroup.Active;
    public ColorGroup CurrentColorGroup
    {
        get => m_currentGroup;
        set => m_currentGroup = value;
    }

    public ColorPalette()
    {
        int groupCount = Enum.GetValues<ColorGroup>().Length;
        int roleCount = Enum.GetValues<ColorRole>().Length;
        m_colors = new SKColor[groupCount, roleCount];
    }

    public SKColor Get(ColorRole role)
    {
        return m_colors[(int)m_currentGroup, (int)role];
    }

    public SKColor Get(ColorGroup group, ColorRole role)
    {
        return m_colors[(int)group, (int)role];
    }

    public void Set(ColorGroup group, ColorRole role, SKColor color)
    {
        m_colors[(int)group, (int)role] = color;
    }

    public void Set(ColorRole role, SKColor color)
    {
        m_colors[(int)m_currentGroup, (int)role] = color;
    }
}