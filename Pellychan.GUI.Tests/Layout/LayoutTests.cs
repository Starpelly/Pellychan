using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;

namespace Pellychan.GUI.Tests.Layout;

[TestFixture]
public partial class LayoutTests
{
    [SetUp]
    public void SetUp()
    {
        Config.HeadlessMode = true;
    }

    [TearDown]
    public void TearDown()
    {
        Config.HeadlessMode = false;
    }

    #region All Layouts

    [TestCase(1, 1, 1, 1)]
    [TestCase(2, 1, 2, 1)]
    [TestCase(16, 16, 0, 0)]
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

        Assert.That(child.X, Is.EqualTo(parent.ContentsMargins.Left));
        Assert.That(child.Y, Is.EqualTo(parent.ContentsMargins.Top));
        Assert.That(child.Width, Is.EqualTo(300 - parent.ContentsMargins.Right - parent.ContentsMargins.Left));
        Assert.That(child.Height, Is.EqualTo(200 - parent.ContentsMargins.Bottom - parent.ContentsMargins.Top)  );
    }

    [TestCase(1, 1, 1, 1)]
    [TestCase(2, 1, 2, 1)]
    [TestCase(16, 16, 0, 0)]
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

        Assert.That(child.X, Is.EqualTo(parent.ContentsMargins.Left));
        Assert.That(child.Y, Is.EqualTo(parent.ContentsMargins.Top));
        Assert.That(child.Width, Is.EqualTo(300 - parent.ContentsMargins.Right - parent.ContentsMargins.Left));
        Assert.That(child.Height, Is.EqualTo(200 - parent.ContentsMargins.Bottom - parent.ContentsMargins.Top));
    }

    #endregion
}