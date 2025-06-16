using Pellychan.GUI.Widgets;
using SkiaSharp;
using System.Drawing;

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
        int extraHeight = 0;
        var x = Padding.Left;

        // First pass: calculate total fixed width
        {
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

            extraHeight = Math.Max(0, parent.Width - totalFixedWidth - Padding.Horizontal);

            /*
            foreach (var child in visibleChildren)
            {
                if (parent.SizePolicy.Vertical == SizePolicy.Policy.Preferred)
                {
                    // parent.Width += child.Width;
                    parent.Height = Math.Max(child.Height, parent.Height);
                }
            }
            */
            var sizeHint = SizeHint(parent);
            var newWidth = parent.Width;
            var newHeight = parent.Height;

            if (parent.SizePolicy.Horizontal == SizePolicy.Policy.Preferred)
                newWidth = sizeHint.Width;
            if (parent.SizePolicy.Vertical == SizePolicy.Policy.Preferred)
                newHeight = sizeHint.Height;

            parent.Resize(newWidth, newHeight);
        }


        // Second pass: calculate grow sizings for children
        {
            // @NOTE - pelly
            // Needs to be floats because of division bullshit...
            // I should better investigate!
            float remainingWidth = parent.Width;
            float remainingHeight = parent.Height;

            remainingWidth -= Padding.Left + Padding.Right;
            remainingHeight -= Padding.Top + Padding.Top;

            foreach (var child in visibleChildren)
            {
                remainingWidth -= child.Width;
            }
            remainingWidth -= (visibleChildren.Count - 1) * Spacing;

            var growables = visibleChildren.Where(c => c.SizePolicy.Horizontal == SizePolicy.Policy.Preferred).ToList();
            var shrinkables = growables.ToList();

            while (remainingWidth > 0) // Grow elements
            {
                float smallest = growables[0].Width;
                float secondSmallest = float.PositiveInfinity;
                float widthToAdd = remainingWidth;
                foreach (var child in growables)
                {
                    if (child.Width < smallest)
                    {
                        secondSmallest = smallest;
                        smallest = child.Width;
                    }
                    if (child.Width > smallest)
                    {
                        secondSmallest = Math.Min(secondSmallest, child.Width);
                        widthToAdd = (int)(secondSmallest - smallest);
                    }
                }

                widthToAdd = Math.Min(widthToAdd, (float)remainingWidth / growables.Count);

                foreach (var child in growables)
                {
                    if (child.Width == smallest)
                    {
                        child.Width += (int)widthToAdd;
                        remainingWidth -= widthToAdd;
                    }
                }

                // This sucks
                /*
                foreach (var child in shrinkables)
                {
                    float previousWidth = child.Width;
                    float childWidthF = child.Width;

                    if (child.Width == smallest)
                    {
                        child.Width += (int)widthToAdd;
                        childWidthF += widthToAdd;

                        if (childWidthF >= child.MaximumWidth)
                        {
                            child.Width = child.MaximumWidth;
                            growables.Remove(child);
                        }
                        remainingWidth -= (childWidthF - previousWidth);
                    }
                }
                */

                remainingWidth = MathF.Round(remainingWidth);
            }

            remainingWidth = MathF.Round(remainingWidth);

            while (remainingWidth < 0) // Shrink elements
            {
                float largest = shrinkables[0].Width;
                float secondLargest = 0;
                float widthToAdd = remainingWidth;
                foreach (var child in shrinkables)
                {
                    if (child.Width > largest)
                    {
                        secondLargest = largest;
                        largest = child.Width;
                    }
                    if (child.Width < largest)
                    {
                        secondLargest = Math.Max(secondLargest, child.Width);
                        widthToAdd = (int)(secondLargest - largest);
                    }
                }

                widthToAdd = Math.Max(widthToAdd, (float)remainingWidth / shrinkables.Count);

                // This sucks
                foreach (var child in growables)
                {
                    float previousWidth = child.Width;
                    float childWidthF = child.Width;

                    if (child.Width == largest)
                    {
                        child.Width += (int)widthToAdd;
                        childWidthF += widthToAdd;

                        if (childWidthF <= child.MinimumWidth)
                        {
                            child.Width = child.MinimumWidth;
                            shrinkables.Remove(child);
                        }
                        remainingWidth -= (childWidthF - previousWidth);
                    }
                }

                // Idk how to feel about this hmmmmm
                remainingWidth = MathF.Round(remainingWidth);
            }


            foreach (var child in visibleChildren)
            {
                switch (child.SizePolicy.Vertical)
                {
                    case SizePolicy.Policy.Minimum:
                    case SizePolicy.Policy.Maximum:
                    case SizePolicy.Policy.Preferred:
                        child.Height += ((int)remainingHeight - child.Height);
                        child.Height = Math.Clamp(child.Height, child.MinimumHeight, child.MaximumHeight);
                        break;
                }
            }
        }

        // Third pass: layout positions
        {
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

    public override SKSizeI SizeHint(Widget parent)
    {
        var visibleChildren = parent.Children.Where(c => c.Visible).ToList();

        if (visibleChildren.Count == 0)
            return new(Padding.Left + Padding.Right, Padding.Top + Padding.Bottom);

        int totalWidth = Padding.Left + Padding.Right + Spacing * (visibleChildren.Count - 1);
        int maxHeight = 0;

        foreach (var child in visibleChildren)
        {
            var hint = child.SizeHint;
            totalWidth += hint.Width;
            maxHeight = Math.Max(maxHeight, hint.Height);
        }

        int totalHeight = Padding.Top + Padding.Bottom + maxHeight;

        return new(totalWidth, totalHeight);
    }
}