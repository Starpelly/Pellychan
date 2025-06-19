using Pellychan.GUI;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace LayoutTesting.Tests
{
    internal class Testy : MainWindow, IMouseMoveHandler, IMouseDownHandler, IMouseUpHandler
    {
        private Label testLabel;
        private bool m_resizing = false;

        public Testy()
        {
            testLabel = new Label(Application.DefaultFont, this)
            {
                Width = 400,
                Height = 10,
                WordWrap = true,
                Text = "One Two Three Four Five Six Seven Eight Nine Ten"
                // Text = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum."
            };
        }

        public void OnMouseMove(int x, int y)
        {
            if (m_resizing)
            {
                var newWidth = x - testLabel.X;
                var newHeight = testLabel.MeasureHeightFromWidth(newWidth);
                testLabel.Resize(newWidth, newHeight);
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
