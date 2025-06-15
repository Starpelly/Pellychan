namespace Pellychan.GUI.Layouts;

/// <summary>
/// Describes how a widget wants to grow or shrink within a layout.
/// </summary>
public struct SizePolicy
{
    /// <summary>
    /// Defines the resizing behavior in one dimension.
    /// </summary>
    public enum Policy
    {
        /// <summary>
        /// The widget cannot grow or shrink. It will always be its preferred size.
        /// </summary>
        Fixed,

        /// <summary>
        /// The widget prefers to be no smaller than its minimum size but can grow.
        /// </summary>
        Minimum,

        /// <summary>
        /// The widget can shrink down to its minimum size but prefers not to grow.
        /// </summary>
        Maximum,

        /// <summary>
        /// The widget prefers to be its preferred size but can grow or shrink as needed.
        /// </summary>
        Preferred,

        /// <summary>
        /// The widget prefers to expand to take up available space.
        /// </summary>
        Expanding,

        /// <summary>
        /// Similar to Minimum, but if there’s space, the widget wants to grow.
        /// </summary>
        MinimumExpanding,

        /// <summary>
        /// The layout ignores the widget’s size hints entirely and can resize it freely.
        /// </summary>
        Ignored
    }

    public Policy Horizontal { get; set; }
    public Policy Vertical { get; set; }

    public SizePolicy(Policy horizontal, Policy vertical)
    {
        Horizontal = horizontal;
        Vertical = vertical;
    }

    public static SizePolicy FixedPolicy => new(Policy.Fixed, Policy.Fixed);
    public static SizePolicy PreferredPolicy => new(Policy.Preferred, Policy.Preferred);
    public static SizePolicy ExpandingPolicy => new(Policy.Expanding, Policy.Expanding);
}