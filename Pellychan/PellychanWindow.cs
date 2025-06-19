using Newtonsoft.Json.Linq;
using Pellychan.GUI;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using Pellychan.Widgets;
using SkiaSharp;
using System.Xml.Linq;

namespace Pellychan;

public class PellychanWindow : MainWindow, IResizeHandler, IMouseDownHandler
{
    public static PellychanWindow Instance { get; private set; }
    public static ChanClient ChanClient => Instance.m_chanClient;

    private readonly ChanClient m_chanClient = new();
    private API.Models.Thread m_thread;

    private readonly SKPaint m_labelPaint = new();
    private readonly SKPicture m_flag;

    private readonly List<Label> m_labels = [];
    private readonly List<PostWidget> m_postWidgets = [];

    private ScrollArea m_mainContentWidget;

    private int m_clickCount = 0;

    private int test_count = 8;

    public PellychanWindow() : base()
    {
        Instance = this;

        // createMenubar();

        m_chanClient.Boards = m_chanClient.GetBoardsAsync().GetAwaiter().GetResult();
        m_chanClient.CurrentBoard = "v";

        m_thread = m_chanClient.GetThreadAsync("712960801").GetAwaiter().GetResult();

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
        Layout = new HBoxLayout
        {
        };

        // Boards
        {
            var boardsContainer = new Widget(this)
            {
                Layout = new HBoxLayout
                {
                    Spacing = 0,
                },
                Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                Width = 200
            };

            var boardsListContainer = new Rect(Application.Palette.Get(ColorRole.Base), boardsContainer)
            {
                Layout = new HBoxLayout
                {
                },
                Fitting = FitPolicy.ExpandingPolicy
            };
            Widget boardsListWidget;

            // List
            {
                boardsListWidget = new Rect(Application.Palette.Get(ColorRole.Base), boardsListContainer)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    Sizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
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
                Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding)
            };
            scroll.OnValueChanged += delegate(int value)
            {
                boardsListWidget.Y = -value;
            };

            boardsListWidget.OnPostLayoutUpdate += boardsListContainer.OnPostLayoutUpdate += delegate()
            {
                scroll.Maximum = Math.Max(0, boardsListWidget.Height - boardsListContainer.Height);
                scroll.PageStep = boardsListContainer.Height;

                scroll.Value = Math.Clamp(scroll.Value, scroll.Minimum, scroll.Maximum);
                scroll.Enabled = scroll.Maximum > 0;
            };
        }

        // Separator
        {
            new VLine(this)
            {
                Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                Width = 1
            };
        }

        if (test_count == 0) return;
        
        // Main content
        {
            m_mainContentWidget = new ScrollArea(this)
            {
                Fitting = FitPolicy.ExpandingPolicy,
            };
            m_mainContentWidget.ContentFrame.Layout = new HBoxLayout
            {
            };
            m_mainContentWidget.ChildWidget = new NullWidget(m_mainContentWidget.ContentFrame)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                Sizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
                Layout = new VBoxLayout
                {
                    Spacing = 1,
                }
            };

            for (var i = 0; i < test_count; i++)
            {
                var post = m_thread.Posts[i];
                var widget = new PostWidget(post, m_mainContentWidget.ChildWidget)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
                };
                m_postWidgets.Add(widget);
            }
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

    public new void OnResize(int width, int height)
    {
        // Console.Clear();
        // Console.WriteLine("\x1b[3J");

        base.OnResize(width, height);

        var menubarHeight = Menubar != null ? Menubar.Height : 0;

        /*
        m_centralWidget.Y = menubarHeight;
        m_centralWidget.Width = width;
        m_centralWidget.Height = height - menubarHeight;
        */
    }

    public void OnMouseDown(int x, int y)
    {

    }
}