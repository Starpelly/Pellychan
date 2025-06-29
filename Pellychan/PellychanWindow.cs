using MaterialDesign;
using Pellychan.API.Models;
using Pellychan.GUI;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using Pellychan.Widgets;
using SkiaSharp;
using System.Diagnostics;

namespace Pellychan;

public class PellychanWindow : MainWindow, IResizeHandler, IMouseDownHandler
{
    private readonly List<ThreadWidget> m_threadWidgets = [];
    private readonly List<PostWidget> m_postWidgets = [];

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
                var menu = MenuBar.AddMenu(title);
                foreach (var item in items)
                {
                    menu.AddAction(item);
                }
            }
            /*
            AddMenu("File", [
                new(MaterialIcons.Save, "Save"),
                new(MaterialIcons.DoorFront, "Exit"),
            ]);
            */
            AddMenu("View", [
                new("New Window"),
                new("")
                {
                    IsSeparator = true,
                },
                new(MaterialIcons.Settings, "Preferences", () => {
                    new PreferencesWindow(this).Show();
                }),
            ]);
            AddMenu("Actions", [
                new(MaterialIcons.Refresh, "Refresh All"),
            ]);
            AddMenu("Tools", [
                new(MaterialIcons.Cloud, "Thread Downloader"),
                new(MaterialIcons.Terminal, "Toggle System Console"),
            ]);
            AddMenu("Help", [
                new(MaterialIcons.Public, "Website", () => {
                    Application.OpenURL("https://boxsubmus.com");

                }),
                new(MaterialIcons.ImportContacts, "Wiki", () => {
                    Application.OpenURL("https://github.com/Starpelly/pellychan/wiki");
                }),
                new("")
                {
                    IsSeparator = true,
                },
                new(MaterialIcons.Code, "Source Code", () => {
                    Application.OpenURL("https://github.com/Starpelly/pellychan");
                }),

                new(MaterialIcons.Info, "About Pellychan")
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
            if (true)
            {
                var boardsListHolder = new NullWidget(mainHolder)
                {
                    Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding),
                    Width = 64,

                    Layout = new VBoxLayout { }
                };

                var m_boardsListWidget = new ScrollArea(boardsListHolder)
                {
                    Fitting = FitPolicy.ExpandingPolicy
                };
                m_boardsListWidget.VerticalScrollbar.Visible = false;

                m_boardsListWidget.ContentFrame.Layout = new HBoxLayout
                {
                };

                m_boardsListWidget.ChildWidget = new NullWidget(m_boardsListWidget.ContentFrame)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
                    Layout = new VBoxLayout
                    {
                        Padding = new(8),
                        Spacing = 4,
                    },
                    Name = "Boards Lists Holder"
                };

                foreach (var board in Pellychan.ChanClient.Boards.Boards)
                {
                    new PushButton(board.URL, m_boardsListWidget.ChildWidget)
                    {
                        Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                        OnClicked = () =>
                        {
                            Pellychan.LoadCatalog(board.URL);
                        }
                    };
                }
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
                var bg = new Rect(Palette.Get(ColorRole.Window), parent)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    Height = 48,

                    Layout = new HBoxLayout
                    {
                        Padding = new(12, 8)
                    }
                };
                w = new Label(bg)
                {
                    Fitting = FitPolicy.ExpandingPolicy,
                    Anchor = Label.TextAnchor.CenterLeft,
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
        clearThreads();
        clearPosts();

        Pellychan.ChanClient.CurrentBoard = board;
        Pellychan.ChanClient.Catalog = Pellychan.ChanClient.GetCatalogAsync().GetAwaiter().GetResult();

        m_boardTitleLabel.Text = $"<span class=\"header\">/{board}/ - {Pellychan.ChanClient.Boards.Boards.Find(c => c.URL == board).Title}</span>";

        var ids = new Dictionary<long, ThreadWidget>();
        void loadPage(CatalogPage page)
        {
            foreach (var thread in page.Threads)
            {
                var widget = new ThreadWidget(thread, m_threadsListWidget.ChildWidget)
                {
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
                    Height = 50,
                };
                m_threadWidgets.Add(widget);

                if (thread.Tim != null && thread.Tim > 0)
                {
                    ids.Add((long)thread.Tim, widget);
                }
            }
        }

        // loadPage(m_chanClient.Catalog.Pages[0]);
        // return;
        foreach (var page in Pellychan.ChanClient.Catalog.Pages)
        {
            loadPage(page);
        }

        // Load thumbnails for threads
        _ = Pellychan.ChanClient.LoadThumbnailsAsync(ids.Keys, (long tim, SKImage? image) =>
        {
            if (image != null)
            {
                ids[tim].SetBitmapPreview(image);
            }
        });
    }

    public void LoadThreadPosts(string threadID)
    {
        clearPosts();

        Pellychan.ChanClient.CurrentThread = Pellychan.ChanClient.GetThreadPostsAsync(threadID).GetAwaiter().GetResult();
        m_threadTitleLabel.Text = $"<span class=\"header\">{Pellychan.ChanClient.CurrentThread.Posts[0].Sub}</span>";

        var ids = new Dictionary<long, PostWidget>();
        for (var i = 0; i < Pellychan.ChanClient.CurrentThread.Posts.Count; i++)
        {
            var post = Pellychan.ChanClient.CurrentThread.Posts[i];
            var widget = new PostWidget(post, m_postsListWidget.ChildWidget)
            {
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
            };
            m_postWidgets.Add(widget);

            if (post.Tim != null && post.Tim > 0)
            {
                ids.Add((long)post.Tim, widget);
            }
        }

        // Load thumbnails for posts
        _ = Pellychan.ChanClient.LoadThumbnailsAsync(ids.Keys, (long tim, SKImage? image) =>
        {
            if (image != null)
            {
                ids[tim].SetBitmapPreview(image);
            }
        });
    }

    private void clearThreads()
    {
        foreach (var widget in m_threadWidgets)
        {
            widget.Delete(); // I'm thinking this should defer to the next event loop? It could cause problems...
        }
        m_threadWidgets.Clear();
        m_threadsListWidget.VerticalScrollbar.Value = 0;
    }

    private void clearPosts()
    {
        foreach (var widget in m_postWidgets)
        {
            widget.Delete(); // I'm thinking this should defer to the next event loop? It could cause problems...
        }
        m_postWidgets.Clear();
        m_postsListWidget.VerticalScrollbar.Value = 0;
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

    public bool OnMouseDown(MouseEvent evt)
    {
        return true;
    }
}