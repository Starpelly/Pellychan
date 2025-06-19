using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace LayoutTesting.Tests
{
    internal class Sizing : MainWindow, IMouseMoveHandler, IMouseDownHandler, IMouseUpHandler
    {
        private Widget parentWidget;
        private bool m_resizing = false;

        public Sizing()
        {
            Layout = new VBoxLayout
            {
                Padding = new(32),
            };

            var frame = new ShapedFrame(this)
            {
                Fitting = FitPolicy.ExpandingPolicy,
                Layout = new VBoxLayout
                {
                    // Padding = new(32),
                },
                ContentsMargins = new(16, 16, 1, 1)
            };

            new Rect(SKColors.Red, frame)
            {
                Fitting = FitPolicy.ExpandingPolicy
            };
        }

        public void OnMouseMove(int x, int y)
        {
            if (m_resizing)
            {
                // parentWidget.Resize(x - parentWidget.X, y - parentWidget.Y);
                // parentWidget.Resize(parentWidget.Width, y - parentWidget.Y);
            }
        }

        public void OnMouseDown(int x, int y)
        {
            m_resizing = true;
        }

        public void OnMouseUp(int x, int y)
        {
            m_resizing = false;
        }
    }
}
