using Pellychan.GUI;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace LayoutTesting
{
    class Window : MainWindow
    {
        public Window()
        {
            // createMenubar();
            var btn = new PushButton("Test", this)
            {
                X = 16,
                Y = 16,
                Width = 200,
                Height = 200,

            };
            btn.OnClicked += createPopup;
        }

        private void createPopup()
        {
            var t = new Rect(SKColors.Red, this)
            {
                X = -16,
                Y = 16,
                Width = 100,
                Height = 100,
                Rounding = 16
            };
            t.Show();
        }

        private void createMenubar()
        {
            MenuBar = new(this)
            {
                Width = this.Width,
                ScreenPosition = MenuBar.Orientation.Top,
            };
            void AddMenu(string title, List<MenuItem> items)
            {
                var menu = new Menu(title, MenuBar);
                foreach (var item in items)
                {
                    menu.AddItem(item);
                }
                MenuBar!.AddMenu(menu);
            }
            AddMenu("View", []);
            AddMenu("Tools", []);
            AddMenu("Help", []);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            using var app = new Application();

            var mainWindow = new Window();
            mainWindow.Resize(1280, 720);
            mainWindow.Show();


            app.Run();
        }
    }
}
