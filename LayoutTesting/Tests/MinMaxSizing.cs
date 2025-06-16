using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace LayoutTesting.Tests
{
    internal class MinMaxSizing : MainWindow, IMouseDownHandler, IMouseUpHandler, IMouseMoveHandler
    {
        private Widget parentWidget;
        private bool m_resizing = false;

        public MinMaxSizing()
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
                    Spacing = 4
                }
            };

            var rounding = 8;

            new Rect(SKColors.Pink, parentWidget)
            {
                SizePolicy = new(SizePolicy.Policy.Preferred, SizePolicy.Policy.Preferred),
                Height = 300,

                Rounding = rounding
            };

            new Rect(SKColors.Yellow, parentWidget)
            {
                SizePolicy = new(SizePolicy.Policy.Preferred, SizePolicy.Policy.Preferred),
                
                MaximumWidth = 300,
                //Width = 350,
                Height = 200,

                Rounding = rounding
            };

            new Rect(SKColors.LightBlue, parentWidget)
            {
                SizePolicy = new(SizePolicy.Policy.Preferred, SizePolicy.Policy.Preferred),
                // Width = 350,
                Height = 200,

                Rounding = rounding
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
