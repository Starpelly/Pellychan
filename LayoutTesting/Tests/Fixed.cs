using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace LayoutTesting.Tests
{
    class Fixed : MainWindow, IMouseDownHandler, IMouseUpHandler, IMouseMoveHandler
    {
        private Widget parentWidget;
        private bool m_resizing = false;

        public Fixed()
        {
            parentWidget = new Rect(SKColors.DarkBlue, this)
            {
                X = 16,
                Y = 16,

                Width = 1600,
                Height = 600,
                SizePolicy = new(SizePolicy.Policy.Fixed, SizePolicy.Policy.Fixed),

                Layout = new HBoxLayout
                {
                    Padding = new(32),
                    Spacing = 32
                }
            };

            new Rect(SKColors.Pink, parentWidget)
            {
                SizePolicy = new(SizePolicy.Policy.Fixed, SizePolicy.Policy.Fixed),
                Width = 300,
                Height = 300,
            };

            new Rect(SKColors.Yellow, parentWidget)
            {
                SizePolicy = new(SizePolicy.Policy.Preferred, SizePolicy.Policy.Preferred),
                //Width = 350,
                Height = 200
            };

            new Rect(SKColors.LightBlue, parentWidget)
            {
                SizePolicy = new(SizePolicy.Policy.Preferred, SizePolicy.Policy.Fixed),
               // Width = 350,
                Height = 200
            };
        }

        public void OnMouseDown(int x, int y)
        {
            /*
            new Rect(SKColors.Blue, parentWidget)
            {
                SizePolicy = SizePolicy.FixedPolicy,
                Width = 350,
                Height = 200
            };
            */

            m_resizing = true;
        }

        public void OnMouseUp(int x, int y)
        {
            m_resizing = false;
        }

        public void OnMouseMove(int x, int y)
        {
            if (m_resizing)
            {
                parentWidget.Resize(x - parentWidget.X, y - parentWidget.Y);
            }
        }
    }
}
