namespace Pellychan.GUI.Widgets;

public partial class Widget
{
    public enum WindowType
    {
        /// <summary>
        /// The default type for <see cref="Widgets.Widget"/>. Widgets of this type are child widgets if they have a parent,
        /// and independent windows if they have no parent.
        /// </summary>
        Widget = 0,

        /// <summary>
        /// Indicates that the window is a window, usually with a window system frame and a title bar, irrespective of
        /// whether the widget has a parent or not. Note that it's not possible to unset this flag if the widget does
        /// not have a parent.
        /// </summary>
        Window,

        /// <summary>
        /// Indicates that the widget is a window that should be decorated as a dialog (i.e., typically no maximize and
        /// minimize buttons in the title bar).
        /// </summary>
        Dialog,

        /// <summary>
        /// Indicates that the widget is a pop-up top-level window, i.e. that it is a modal, but has a window system frame
        /// appropriate for pop-up menus.
        /// </summary>
        Popup,

        /// <summary>
        /// Indicates that the widget is a tool window.
        /// </summary>
        Tool,

        /// <summary>
        /// Indicates that the widget is a tooltip.
        /// </summary>
        ToolTip
    }
}