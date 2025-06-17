using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace LayoutTesting.Tests
{
    internal class Fitting : MainWindow, IMouseMoveHandler, IMouseDownHandler, IMouseUpHandler
    {
        private Widget parentWidget;
        private bool m_resizing = false;

        public Fitting()
        {
            /*
            Layout = new HBoxLayout
            {
                Padding = new(32)
            };
            */

            var rounding = 8;

            parentWidget = new Rect(SKColors.DarkBlue, this)
            {
                X = 16,
                Y = 16,

                Width = 960,
                Height = 540,

                Fitting = FitPolicy.ExpandingPolicy,
                Sizing = SizePolicy.FixedPolicy,

                Layout = new HBoxLayout
                {
                    Padding = new(32),
                    Spacing = 32
                },

                Rounding = rounding,
            };

            new Rect(SKColors.Pink, parentWidget)
            {
                Fitting = FitPolicy.FixedPolicy,
                
                // MaximumWidth = 300,
                Width = 300,
                Height = 300,

                Rounding = rounding
            };

            var t = new Rect(SKColors.LightYellow, parentWidget)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Expanding),

                Width = 0,
                Height = 200,

                Rounding = rounding
            };

            new Rect(SKColors.LightBlue, parentWidget)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),

                Width = 350,
                Height = 200,

                Rounding = rounding
            };
        }

        public void OnMouseMove(int x, int y)
        {
            if (m_resizing)
            {
                parentWidget.Resize(x - parentWidget.X, y - parentWidget.Y);
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
