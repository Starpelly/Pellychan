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

            // vertical();
            verticalFitToChildren();
        }

        private void verticalFitToChildren()
        {
            var rounding = 8;

            var parentWidget = new Rect(SKColors.DarkBlue, this)
            {
                X = 16,
                Y = 16,

                Width = 960,
                Height = 540,

                Fitting = FitPolicy.FixedPolicy,
                Sizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),

                Layout = new VBoxLayout
                {
                    Padding = new(32),
                    Spacing = 8
                },

                Rounding = rounding,
            };

            void createChild(SKColor color)
            {
                new Rect(color, parentWidget)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),

                    // MaximumWidth = 300,
                    Width = 0,
                    Height = 50,

                    Rounding = rounding
                };
            }

            for (int i = 0; i < 3; i++)
            {
                createChild(SKColors.Pink);
                createChild(SKColors.LightYellow);
                createChild(SKColors.LightBlue);
            }
        }

        private void vertical()
        {
            var rounding = 8;

            parentWidget = new Rect(SKColors.DarkBlue, this)
            {
                X = 16,
                Y = 16,

                Width = 960,
                Height = 540,

                Fitting = FitPolicy.ExpandingPolicy,
                Sizing = SizePolicy.FixedPolicy,

                Layout = new VBoxLayout
                {
                    Padding = new(32),
                    Spacing = 8
                },

                Rounding = rounding,
            };

            new Rect(SKColors.Pink, parentWidget)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),

                // MaximumWidth = 300,
                Width = 0,
                Height = 50,

                Rounding = rounding
            };

            new Rect(SKColors.LightYellow, parentWidget)
            {
                Fitting = FitPolicy.ExpandingPolicy,

                Width = 200,
                Height = 0,

                Rounding = rounding
            };

            new Rect(SKColors.LightBlue, parentWidget)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),

                Width = 0,
                Height = 50,

                Rounding = rounding
            };
        }

        private void horizontal()
        {
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

            new Rect(SKColors.LightYellow, parentWidget)
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
