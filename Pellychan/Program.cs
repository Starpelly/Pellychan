using Pellychan.API.Models;
using Pellychan.GUI;
using SkiaSharp;

namespace Pellychan
{
    public class MainWindow : GUI.Widgets.MainWindow
    {
        public ChanClient m_chanClient = new();

        private readonly List<Board> m_boards = [];
        private readonly SKPaint m_labelPaint = new();

        public MainWindow()
        {
            m_boards = m_chanClient.GetBoardsAsync().GetAwaiter().GetResult();
            m_labelPaint.Color = SKColors.White;
        }

        public override void OnPaint(SKCanvas canvas)
        {
            base.OnPaint(canvas);

            canvas.Clear(new(15, 15, 15, 255));

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
