using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace LayoutTesting.Tests
{
    internal class ScrollViewer : MainWindow, IMouseMoveHandler, IMouseDownHandler, IMouseUpHandler
    {
        private ScrollArea parentWidget;
        private bool m_resizing = false;

        public ScrollViewer()
        {
            Layout = new VBoxLayout
            {
                Padding = new(32)
            };

            parentWidget = new ScrollArea(this)
            {
                Fitting = FitPolicy.ExpandingPolicy,
            };
            // parentWidget.SetWidget();

            parentWidget.ContentFrame.Layout = new HBoxLayout
            {
                // Padding = new(32)
            };

            var child = parentWidget.ChildWidget = new Rect(SKColors.Yellow, parentWidget.ContentFrame)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                Sizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
                // Width = 300,

                Layout = new VBoxLayout
                {
                    Padding = new(8),
                    Spacing = 8
                }
            };

            for (var i = 0; i < 20; i++)
            {
                new Rect(SKColors.Red, child)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    Height = 32
                };
            }

            /*
            parentWidget.ContentHolder = new Rect(SKColors.Red, parentWidget.ContentHolder)
            {
                Fitting = FitPolicy.ExpandingPolicy
            };
            */
        }

        public void OnMouseMove(int x, int y)
        {
            if (m_resizing)
            {
                parentWidget.Resize(x - parentWidget.X, y - parentWidget.Y);
                // parentWidget.Resize(parentWidget.Width, y - parentWidget.Y);
            }
        }

        public void OnMouseDown(int x, int y)
        {
            m_resizing = true;

            parentWidget.Resize(x - parentWidget.X, y - parentWidget.Y);

            return;
            new Rect(SKColors.Red, parentWidget.ChildWidget)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                Height = 32
            };
        }

        public void OnMouseUp(int x, int y)
        {
            m_resizing = false;
        }
    }
}
