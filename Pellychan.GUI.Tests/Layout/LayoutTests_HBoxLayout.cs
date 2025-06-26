using ExCSS;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;

namespace Pellychan.GUI.Tests.Layout;

public partial class LayoutTests
{
    [Test]
    public void HBoxLayout_FixedWidgets_HaveExactSize()
    {
        var parent = new Widget
        {
            Width = 300,
            Height = 200,

            Layout = new HBoxLayout
            {
            }
        };
        var child = new Widget(parent)
        {
            Width = 100,
            Height = 50,

            Fitting = FitPolicy.FixedPolicy
        };
        parent.PerformLayoutUpdate(LayoutFlushType.All);

        Assert.That(child.Width, Is.EqualTo(100));
        Assert.That(child.Height, Is.EqualTo(50));
    }

    [Test]
    public void HBoxLayout_Moves_Widgets()
    {
        var parent = new Widget
        {
            Width = 200,
            Height = 200,

            Layout = new HBoxLayout
            {
            }
        };
        var a = new Widget(parent)
        {
            X = 16,
            Y = 32,
            Width = 100,
            Height = 100,

            Fitting = FitPolicy.FixedPolicy
        };
        var b = new Widget(parent)
        {
            X = 16,
            Y = 32,
            Width = 100,
            Height = 100,

            Fitting = FitPolicy.FixedPolicy
        };
        parent.PerformLayoutUpdate(LayoutFlushType.All);

        Assert.That(a.X, Is.EqualTo(0));
        Assert.That(b.X, Is.EqualTo(100));
        Assert.That(a.Y, Is.EqualTo(0));
        Assert.That(b.Y, Is.EqualTo(0));
    }

    [Test]
    public void HBoxLayout_Expands_Horizontal()
    {
        var parent = new Widget
        {
            Width = 300,
            Height = 200,

            Layout = new HBoxLayout
            {
            }
        };
        var child = new Widget(parent)
        {
            X = 16,
            Y = 32,
            Width = 100,
            Height = 50,

            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
        };
        parent.PerformLayoutUpdate(LayoutFlushType.All);

        Assert.That(child.X, Is.EqualTo(0)); // The position changes because layouts still move widgets around
        Assert.That(child.Y, Is.EqualTo(0));
        Assert.That(child.Width, Is.EqualTo(300));
        Assert.That(child.Height, Is.EqualTo(50));
    }

    [Test]
    public void HBoxLayout_Expands_Vertical()
    {
        var parent = new Widget
        {
            Width = 300,
            Height = 200,

            Layout = new HBoxLayout
            {
            }
        };
        var child = new Widget(parent)
        {
            X = 16,
            Y = 32,
            Width = 100,
            Height = 50,

            Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding)
        };
        parent.PerformLayoutUpdate(LayoutFlushType.All);

        Assert.That(child.X, Is.EqualTo(0)); // The position changes because layouts still move widgets around
        Assert.That(child.Y, Is.EqualTo(0));
        Assert.That(child.Width, Is.EqualTo(100));
        Assert.That(child.Height, Is.EqualTo(200));
    }

    [Test]
    public void HBoxLayout_RespectsSpacing()
    {
        var parent = new Widget
        {
            Width = 200,
            Height = 100,

            Layout = new HBoxLayout
            {
                Spacing = 10
            }
        };

        var a = new Widget(parent) { Fitting = FitPolicy.ExpandingPolicy };
        var b = new Widget(parent) { Fitting = FitPolicy.ExpandingPolicy };

        parent.PerformLayoutUpdate(LayoutFlushType.All);

        Assert.That(a.X, Is.EqualTo(0));
        Assert.That(a.Width, Is.EqualTo(95));
        Assert.That(b.X, Is.EqualTo(105));
        Assert.That(b.Width, Is.EqualTo(95));
    }

    [TestCase(0)]
    [TestCase(3)]
    [TestCase(16)]
    public void HBoxLayout_FitSpacing(int spacing)
    {
        int boxCount = 3;
        int boxWidth = 100;
        int boxHeight = 100;

        var parent = new Widget
        {
            Width = 1337,
            Height = 69,

            AutoSizing = new(SizePolicy.Policy.Fit, SizePolicy.Policy.Fit),

            Layout = new HBoxLayout
            {
                Spacing = spacing
            }
        };
        for (var i = 0; i < boxCount; i++)
        {
            new Widget(parent)
            {
                Width = boxWidth,
                Height = boxHeight,
            };
        }

        parent.PerformLayoutUpdate(LayoutFlushType.All);

        Assert.That(parent.Width, Is.EqualTo((boxWidth * boxCount) + (spacing * (boxCount - 1))));
        Assert.That(parent.Height, Is.EqualTo(boxHeight));
    }
}