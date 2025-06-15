using SkiaSharp;

namespace Pellychan.GUI
{
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
    }

    public class ColorPalette
    {
        private readonly SKColor[,] m_colors;

        public static ColorPalette Default { get; } = new();

        public ColorPalette()
        {
            int groupCount = Enum.GetValues<ColorGroup>().Length;
            int roleCount = Enum.GetValues<ColorRole>().Length;
            m_colors = new SKColor[groupCount, roleCount];

            // Fill with default values
            setDefaults();
        }

        public SKColor Get(ColorRole role)
        {
            return m_colors[(int)ColorGroup.Active, (int)role];
        }

        public SKColor Get(ColorGroup group, ColorRole role)
        {
            return m_colors[(int)group, (int)role];
        }

        public void Set(ColorGroup group, ColorRole role, SKColor color)
        {
            m_colors[(int)group, (int)role] = color;
        }

        #region Private methods

        private void setDefaults()
        {
            Set(ColorGroup.Active, ColorRole.Window, new SKColor(60, 61, 64));
            Set(ColorGroup.Active, ColorRole.WindowText, SKColors.Black);

            Set(ColorGroup.Active, ColorRole.Button, new SKColor(74, 75, 80));
            Set(ColorGroup.Active, ColorRole.ButtonText, SKColors.Black);

            Set(ColorGroup.Active, ColorRole.Base, new SKColor(46, 47, 49));
            Set(ColorGroup.Active, ColorRole.Text, new SKColor(208, 209, 212));

            Set(ColorGroup.Active, ColorRole.Highlight, new SKColor(191, 199, 213));
            Set(ColorGroup.Active, ColorRole.HighlightedText, new SKColor(45, 44, 39));

            Set(ColorGroup.Active, ColorRole.ButtonText, Get(ColorGroup.Active, ColorRole.Text));

            // @HACK
            Set(ColorGroup.Disabled, ColorRole.Text, new SKColor(164, 166, 168));
            Set(ColorGroup.Disabled, ColorRole.WindowText, new SKColor(164, 166, 168));

            Set(ColorGroup.Active, ColorRole.WindowText, Get(ColorRole.Text));
        }

        #endregion
    }
}