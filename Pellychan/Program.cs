using Pellychan.GUI;

namespace Pellychan
{
    public class MainWindow : GUI.Widgets.MainWindow
    {
        
    }
    
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using var app = new Application();
            
            var mainWindow = new MainWindow();
            mainWindow.Resize(1280, 720);
            mainWindow.Show();
            mainWindow.SetTitle("Pellychan");
            
            app.Run();
        }
    }
}
