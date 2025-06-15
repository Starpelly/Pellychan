using Pellychan.GUI;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace Pellychan;

public class PellychanWindow : MainWindow, IPaintHandler, IResizeHandler
{
    private readonly ChanClient m_chanClient = new();

    private readonly SKPaint m_labelPaint = new();
    private readonly SKPicture m_flag;

    private readonly List<Label> m_labels = [];

    private Widget m_centralWidget;

    private int m_clickCount = 0;

    public PellychanWindow() : base()
    {
        // createMenubar();

        m_chanClient.Boards = m_chanClient.GetBoardsAsync().GetAwaiter().GetResult();
        m_labelPaint.Color = SKColors.White;

        m_flag = Helpers.LoadSvgPicture($"Pellychan.Resources.Images.Flags.{Helpers.FlagURL("US")}")!;

        createUI();
    }

    private void createUI()
    {
        m_centralWidget = new Widget(this)
        {
            // Y = Menubar!.Height,
            Layout = new HBoxLayout
            {
                Align = HBoxLayout.VerticalAlignment.Top,
                Padding = new()
            }
        };

        // Boards
        {
            var boardsContainer = new Widget(m_centralWidget)
            {
                Layout = new HBoxLayout
                {
                    Spacing = 0,
                },
                SizePolicy = new(SizePolicy.Policy.Fixed, SizePolicy.Policy.Expanding),
                Width = 200
            };

            var boardsListContainer = new Widget(boardsContainer)
            {
                Layout = new HBoxLayout
                {
                },
                SizePolicy = SizePolicy.ExpandingPolicy
            };
            Widget boardsListWidget;

            // List
            {
                boardsListWidget = new Rect(Application.Palette.Get(ColorRole.Base), boardsListContainer)
                {
                    SizePolicy = new SizePolicy(SizePolicy.Policy.Expanding, SizePolicy.Policy.Fixed),
                    Layout = new VBoxLayout
                    {
                        Spacing = 2,
                        Align = VBoxLayout.HorizontalAlignment.Center,
                        Padding = new(16)
                    }
                };

                void createLabel(string text, int x, int y)
                {
                    var label = new Label(Application.DefaultFont, boardsListWidget)
                    {
                        Text = text
                    };

                    m_labels.Add(label);
                }

                for (int i = 0; i < m_chanClient.Boards.Count; i++)
                {
                    var board = m_chanClient.Boards[i];

                    createLabel(board.Title, 16, (i * 16) + 16);
                }
            }

            var scroll = new ScrollBar(boardsContainer)
            {
                X = 400,
                Y = 16,
                Width = 16,
                Height = 400,
                SizePolicy = new(SizePolicy.Policy.Fixed, SizePolicy.Policy.Expanding)
            };
            scroll.OnValueChanged += delegate(int value)
            {
                boardsListWidget.Y = -value;
            };

            boardsListContainer.OnResize += delegate()
            {
                scroll.Maximum = boardsListWidget.Height - boardsContainer.Height;
                scroll.PageStep = boardsContainer.Height;
            };
            boardsListWidget.OnLayoutUpdate += delegate()
            {
                boardsListWidget.Resize(boardsListWidget.Width, boardsListWidget.SizeHint.Height);
            };
        }

        // Separator (temp?)
        {
            new Rect(new SKColor(42, 42, 45), m_centralWidget)
            {
                SizePolicy = new(SizePolicy.Policy.Fixed, SizePolicy.Policy.Expanding),
                Width = 1
            };
        }

        // Main content
        {
            var mainContentWidget = new Widget(m_centralWidget)
            {
                SizePolicy = SizePolicy.ExpandingPolicy
            };

            for (int i = 0; i < 1; i++)
            {
                var button = new PushButton("Test Notification", mainContentWidget)
                {
                    X = 16,
                    Y = 16 + (i * 29)
                };
                button.OnClicked += delegate ()
                {
                    m_clickCount++;
                    button.Text = $"Test Notification ({m_clickCount})";
                };
            }

            new ScrollBar(mainContentWidget)
            {
                X = 400,
                Y = 16,
                Width = 16,
                Height = 400
            };
        }

        // Idk lol
        if (false)
        {
            var rect1 = new Rect(SKColors.Red, m_centralWidget)
            {
                X = 16,
                Y = 32,
                Width = 300,
                Height = 300,
                SizePolicy = new(SizePolicy.Policy.Fixed, SizePolicy.Policy.Expanding)
            };
            var rect2 = new Rect(SKColors.Green, rect1)
            {
                X = 16,
                Y = 16,
                Width = 200,
                Height = 200,
            };
            var rect3 = new Rect(SKColors.Blue, rect2)
            {
                X = 16,
                Y = 16,
                Width = 100,
                Height = 100,
            };

            rect1.Show();
        }
    }

    private void createMenubar()
    {
        Menubar = new(this)
        {
            Width = 1280,
            ScreenPosition = MenuBar.Orientation.Top,
        };

        void AddMenu(string title, List<MenuItem> items)
        {
            var menu = new Menu(title, Menubar);
            foreach (var item in items)
            {
                menu.AddItem(item);
            }
            Menubar!.AddMenu(menu);
        }
        AddMenu("File",
        [
            new ("Open", () => Console.WriteLine("Open clicked!"))
        ]);
        AddMenu("Edit",
        [
            new ("Undo"),
            new ("Redo"),
        ]);
        AddMenu("View", []);
        AddMenu("Build", []);
        AddMenu("Debug", []);
        AddMenu("Test", []);
        AddMenu("Window", []);
        AddMenu("Help", []);
    }

    public void OnPaint(SKCanvas canvas)
    {
        canvas.Clear(EffectivePalette.Get(GUI.ColorGroup.Active, GUI.ColorRole.Window));

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

    public new void OnResize(int width, int height)
    {
        base.OnResize(width, height);
        m_centralWidget?.Resize(width, height);
    }
}