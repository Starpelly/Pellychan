namespace Pellychan.GUI.Layouts;

/// <summary>
/// Describes how a widget wants to size, if not affected by a layout.
/// </summary>
public struct SizePolicy
{
    public enum Policy
    {
        /// <summary>
        /// Will not be affected by its children.
        /// </summary>
        Fixed,

        /// <summary>
        /// Will size itself to fit its children.
        /// </summary>
        Fit
    }

    public Policy Horizontal { get; set; }
    public Policy Vertical { get; set; }

    public SizePolicy(Policy horizontal, Policy vertical)
    {
        Horizontal = horizontal;
        Vertical = vertical;
    }

    public static SizePolicy FixedPolicy => new(Policy.Fixed, Policy.Fixed);
    public static SizePolicy FitPolicy => new(Policy.Fit, Policy.Fit);

    public static bool operator ==(SizePolicy left, SizePolicy right)
        => left.Equals(right);

    public static bool operator !=(SizePolicy left, SizePolicy right)
        => !left.Equals(right);

    public override readonly bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (typeof(SizePolicy) != obj.GetType()) return false;

        var other = (SizePolicy)obj;
        return other.Horizontal == Horizontal && other.Vertical == Vertical;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Horizontal, Vertical);
    }
}