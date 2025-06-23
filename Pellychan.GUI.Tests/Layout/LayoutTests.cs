using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;

namespace Pellychan.GUI.Tests.Layout;

public class LayoutTests : IDisposable
{
    public LayoutTests()
    {
        Application.HeadlessMode = true;
    }

    public void Dispose()
    {
        Application.HeadlessMode = false;
    }

    #region HBoxLayout

    [Fact]
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

        Assert.Equal(100, child.Width);
        Assert.Equal(50, child.Height);
    }

    [Fact]
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

        Assert.Equal(0, a.X);
        Assert.Equal(100, b.X);
        Assert.Equal(0, a.Y);
        Assert.Equal(0, b.Y);
    }

    [Fact]
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

        Assert.Equal(0, child.X); // The position changes because layouts still move widgets around
        Assert.Equal(0, child.Y);
        Assert.Equal(300, child.Width);
        Assert.Equal(50, child.Height);
    }

    [Fact]
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

        Assert.Equal(0, child.X); // The position changes because layouts still move widgets around
        Assert.Equal(0, child.Y);
        Assert.Equal(100, child.Width);
        Assert.Equal(200, child.Height);
    }

    [Fact]
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

        Assert.Equal(0, a.X);
        Assert.Equal(95, a.Width);
        Assert.Equal(105, b.X);
        Assert.Equal(95, b.Width);
    }

    #endregion

    #region All Layouts

    [Theory]
    [InlineData(1, 1, 1, 1)]
    [InlineData(2, 1, 2, 1)]
    [InlineData(16, 16, 0, 0)]
    public void HBoxLayout_Respects_ContentsMargins(int l, int t, int r, int b)
    {
        var parent = new Widget
        {
            Width = 300,
            Height = 200,

            ContentsMargins = new Margins(l, t, r, b),

            Layout = new HBoxLayout
            {
            }
        };
        var child = new Widget(parent)
        {
            Fitting = FitPolicy.ExpandingPolicy
        };
        parent.PerformLayoutUpdate(LayoutFlushType.All);

        Assert.Equal(parent.ContentsMargins.Left, child.X);
        Assert.Equal(parent.ContentsMargins.Top, child.Y);
        Assert.Equal(300 - parent.ContentsMargins.Right - parent.ContentsMargins.Left, child.Width);
        Assert.Equal(200 - parent.ContentsMargins.Bottom - parent.ContentsMargins.Top, child.Height);
    }

    [Theory]
    [InlineData(1, 1, 1, 1)]
    [InlineData(2, 1, 2, 1)]
    [InlineData(16, 16, 0, 0)]
    public void VBoxLayout_Respects_ContentsMargins(int l, int t, int r, int b)
    {
        var parent = new Widget
        {
            Width = 300,
            Height = 200,

            ContentsMargins = new Margins(l, t, r, b),

            Layout = new VBoxLayout
            {
            }
        };
        var child = new Widget(parent)
        {
            Fitting = FitPolicy.ExpandingPolicy
        };
        parent.PerformLayoutUpdate(LayoutFlushType.All);

        Assert.Equal(parent.ContentsMargins.Left, child.X);
        Assert.Equal(parent.ContentsMargins.Top, child.Y);
        Assert.Equal(300 - parent.ContentsMargins.Right - parent.ContentsMargins.Left, child.Width);
        Assert.Equal(200 - parent.ContentsMargins.Bottom - parent.ContentsMargins.Top, child.Height);
    }

    #endregion
}