using Pellychan.GUI;
using LayoutTesting.Tests;

namespace LayoutTesting
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var app = new Application();

            var mainWindow = new Caching();
            mainWindow.SetWindowTitle("Layout Testing");
            mainWindow.Resize(1280, 720);

            mainWindow.Show();

            app.Run();
        }
    }
}
