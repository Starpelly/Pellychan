using ExCSS;
using Pellychan.GUI.Widgets;
using SkiaSharp;
using System.Drawing;

namespace Pellychan.GUI.Layouts;

public class VBoxLayout : Layout
{
    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    public HorizontalAlignment Align { get; set; } = HorizontalAlignment.Left;

    public override SKSizeI SizeHint(Widget parent)
    {
        var visibleChildren = parent.Children.Where(c => c.Visible).ToList();

        var finalPadding = GetFinalPadding(parent);

        int width = 0;
        int height = finalPadding.Top + finalPadding.Bottom + Spacing * (visibleChildren.Count - 1);

        foreach (var child in visibleChildren)
        {
            var hint = child.SizeHint;
            width = Math.Max(width, hint.Width);
            height += hint.Height;
        }

        width += finalPadding.Left + finalPadding.Right;

        return new(width, height);
    }

    public override void FitSizingPass(Widget widget)
    {
        bool fitHorizontal = widget.Sizing.Horizontal == SizePolicy.Policy.Fit;
        bool fitVertical = widget.Sizing.Vertical == SizePolicy.Policy.Fit;

        if (!fitHorizontal && !fitVertical)
            return;

        var visibleChildren = widget.Children.Where(c => c.Visible).Reverse().ToList();
        if (visibleChildren.Count == 0)
            return;

        var finalPadding = GetFinalPadding(widget);

        var lastParentSize = new SKSizeI(widget.Width, widget.Height);

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
            if (fitVertical)
                widget.Height += child.Height;

            if (widget.Sizing.Horizontal == SizePolicy.Policy.Fit)
                widget.Width = Math.Max(child.Width, widget.Width);
        }

        if (fitHorizontal)
        {
            widget.Width += finalPadding.Left + finalPadding.Right;
        }
        if (fitVertical)
        {
            widget.Height += childGap;
            widget.Height += finalPadding.Top + finalPadding.Bottom;
        }

        if (lastParentSize != new SKSizeI(widget.Width, widget.Height))
            widget.CallResizeEvents();
    }

    public override void GrowSizingPass(Widget parent)
    {
        var visibleChildren = parent.Children.Where(c => c.Visible).ToList();
        if (visibleChildren.Count == 0)
            return;

        var finalPadding = GetFinalPadding(parent);

        // Collect sizes for actually sending resize events
        var lastChildrenSizes = visibleChildren.Select(c => new SKSizeI(c.Width, c.Height)).ToList();

        float remainingWidth = parent.Width;
        float remainingHeight = parent.Height;

        remainingWidth -= finalPadding.Left + finalPadding.Right;
        remainingHeight -= finalPadding.Top + finalPadding.Bottom;

        foreach (var child in visibleChildren)
        {
            // @Investigate
            // This is sus...
            if (child.Fitting.Vertical != FitPolicy.Policy.Fixed)
                child.Height = 0;

            remainingHeight -= child.Height;
        }
        remainingHeight -= (visibleChildren.Count - 1) * Spacing;

        var growables = visibleChildren.Where(c => c.Fitting.Vertical != FitPolicy.Policy.Fixed).ToList();
        var shrinkables = growables.ToList();

        while (remainingHeight > 0 && growables.Count > 0) // Grow elements
        {
            float smallest = growables[0].Height;
            float secondSmallest = float.PositiveInfinity;
            float heightToAdd = remainingHeight;
            foreach (var child in growables)
            {
                if (child.Height < smallest)
                {
                    secondSmallest = smallest;
                    smallest = child.Height;
                }
                if (child.Height > smallest)
                {
                    secondSmallest = Math.Min(secondSmallest, child.Height);
                    heightToAdd = (int)(secondSmallest - smallest);
                }
            }

            heightToAdd = Math.Min(heightToAdd, (float)remainingHeight / growables.Count);

            // This sucks
            foreach (var child in shrinkables)
            {
                float previousHeight = child.Height;
                float childHeightF = child.Height;

                if (child.Height == smallest)
                {
                    child.Height += (int)heightToAdd;
                    childHeightF += heightToAdd;

                    if (childHeightF >= child.MaximumHeight)
                    {
                        child.Height = child.MaximumHeight;
                        childHeightF = child.MaximumHeight;
                        growables.Remove(child);
                    }
                    remainingHeight -= (childHeightF - previousHeight);
                }
            }

            remainingHeight = MathF.Round(remainingHeight);
        }

        remainingHeight = MathF.Round(remainingHeight);

        while (remainingHeight < 0 && shrinkables.Count > 0) // Shrink elements
        {
            float largest = shrinkables[0].Height;
            float secondLargest = 0;
            float heightToAdd = remainingHeight;
            foreach (var child in shrinkables)
            {
                if (child.Height > largest)
                {
                    secondLargest = largest;
                    largest = child.Height;
                }
                if (child.Height < largest)
                {
                    secondLargest = Math.Max(secondLargest, child.Height);
                    heightToAdd = (int)(secondLargest - largest);
                }
            }

            heightToAdd = Math.Max(heightToAdd, (float)remainingHeight / shrinkables.Count);

            // This sucks
            foreach (var child in growables)
            {
                float previousHeight = child.Height;
                float childHeightF = child.Height;

                if (child.Height == largest)
                {
                    child.Height += (int)heightToAdd;
                    childHeightF += heightToAdd;

                    if (childHeightF <= child.MinimumHeight)
                    {
                        child.Height = child.MinimumHeight;
                        childHeightF = child.MinimumHeight;
                        shrinkables.Remove(child);
                    }
                    remainingHeight -= (childHeightF - previousHeight);
                }
            }

            // Idk how to feel about this hmmmmm
            remainingHeight = MathF.Round(remainingHeight);
        }

        foreach (var child in visibleChildren)
        {
            switch (child.Fitting.Horizontal)
            {
                case FitPolicy.Policy.Minimum:
                case FitPolicy.Policy.Maximum:
                case FitPolicy.Policy.Preferred:
                case FitPolicy.Policy.Expanding:
                    child.Width += ((int)remainingWidth - child.Width);
                    child.Width = Math.Clamp(child.Width, child.MinimumWidth, child.MaximumWidth);
                    break;
            }
        }

        // Send resize events
        {
            for (var i = 0; i < visibleChildren.Count; i++)
            {
                if (lastChildrenSizes[i] != new SKSizeI(visibleChildren[i].Width, visibleChildren[i].Height))
                    visibleChildren[i].CallResizeEvents();
            }
        }
    }

    public override void PositionsPass(Widget parent)
    {
        var visibleChildren = parent.Children.Where(c => c.Visible).ToList();

        if (visibleChildren.Count == 0)
            return;

        var finalPadding = GetFinalPadding(parent);
        var y = finalPadding.Top;

        foreach (var child in visibleChildren)
        {
            var finalHeight = child.Height;

            // Determine vertical placement
            var finalWidth = child.Width;
            var hPolicy = child.Fitting.Horizontal;

            if (hPolicy == FitPolicy.Policy.Expanding ||
                hPolicy == FitPolicy.Policy.MinimumExpanding ||
                hPolicy == FitPolicy.Policy.Ignored)
            {
                finalWidth = parent.Width - finalPadding.Horizontal;
            }

            var x = finalPadding.Left + (Align switch
            {
                HorizontalAlignment.Center => (parent.Width - finalPadding.Horizontal - finalWidth) / 2,
                HorizontalAlignment.Right => (parent.Width - finalPadding.Right - finalWidth),
                _ => 0
            });

            child.SetPosition(x, y);

            y += finalHeight + Spacing;
        }
    }
}