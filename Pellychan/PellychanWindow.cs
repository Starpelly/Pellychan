using ExCSS;
using MaterialDesign;
using Pellychan.API.Models;
using Pellychan.GUI;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using Pellychan.Resources;
using Pellychan.Widgets;
using SkiaSharp;
using System.Diagnostics;

namespace Pellychan;

public class PellychanWindow : MainWindow, IResizeHandler, IMouseDownHandler
{
    private readonly List<PostWidget> m_postWidgets = [];
    public List<PostWidget> Tester => m_postWidgets;

    private ScrollArea m_threadsListWidget;
    private ScrollArea m_postsListWidget;

    public readonly SKFont IconsFont;

    private readonly Label m_boardTitleLabel;
    private readonly Label m_threadTitleLabel;

    public PellychanWindow() : base()
    {
        Layout = new VBoxLayout
        {
        };

        // Setup MenuBar
        {
            MenuBar = new(this)
            {
                Width = this.Width,
                ScreenPosition = MenuBar.Orientation.Top,
            };
            void AddMenu(string title, List<MenuAction> items)
            {
                // var menu = new Menu(title, MenuBar);
                var menu = MenuBar.AddMenu(title);
                foreach (var item in items)
                {
                    menu.AddAction(item);
                }
            }
            AddMenu("File", [
                new(MaterialIcons.Save, "Save"),
                new(MaterialIcons.DoorFront, "Exit"),
            ]);
            AddMenu("View", [
                new(MaterialIcons.Settings, "Preferences", () => {
                    new PreferencesWindow(this).Show();
                }),
            ]);
            AddMenu("Actions", [
                new(MaterialIcons.Refresh, "Refresh All"),
            ]);
            AddMenu("Tools", [
                new(MaterialIcons.Cloud, "Thread Downloader")
            ]);
            AddMenu("Help", [
                new(MaterialIcons.Public, "Website"),
                new(MaterialIcons.Info, "About Pellychan", () => {
                    var info = new ProcessStartInfo()
                    {
                        FileName = "https://boxsubmus.com",
                        UseShellExecute = true,
                    };
                    System.Diagnostics.Process.Start(info);
                })
            ]);
        }

        CentralWidget = new(this)
        {
            Fitting = FitPolicy.ExpandingPolicy
        };

        // Setup UI
        {
            CentralWidget!.Layout = new HBoxLayout
            {
                // Padding = new(4)
            };

            Widget mainHolder = CentralWidget;
            /*
            mainHolder = new ShapedFrame(this)
            {
                Fitting = FitPolicy.ExpandingPolicy,
                Layout = new HBoxLayout
                {
                }
            };
            */

            // Boards list
            {
                new NullWidget(mainHolder)
                {
                    Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                    Width = 64
                };
            }

            CreateSeparator();

            // Threads list
            {
                var threadsListHolder = new NullWidget(mainHolder)
                {
                    Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                    Width = 400,

                    Layout = new VBoxLayout
                    {
                    }
                };

                TabInfoWidgetThing(out m_boardTitleLabel, threadsListHolder);

                m_threadsListWidget = new ScrollArea(threadsListHolder)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Expanding),
                };
                m_threadsListWidget.ContentFrame.Layout = new HBoxLayout
                {
                };
                m_threadsListWidget.ChildWidget = new NullWidget(m_threadsListWidget.ContentFrame)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
                    Layout = new VBoxLayout
                    {
                        Spacing = 1,
                    },
                    Name = "Threads Lists Holder"
                };
            }

            CreateSeparator();

            // Main content
            {
                var postsListHolder = new NullWidget(mainHolder)
                {
                    Fitting = FitPolicy.ExpandingPolicy,

                    Layout = new VBoxLayout
                    {
                    }
                };

                TabInfoWidgetThing(out m_threadTitleLabel, postsListHolder);

                m_postsListWidget = new ScrollArea(postsListHolder)
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
                    AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
                    Layout = new VBoxLayout
                    {
                        Spacing = 1,
                    },
                    Name = "Posts Lists Holder"
                };
            }

            void TabInfoWidgetThing(out Label w, Widget parent)
            {
                // @TODO
                // Add anchor points
                var bg = new Rect(Palette.Get(ColorRole.Base), parent)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    Height = 20,

                    Layout = new HBoxLayout
                    { }
                };
                w = new Label(Application.DefaultFont, bg)
                {
                    Fitting = FitPolicy.ExpandingPolicy,
                    Anchor = Label.TextAnchor.CenterCenter,
                };

                // Separator
                new HLine(parent)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    Height = 1
                };
            }

            void CreateSeparator()
            {
                new VLine(mainHolder)
                {
                    Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                };
            }
        }
    }

    public void LoadBoardCatalog(string board)
    {
        Pellychan.ChanClient.CurrentBoard = board;
        Pellychan.ChanClient.Catalog = Pellychan.ChanClient.GetCatalogAsync().GetAwaiter().GetResult();

        m_boardTitleLabel.Text = $"/{board}/";

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

        // loadPage(m_chanClient.Catalog.Pages[0]);
        // return;
        foreach (var page in Pellychan.ChanClient.Catalog.Pages)
        {
            loadPage(page);
        }
    }

    public void LoadThreadPosts(string threadID)
    {
        foreach (var widget in m_postWidgets)
        {
            widget.Delete(); // I'm thinking this should defer to the next event loop? It could cause problems...
        }
        m_postWidgets.Clear();
        m_postsListWidget.VerticalScrollbar.Value = 0;

        Pellychan.ChanClient.CurrentThread = Pellychan.ChanClient.GetThreadPostsAsync(threadID).GetAwaiter().GetResult();
        m_threadTitleLabel.Text = $"/{Pellychan.ChanClient.CurrentBoard}/{Pellychan.ChanClient.CurrentThread.Posts[0].No}/";

        for (var i = 0; i < Pellychan.ChanClient.CurrentThread.Posts.Count; i++)
        {
            var post = Pellychan.ChanClient.CurrentThread.Posts[i];
            var widget = new PostWidget(post, m_postsListWidget.ChildWidget)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
            };
            m_postWidgets.Add(widget);

            Pellychan.ChanClient.LoadThumbnail(post, (thumbnail) =>
            {
                if (thumbnail != null)
                {
                    Console.WriteLine($"Loaded {(long)post.Tim!}.jpg");
                    widget.SetBitmapPreview(thumbnail);
                }
            });
        }
    }

    public new void OnResize(int width, int height)
    {
        // Console.Clear();
        // Console.WriteLine("\x1b[3J");

        base.OnResize(width, height);

        var menubarHeight = MenuBar != null ? MenuBar.Height : 0;

        /*
        m_centralWidget.Y = menubarHeight;
        m_centralWidget.Width = width;
        m_centralWidget.Height = height - menubarHeight;
        */
    }

    public bool OnMouseDown(int x, int y)
    {
        return true;
    }
}