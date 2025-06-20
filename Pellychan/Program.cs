using Pellychan.GUI;
using Pellychan.Resources;

namespace Pellychan
{    
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using var app = new Application();
            
            var mainWindow = new PellychanWindow();
            mainWindow.SetWindowTitle("Pellychan");
            mainWindow.Resize(1280, 720);
            
            // Icon
            using var iconStream = PellychanResources.ResourceAssembly.GetManifestResourceStream("Pellychan.Resources.Images.4channy.ico");
            mainWindow.SetIconFromStream(iconStream!);

            mainWindow.Show();


            app.Run();
        }
    }
}
