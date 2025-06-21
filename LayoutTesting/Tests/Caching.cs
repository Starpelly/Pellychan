using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace LayoutTesting.Tests
{
    internal class Caching : MainWindow, IMouseMoveHandler, IMouseDownHandler, IMouseUpHandler
    {
        private Rect parentWidget;
        private bool m_resizing = false;

        public Caching()
        {
            parentWidget = new Rect(SKColors.Yellow, this)
            {
                X = 16,
                Y = 16,
                Width = 300,
                Height = 300,
            };

            new Rect(SKColors.AliceBlue, this)
            {
                X = 16,
                Y = 400,
                Width = 300,
                Height = 300
            };

            new PushButton("Test", this)
            {
                X = 400,
                Y = 24,
                Width = 400,
                Height = 200
            };
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
        }

        public void OnMouseUp(int x, int y)
        {
            m_resizing = false;
        }
    }
}
