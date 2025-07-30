using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using Pellychan.Widgets;
using SkiaSharp;

namespace Pellychan;

public class MinTestWindow : MainWindow, IMouseDownHandler, IMouseMoveHandler, IMouseUpHandler
{
    private ScrollArea m_postsListWidget;
    private readonly Dictionary<int, PostWidgetContainer> m_postWidgets = [];
    private Widget m_mainHolder;

    public MinTestWindow() : base()
    {
        m_mainHolder = new ShapedFrame(this)
        {
            X = 16,
            Y = 16,
            Width = 300,
            Height = 300,

            Layout = new VBoxLayout
            { }
        };

        if (false)
        {
            var t = new ScrollArea(m_mainHolder)
            {
                Fitting = FitPolicy.ExpandingPolicy
            };
            t.ChildWidget = new Rect(SKColors.Red, t.ContentFrame)
            {
                Fitting = FitPolicy.ExpandingPolicy,
                AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),
                Layout = new VBoxLayout
                {
                    Padding = new(8),
                    Spacing = 4
                }
            };
            for (var i = 0; i < 0; i++)
            {
                new Rect(SKColors.Blue, t.ChildWidget)
                {
                    Height = 32,
                    Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
                };
            }
        }
        // Main content
        else
        {
            var postsListHolder = new NullWidget(m_mainHolder)
            {
                Fitting = FitPolicy.ExpandingPolicy,

                Layout = new VBoxLayout
                {
                }
            };

            // TabInfoWidgetThing(out m_threadTitleLabel, postsListHolder);

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
    }

    public void LoadThreadPosts(string threadID)
    {
        if (m_postsListWidget == null)
            return;

        clearPosts();

        // m_threadTitleLabel.Text = $"<span class=\"header\">{Pellychan.ChanClient.CurrentThread.Posts[0].Sub}</span>";


        var count = ChanApp.ChanClient.CurrentThread.Posts.Count;
        // count = 1;

        var imageIDs = new Dictionary<long, PostWidgetContainer>(count);
        for (var i = 0; i < count; i++)
        {
            var post = ChanApp.ChanClient.CurrentThread.Posts[i];
            var widget = new PostWidgetContainer(post, m_postsListWidget.ChildWidget)
            {
                Name = $"Post Widget Container ({i})",
                Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
            };
            m_postWidgets.Add(post.No, widget);

            if (post.Tim != null && post.Tim > 0)
            {
                imageIDs.Add((long)post.Tim, widget);
            }
        }

        // Bruhhh(m_postWidgets);

        // Load thumbnails for posts
        _ = ChanApp.ChanClient.LoadThumbnailsAsync(imageIDs.Keys, (long tim, SKImage? image) =>
        {
            if (image != null)
            {
                imageIDs[tim].Test.SetBitmapPreview(image);
            }
        });
    }

    private void clearPosts()
    {
        foreach (var widget in m_postWidgets)
        {
            widget.Value.Dispose(); // I'm thinking this should defer to the next event loop? It could cause problems...
        }
        m_postWidgets.Clear();
        m_postsListWidget.VerticalScrollbar.Value = 0;
    }

    public void Bruhhh(Dictionary<int, PostWidgetContainer> widgetsToUpdate)
    {
        var refPosts = new Dictionary<int, List<PostWidgetContainer>>();

        foreach (var key in widgetsToUpdate.Keys)
        {
            refPosts.Add(key, []);
        }

        foreach (var widget in m_postWidgets)
        {
            foreach (var refID in widget.Value.ReferencedPosts)
            {
                if (int.TryParse(refID, out var id))
                {
                    // if (postWidgets.TryGetValue(id, out var value))
                    if (widgetsToUpdate.ContainsKey(id))
                    {
                        // list.Add(value);
                        refPosts[id].Add(widget.Value);
                    }
                    else
                    {
                        // @NOTE
                        // So this indicates the post comes from another thread
                        // I need to add a case to handle this!
                    }
                }
            }
        }

        foreach (var post in refPosts)
        {
            widgetsToUpdate[post.Key].SetReplies(post.Value);
        }
    }

    private bool m_resizing = false;

    public bool OnMouseDown(MouseEvent evt)
    {
        m_resizing = true;

        m_mainHolder.SetRect(m_mainHolder.X, m_mainHolder.Y, evt.x - 32, evt.y - 32);

        return false;
    }

    public bool OnMouseMove(int x, int y)
    {
        if (m_resizing)
        {
            m_mainHolder.SetRect(m_mainHolder.X, m_mainHolder.Y, x - 32, y - 32);
        }

        return true;
    }

    public bool OnMouseUp(MouseEvent evt)
    {
        m_resizing = false;

        return false;
    }
}