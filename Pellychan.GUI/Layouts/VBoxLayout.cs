using Pellychan.GUI.Widgets;
using SkiaSharp;
using System.Drawing;

namespace Pellychan.GUI.Layouts;

public class VBoxLayout : Layout
{
    public int Spacing { get; set; } = 0;

    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    public HorizontalAlignment Align { get; set; } = HorizontalAlignment.Left;

    public override void PerformLayout(Widget parent)
    {
        var visibleChildren = parent.Children.Where(c => c.Visible).ToList();

        if (visibleChildren.Count == 0)
            return;

        var totalFixedHeight = 0;
        var expandCount = 0;

        // First pass: calculate total fixed height
        foreach (var child in visibleChildren)
        {
            switch (child.SizePolicy.Vertical)
            {
                case SizePolicy.Policy.Fixed:
                case SizePolicy.Policy.Minimum:
                case SizePolicy.Policy.Preferred:
                    totalFixedHeight += child.Height;
                    break;
                case SizePolicy.Policy.Maximum:
                case SizePolicy.Policy.Ignored:
                    totalFixedHeight += child.MinimumHeight;
                    break;
                case SizePolicy.Policy.Expanding:
                case SizePolicy.Policy.MinimumExpanding:
                    totalFixedHeight += child.MinimumHeight;
                    expandCount++;
                    break;
            }
        }

        totalFixedHeight += Spacing * (visibleChildren.Count - 1);

        var extraHeight = Math.Max(0, parent.Height - totalFixedHeight - Padding.Vertical);
        var y = Padding.Top;

        // Second pass: layout
        foreach (var child in visibleChildren)
        {
            var finalHeight = child.Height;

            switch (child.SizePolicy.Vertical)
            {
                case SizePolicy.Policy.Fixed:
                    finalHeight = child.Height;
                    break;
                case SizePolicy.Policy.Minimum:
                case SizePolicy.Policy.Maximum:
                case SizePolicy.Policy.Ignored:
                    finalHeight = child.MinimumHeight;
                    break;
                case SizePolicy.Policy.Preferred:
                    finalHeight = child.SizeHint.Height;
                    break;
                case SizePolicy.Policy.Expanding:
                case SizePolicy.Policy.MinimumExpanding:
                    finalHeight = child.MinimumHeight + (expandCount > 0 ? extraHeight / expandCount : 0);
                    break;
            }

            // Determine horizontal placement
            var finalWidth = child.Width;
            var hPolicy = child.SizePolicy.Horizontal;

            if (hPolicy == SizePolicy.Policy.Expanding ||
                hPolicy == SizePolicy.Policy.MinimumExpanding ||
                hPolicy == SizePolicy.Policy.Ignored)
            {
                finalWidth = parent.Width - Padding.Horizontal;
            }

            var x = Padding.Left + (Align switch
            {
                HorizontalAlignment.Center => (parent.Width - Padding.Horizontal - finalWidth) / 2,
                HorizontalAlignment.Right => (parent.Width - Padding.Right - finalWidth),
                _ => 0
            });

            child.SetPosition(x, y);
            child.Resize(finalWidth, finalHeight);

            y += finalHeight + Spacing;
        }
    }

    public override SKSizeI SizeHint(Widget parent)
    {
        var visibleChildren = parent.Children.Where(c => c.Visible).ToList();

        int width = 0;
        int height = Padding.Top + Padding.Bottom + Spacing * (visibleChildren.Count - 1);

        foreach (var child in visibleChildren)
        {
            var hint = child.SizeHint;
            width = Math.Max(width, hint.Width);
            height += hint.Height;
        }

        width += Padding.Left + Padding.Right;

        return new(width, height);
    }
}