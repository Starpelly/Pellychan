using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace Pellychan;

public class PellychanWindow : MainWindow, IPaintHandler, IMouseDownHandler
{
    private readonly ChanClient m_chanClient = new();

    private readonly SKPaint m_labelPaint = new();
    private readonly SKPicture m_flag;
    private readonly SKFont m_font;

    private List<Label> m_labels = [];

    Rect m_rect;

    public PellychanWindow() : base()
    {
        // m_chanClient.Boards = m_chanClient.GetBoardsAsync().GetAwaiter().GetResult();
        m_labelPaint.Color = SKColors.White;

        m_flag = Helpers.LoadSvgPicture($"Pellychan.Resources.Images.Flags.{Helpers.FlagURL("US")}")!;

        /*
        void createLabel(string text, int x, int y)
        {
            var label = new Label(m_font)
            {
                Text = text,
                X = x,
                Y = y
            };
            this.AddChild(label);

            m_labels.Add(label);
        }

        for (int i = 0; i < m_chanClient.Boards.Count; i++)
        {
            var board = m_chanClient.Boards[i];

            createLabel(board.Title, 16, (i * 16) + 16);
        }
        for (var i = 0; i < 20; i++)
        createLabel("test", 0, 0);
        */

        Menubar = new()
        {
            Width = 1280,
            ScreenPosition = MenuBar.Orientation.Top
        };

        void addMenu(string title, List<MenuItem> items)
        {
            var menu = new Menu(title);
            foreach (var item in items)
            {
                menu.AddItem(item);
            }
            Menubar!.AddMenu(menu);
        }
        addMenu("File",
        [
            new ("Open", () => Console.WriteLine("Open clicked!"))
        ]);
        addMenu("Edit",
        [
            new ("Undo"),
            new ("Redo"),
        ]);
        /*
        addMenu("View", []);
        addMenu("Project", []);
        addMenu("Build", []);
        addMenu("Debug", []);
        addMenu("Test", []);
        addMenu("Window", []);
        addMenu("Help", []);
        */

        AddChild(Menubar);

        var rect1 = new Rect(SKColors.Red)
        {
            X = 16,
            Y = 32,
            Width = 300,
            Height = 300,
        };
        var rect2 = new Rect(SKColors.Green)
        {
            X = 16,
            Y = 16,
            Width = 200,
            Height = 200,
        };
        var rect3 = new Rect(SKColors.Blue)
        {
            X = 16,
            Y = 16,
            Width = 100,
            Height = 100,
        };

        rect1.AddChild(rect2);
        rect2.AddChild(rect3);

        m_rect = rect1;

        AddChild(rect1);
    }

    public void OnMouseDown(int x, int y)
    {
        return;
        m_rect.X = x;
        m_rect.Y = y;

        Invalidate();
    }

    public void OnPaint(SKCanvas canvas)
    {
        canvas.Clear(new(15, 15, 15, 255));

        // Helpers.DrawSvg(canvas, m_flag, new SKRect(0, 0, 256, 256));
        
        /*
        for (int i = 0; i < m_chanClient.Boards.Count; i++)
        {
            var board = m_chanClient.Boards[i];
            var pos = new SKPoint(16, (i * 16) + 16 + 8);

            canvas.DrawText($"/{board.URL}/", pos, m_font, m_labelPaint);
            canvas.DrawText(board.Title, pos + new SKPoint(62, 0), m_font, m_labelPaint);
        }
        */
    }
}