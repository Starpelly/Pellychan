using Pellychan.GUI.Widgets;

namespace Pellychan.GUI.Layouts;

public class HBoxLayout : Layout
{
    public int Spacing { get; set; } = 0;

    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    public VerticalAlignment Align { get; set; } = VerticalAlignment.Top;

    public override void PerformLayout(Widget parent)
    {
        var visibleChildren = parent.Children.Where(c => c.Visible).ToList();

        if (visibleChildren.Count == 0)
            return;

        var totalFixedWidth = 0;
        var expandCount = 0;

        // First pass: calculate total fixed height
        foreach (var child in visibleChildren)
        {
            switch (child.SizePolicy.Horizontal)
            {
                case SizePolicy.Policy.Fixed:
                case SizePolicy.Policy.Minimum:
                case SizePolicy.Policy.Preferred:
                    totalFixedWidth += child.Width;
                    break;
                case SizePolicy.Policy.Maximum:
                case SizePolicy.Policy.Ignored:
                    totalFixedWidth += child.MinimumWidth;
                    break;
                case SizePolicy.Policy.Expanding:
                case SizePolicy.Policy.MinimumExpanding:
                    totalFixedWidth += child.MinimumWidth;
                    expandCount++;
                    break;
            }
        }

        totalFixedWidth += Spacing * (visibleChildren.Count - 1);

        var extraHeight = Math.Max(0, parent.Width - totalFixedWidth - Padding.Horizontal);
        var x = Padding.Left;

        // Second pass: layout
        foreach (var child in visibleChildren)
        {
            var finalWidth = child.Width;

            switch (child.SizePolicy.Horizontal)
            {
                case SizePolicy.Policy.Fixed:
                    finalWidth = child.Width;
                    break;
                case SizePolicy.Policy.Minimum:
                case SizePolicy.Policy.Maximum:
                case SizePolicy.Policy.Ignored:
                    finalWidth = child.MinimumWidth;
                    break;
                case SizePolicy.Policy.Preferred:
                    finalWidth = child.SizeHint.Width;
                    break;
                case SizePolicy.Policy.Expanding:
                case SizePolicy.Policy.MinimumExpanding:
                    finalWidth = child.MinimumWidth + (expandCount > 0 ? extraHeight / expandCount : 0);
                    break;
            }

            // Determine vertical placement
            var finalHeight = child.Height;
            var vPolicy = child.SizePolicy.Vertical;

            if (vPolicy == SizePolicy.Policy.Expanding ||
                vPolicy == SizePolicy.Policy.MinimumExpanding ||
                vPolicy == SizePolicy.Policy.Ignored)
            {
                finalHeight = parent.Height - Padding.Vertical;
            }

            var y = Padding.Top + (Align switch
            {
                VerticalAlignment.Center => (parent.Height - Padding.Vertical - finalHeight) / 2,
                VerticalAlignment.Bottom => (parent.Height - Padding.Bottom - finalHeight),
                _ => 0
            });

            child.SetPosition(x, y);
            child.Resize(finalWidth, finalHeight);

            x += finalWidth + Spacing;
        }
    }
}