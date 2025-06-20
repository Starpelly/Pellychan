using Newtonsoft.Json.Linq;
using Pellychan.API.Models;
using Pellychan.GUI;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using Pellychan.Widgets;
using SkiaSharp;
using System;
using System.Reflection;
using System.Xml.Linq;

namespace Pellychan;

public class PellychanWindow : MainWindow, IResizeHandler, IMouseDownHandler
{
    public static PellychanWindow Instance { get; private set; }
    public static ChanClient ChanClient => Instance.m_chanClient;

    private readonly ChanClient m_chanClient = new();
    private API.Models.Thread m_thread;

    private readonly SKPicture m_flag;

    private readonly List<PostWidget> m_postWidgets = [];
    public List<PostWidget> Tester => m_postWidgets;

    private ScrollArea m_threadsListWidget;
    private ScrollArea m_postsListWidget;

    public PellychanWindow() : base()
    {
        Instance = this;

        m_chanClient.Boards = m_chanClient.GetBoardsAsync().GetAwaiter().GetResult();
        m_chanClient.CurrentBoard = "vg";

        // m_flag = Helpers.LoadSvgPicture($"Pellychan.Resources.Images.Flags.{Helpers.FlagURL("US")}")!;

        // createMenubar();
        createUI();

        m_chanClient.Catalog = m_chanClient.GetCatalogAsync().GetAwaiter().GetResult();
        LoadBoardThreads();
    }

    public void LoadBoardThreads()
    {
        void loadPage(CatalogPage page)
        {
            foreach (var thread in page.Threads)
            {
                new ThreadWidget(thread, m_threadsListWidget.ChildWidget)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    Height = 50,
                };
            }
        }

        loadPage(m_chanClient.Catalog.Pages[0]);
        return;
        foreach (var page in m_chanClient.Catalog.Pages)
        {
            loadPage(page);
        }
    }

    public void LoadThread(string threadID)
    {
        foreach (var widget in m_postWidgets)
        {
            widget.Delete(); // I'm thinking this should defer to the next event loop? It could cause problems...
        }
        m_postWidgets.Clear();
        m_postsListWidget.VerticalScrollbar.Value = 0;

        m_thread = m_chanClient.GetThreadPostsAsync(threadID).GetAwaiter().GetResult();

        for (var i = 0; i < m_thread.Posts.Count; i++)
        {
            var post = m_thread.Posts[i];
            var widget = new PostWidget(post, m_postsListWidget.ChildWidget)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
            };
            m_postWidgets.Add(widget);

            m_chanClient.LoadThumbnail(post, (thumbnail) =>
            {
                if (thumbnail != null)
                {
                    Console.WriteLine($"Loaded {(long)post.Tim}.jpg");
                    widget.SetBitmapPreview(thumbnail);
                }
            });
        }
    }

    private void createUI()
    {
        Layout = new HBoxLayout
        {
            // Padding = new(4)
        };

        Widget mainHolder = this;
        /*
        mainHolder = new ShapedFrame(this)
        {
            Fitting = FitPolicy.ExpandingPolicy,
            Layout = new HBoxLayout
            {
            }
        };
        */

        // Threads list
        {
            m_threadsListWidget = new ScrollArea(mainHolder)
            {
                Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                Width = 400,
            };
            m_threadsListWidget.ContentFrame.Layout = new HBoxLayout
            {
            };
            m_threadsListWidget.ChildWidget = new NullWidget(m_threadsListWidget.ContentFrame)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                Sizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
                Layout = new VBoxLayout
                {
                    Spacing = 1,
                },
                Name = "Threads Lists Holder"
            };
        }

        // Separator
        {
            new VLine(mainHolder)
            {
                Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
            };
        }

        // Main content
        {
            m_postsListWidget = new ScrollArea(mainHolder)
            {
                Fitting = FitPolicy.ExpandingPolicy,
                Name = "Main Content Holder"
            };
            m_postsListWidget.ContentFrame.Layout = new HBoxLayout
            {
            };
            m_postsListWidget.ChildWidget = new NullWidget(m_postsListWidget.ContentFrame)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                Sizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
                Layout = new VBoxLayout
                {
                    Spacing = 1,
                },
                Name = "Posts Lists Holder"
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