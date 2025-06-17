using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace LayoutTesting.Tests
{
    internal class VerticalList : MainWindow
    {
        public VerticalList()
        {
            var container = new Rect(SKColors.Red, this)
            {
                X = 16,
                Y = 16,

                Width = 400,
                Height = 400,

                Fitting = FitPolicy.FixedPolicy,
                Layout = new HBoxLayout
                {
                    Padding = new(16),
                    Spacing = 8
                }
            };

            var list = new Rect(SKColors.Black, container)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Preferred),
                Layout = new VBoxLayout
                {
                    Padding = new(8),
                    Spacing = 8
                },
            };

            for (var i = 0; i < 12; i++)
            {
                new Rect(SKColors.White, list)
                {
                    Height = 32,
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                };
            }

            var scrollbar = new ScrollBar(container)
            {
                Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                Width = 16,
            };
            scrollbar.Minimum = 0;
            scrollbar.OnValueChanged += (value) =>
            {
                scrollbar.Maximum = list.Height - container.Height;
                list.Y = -value;
            };
        }
    }
}
