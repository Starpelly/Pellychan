using Pellychan.GUI;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using Pellychan.Widgets;
using SkiaSharp;
using System.Xml.Linq;

namespace Pellychan;

public class PellychanWindow : MainWindow, IPaintHandler, IResizeHandler, IMouseDownHandler
{
    public static PellychanWindow Instance { get; private set; }
    public static ChanClient ChanClient => Instance.m_chanClient;

    private readonly ChanClient m_chanClient = new();
    private API.Models.Thread m_thread;

    private readonly SKPaint m_labelPaint = new();
    private readonly SKPicture m_flag;

    private readonly List<Label> m_labels = [];
    private readonly List<PostWidget> m_postWidgets = [];

    private Widget m_centralWidget;
    private Widget m_mainContentWidget;

    private int m_clickCount = 0;

    private int test_count = 8;

    public PellychanWindow() : base()
    {
        Instance = this;

        createMenubar();

        m_chanClient.Boards = m_chanClient.GetBoardsAsync().GetAwaiter().GetResult();
        m_chanClient.CurrentBoard = "vg";

        m_thread = m_chanClient.GetThreadAsync("527536942").GetAwaiter().GetResult();

        m_labelPaint.Color = SKColors.White;

        m_flag = Helpers.LoadSvgPicture($"Pellychan.Resources.Images.Flags.{Helpers.FlagURL("US")}")!;

        test_count = m_thread.Posts.Count;

        createUI();

        for (var i = 0; i < test_count; i++)
        {
            var post = m_thread.Posts[i];

            if (post.Tim == null) continue;

            m_chanClient.LoadThumbnail(post, i, (thumbnail, index) =>
            {
                if (thumbnail != null)
                {
                    Console.WriteLine($"Loaded {(long)post.Tim}");
                    Done(index, thumbnail);
                }
            });
        }
    }

    public void Done(int index, SKBitmap thumbnail)
    {
        m_postWidgets[index].SetBitmapPreview(thumbnail);
    }

    private void createUI()
    {
        m_centralWidget = new NullWidget(this)
        {
            // Y = Menubar!.Height,
            SizePolicy = SizePolicy.ExpandingPolicy,
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
                // SizePolicy = SizePolicy.ExpandingPolicy,
                Width = 200
            };

            var boardsListContainer = new Rect(Application.Palette.Get(ColorRole.Base), boardsContainer)
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

                for (int i = 0; i < m_chanClient.Boards.Boards.Count; i++)
                {
                    var board = m_chanClient.Boards.Boards[i];

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

            boardsListWidget.OnLayoutResize += boardsListContainer.OnLayoutResize += delegate()
            {
                scroll.Maximum = Math.Max(0, boardsListWidget.Height - boardsContainer.Height);
                scroll.PageStep = boardsContainer.Height;

                scroll.Value = Math.Clamp(scroll.Value, scroll.Minimum, scroll.Maximum);
                scroll.Enabled = scroll.Maximum > 0;
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

        if (test_count == 0) return;
        
        // Main content
        {
            m_mainContentWidget = new Widget(m_centralWidget)
            {
                SizePolicy = new(SizePolicy.Policy.Expanding, SizePolicy.Policy.Fixed),
                Layout = new VBoxLayout
                {
                    Spacing = 1
                },
                Width = 100,
                Height = 100
            };

            for (var i = 0; i < test_count; i++)
            {
                var post = m_thread.Posts[i];
                var widget = new PostWidget(post, m_mainContentWidget)
                {
                    SizePolicy = new(SizePolicy.Policy.Expanding, SizePolicy.Policy.Fixed)
                };
                m_postWidgets.Add(widget);

                /*
                var widget = new Label(Application.DefaultFont, m_mainContentWidget)
                {
                    SizePolicy = new(SizePolicy.Policy.Expanding, SizePolicy.Policy.Fixed),
                    Text = "test"
                };
                */
            }

            var scroll = new ScrollBar(m_centralWidget)
            {
                X = 400,
                Y = 16,
                Width = 16,
                Height = 400,
                SizePolicy = new(SizePolicy.Policy.Fixed, SizePolicy.Policy.Expanding)
            };
            scroll.OnValueChanged += delegate (int value)
            {
                m_mainContentWidget.Y = -value;
            };

            m_mainContentWidget.OnLayoutResize += m_centralWidget.OnLayoutResize += delegate ()
            {
                scroll.Maximum = Math.Max(0, m_mainContentWidget.Height - m_centralWidget.Height);
                scroll.PageStep = m_centralWidget.Height;

                scroll.Value = Math.Clamp(scroll.Value, scroll.Minimum, scroll.Maximum);
                scroll.Enabled = scroll.Maximum > 0;
            };
            m_mainContentWidget.OnLayoutUpdate += delegate ()
            {
                m_mainContentWidget.Resize(m_mainContentWidget.Width, m_mainContentWidget.SizeHint.Height);
            };
        }
    }

    private void createMenubar()
    {
        Menubar = new(this)
        {
            Width = this.Width,
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
        AddMenu("View", []);
        AddMenu("Tools", []);
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
        // Console.Clear();
        // Console.WriteLine("\x1b[3J");

        base.OnResize(width, height);

        var menubarHeight = Menubar != null ? Menubar.Height : 0;

        m_centralWidget.Y = menubarHeight;
        m_centralWidget.Width = width;
        m_centralWidget.Height = height - menubarHeight;
    }

    public void OnMouseDown(int x, int y)
    {

    }
}