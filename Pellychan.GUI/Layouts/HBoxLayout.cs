using Pellychan.GUI.Widgets;
using SkiaSharp;
using System.Drawing;

namespace Pellychan.GUI.Layouts;

public class HBoxLayout : Layout
{
    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    public VerticalAlignment Align { get; set; } = VerticalAlignment.Top;

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

    public override void FitSizingPass(Widget widget)
    {
        var visibleChildren = widget.Children.Where(c => c.Visible).Reverse().ToList();
        if (visibleChildren.Count == 0)
            return;

        bool fitHorizontal = widget.Sizing.Horizontal == SizePolicy.Policy.Fit;
        bool fitVertical = widget.Sizing.Vertical == SizePolicy.Policy.Fit;

        if (!fitHorizontal && !fitVertical)
            return;

        if (fitHorizontal)
        {
            widget.Width = 0;
        }
        if (fitVertical)
        {
            widget.Height = 0;
        }

        var childGap = (widget.Children.Count - 1) * Spacing;

        foreach (var child in visibleChildren)
        {
            if (fitHorizontal)
                widget.Width += child.Width;

            if (widget.Sizing.Vertical == SizePolicy.Policy.Fit)
                widget.Height = Math.Max(child.Height, widget.Height);
        }

        if (fitHorizontal)
        {
            widget.Width += childGap;
            widget.Width += Padding.Left + Padding.Right;
        }
        if (fitVertical)
        {
            widget.Height += Padding.Top + Padding.Bottom;
        }
    }

    public override void GrowSizingPass(Widget parent)
    {
        var visibleChildren = parent.Children.Where(c => c.Visible).ToList();
        if (visibleChildren.Count == 0)
            return;

        float remainingWidth = parent.Width;
        float remainingHeight = parent.Height;

        remainingWidth -= Padding.Left + Padding.Right;
        remainingHeight -= Padding.Top + Padding.Bottom;

        foreach (var child in visibleChildren)
        {
            // @Investigate
            // This is sus...
            if (child.Fitting.Horizontal != FitPolicy.Policy.Fixed)
                child.Width = 0;

            remainingWidth -= child.Width;
        }
        remainingWidth -= (visibleChildren.Count - 1) * Spacing;

        var growables = visibleChildren.Where(c => c.Fitting.Horizontal != FitPolicy.Policy.Fixed).ToList();
        var shrinkables = growables.ToList();

        while (remainingWidth > 0 && growables.Count > 0) // Grow elements
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

            // This sucks
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
                        childWidthF = child.MaximumWidth;
                        growables.Remove(child);
                    }
                    remainingWidth -= (childWidthF - previousWidth);
                }
            }

            remainingWidth = MathF.Round(remainingWidth);
        }

        remainingWidth = MathF.Round(remainingWidth);

        while (remainingWidth < 0 && shrinkables.Count > 0) // Shrink elements
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
                        childWidthF = child.MinimumWidth;
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
            switch (child.Fitting.Vertical)
            {
                case FitPolicy.Policy.Minimum:
                case FitPolicy.Policy.Maximum:
                case FitPolicy.Policy.Preferred:
                case FitPolicy.Policy.Expanding:
                    child.Height += ((int)remainingHeight - child.Height);
                    child.Height = Math.Clamp(child.Height, child.MinimumHeight, child.MaximumHeight);
                    break;
            }
        }
    }

    public override void PositionsPass(Widget parent)
    {
        var visibleChildren = parent.Children.Where(c => c.Visible).ToList();

        if (visibleChildren.Count == 0)
            return;

        var x = Padding.Left;

        foreach (var child in visibleChildren)
        {
            var finalWidth = child.Width;

            // Determine vertical placement
            var finalHeight = child.Height;
            var vPolicy = child.Fitting.Vertical;

            if (vPolicy == FitPolicy.Policy.Expanding ||
                vPolicy == FitPolicy.Policy.MinimumExpanding ||
                vPolicy == FitPolicy.Policy.Ignored)
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

            x += finalWidth + Spacing;
        }
    }
}