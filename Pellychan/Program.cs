using Pellychan.API.Models;
using Pellychan.GUI;
using Pellychan.Resources;
using SkiaSharp;
using Svg.Skia;

namespace Pellychan
{
    public class MainWindow : GUI.Widgets.MainWindow
    {
        public static SKPicture? LoadSvgPicture(string resourceName)
        {
            var assembly = PellychanResources.ResourceAssembly;
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;

            var svg = new SKSvg();
            svg.Load(stream);
            return svg.Picture;
        }

        public static void DrawSvg(SKCanvas canvas, SKPicture? picture, SKRect targetBounds)
        {
            if (picture == null) return;

            // Calculate scale
            var originalSize = picture.CullRect;
            var matrix = SKMatrix.CreateScale(
                targetBounds.Width / originalSize.Width,
                targetBounds.Height / originalSize.Height);

            canvas.Save();
            canvas.Translate(targetBounds.Left, targetBounds.Top);
            canvas.DrawPicture(picture, in matrix);
            canvas.Restore();
        }

        private readonly ChanClient m_chanClient = new();

        private readonly List<Board> m_boards = [];
        private readonly SKPaint m_labelPaint = new();

        private readonly SKPicture m_flag;

        public MainWindow()
        {
            m_boards = m_chanClient.GetBoardsAsync().GetAwaiter().GetResult();
            m_labelPaint.Color = SKColors.White;

            m_flag = LoadSvgPicture($"Pellychan.Resources.Images.Flags.{Helpers.FlagURL("US")}")!;
        }

        public override void OnPaint(SKCanvas canvas)
        {
            base.OnPaint(canvas);

            canvas.Clear(new(15, 15, 15, 255));

            DrawSvg(canvas, m_flag, new SKRect(0, 0, 256, 256));
            
            for (var i = 0; i < m_boards.Count; i++)
            {
                var board = m_boards[i];
                canvas.DrawText(board.Title, new SKPoint(16, i * 16), m_labelPaint);
            }
        }
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
