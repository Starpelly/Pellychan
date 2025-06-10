using Pellychan.GUI;

namespace Pellychan
{    
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using var app = new Application();
            
            var mainWindow = new MainWindow();
            mainWindow.SetWindowTitle("Pellychan");
            mainWindow.Resize(1280, 720);
            mainWindow.Show();
            
            app.Run();
        }
    }
}
