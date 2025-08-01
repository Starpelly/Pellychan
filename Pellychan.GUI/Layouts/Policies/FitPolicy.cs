﻿namespace Pellychan.GUI.Layouts;

/// <summary>
/// Describes how a widget wants to grow or shrink within a layout.
/// </summary>
public struct FitPolicy
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

    public FitPolicy(Policy horizontal, Policy vertical)
    {
        Horizontal = horizontal;
        Vertical = vertical;
    }

    public static FitPolicy FixedPolicy => new(Policy.Fixed, Policy.Fixed);
    public static FitPolicy PreferredPolicy => new(Policy.Preferred, Policy.Preferred);
    public static FitPolicy ExpandingPolicy => new(Policy.Expanding, Policy.Expanding);

    public static bool operator == (FitPolicy left, FitPolicy right)
        => left.Equals(right);

    public static bool operator != (FitPolicy left, FitPolicy right)
        => !left.Equals(right);

    public override readonly bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (typeof(FitPolicy) != obj.GetType()) return false;

        var other = (FitPolicy)obj;
        return other.Horizontal == Horizontal && other.Vertical == Vertical;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Horizontal, Vertical);
    }
}