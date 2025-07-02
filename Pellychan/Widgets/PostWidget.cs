using HtmlAgilityPack;
using MaterialDesign;
using Pellychan.API.Models;
using Pellychan.GUI;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using Pellychan.Utils;
using SkiaSharp;
using System.Net;

namespace Pellychan.Widgets;

public class PostWidgetContainer : Widget, IPaintHandler
{
    private static readonly Padding Padding = new(8);
    private static readonly int Spacing = 2;

    public Post APIPost => m_postWidget.APIPost;

    private readonly PostWidget m_postWidget;
    public PostWidget Test => m_postWidget;

    private NullWidget? m_repliesHolder = null;
    private PushButton? m_showRepliesButton;

    private bool m_viewingReplies = false;

    public List<string> ReferencedPosts => m_postWidget.ReferencedPosts;

    public PostWidgetContainer(Post post, Widget? parent = null) : base(parent)
    {
        Name = "PostWidgetContainer";

        this.Layout = new VBoxLayout
        {
            Padding = new(8),
            Spacing = 8
        };
        this.AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit);

        m_postWidget = new PostWidget(post, this)
        {
            Width = this.Width,
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
        };
    }

    public void SetReplies(List<PostWidgetContainer> replies)
    {
        if (replies.Count == 0) return;

        /*
        var repliesString = new StringBuilder();

        foreach (var widget in replies)
        {
            repliesString.Append($">{widget.m_postWidget.APIPost.No} ");
        }
        m_postWidget.SetReplies(repliesString.ToString());
        */

        m_showRepliesButton = new PushButton("View replies", this)
        {
            X = Padding.Left,
            OnClicked = () =>
            {
                if (!m_viewingReplies)
                {
                    loadReplies(replies);
                }
            }
        };
    }

    private void loadReplies(List<PostWidgetContainer> replies)
    {
        m_repliesHolder = new NullWidget(this)
        {
            Name = "Hello",
            Width = this.Width,
            
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
            AutoSizing = new(SizePolicy.Policy.Ignore, SizePolicy.Policy.Fit),

            Layout = new VBoxLayout
            {
                Padding = new(32, 0, -8, 0),
                Spacing = 1,
            },
        };

        m_viewingReplies = true;
        var pw = new Dictionary<int, PostWidgetContainer>(replies.Count);
        foreach (var item in replies)
        {
            var widget = new PostWidgetContainer(item.m_postWidget.APIPost, m_repliesHolder)
            {
                Width = this.Width,
                Fitting = new(GUI.Layouts.FitPolicy.Policy.Expanding, GUI.Layouts.FitPolicy.Policy.Fixed)
            };
            pw.Add(item.m_postWidget.APIPost.No, widget);
        }
        Pellychan.MainWindow.Bruhhh(pw);
    }

    #region Widget events

    public override void OnPostLayout()
    {
        m_postWidget.OnResize();
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();

        paint.Color = Palette.Get(ColorRole.Base);
        canvas.DrawRect(new(0, 0, this.Width, this.Height), paint);
    }

    #endregion
}

public class PostWidget : Widget, IMouseClickHandler
{
    private readonly Post m_apiPost;
    public Post APIPost => m_apiPost;

    private readonly PostThumbnail m_previewBitmap;
    private readonly Label m_nameLabel;
    private readonly Label m_dateLabel;
    private readonly Label? m_previewInfoLabel;
    private readonly Label m_postIDLabel;
    private readonly Label m_commentLabel;
    private readonly Label m_repliesLabel;

    public readonly List<string> ReferencedPosts = [];

    public PostWidget(API.Models.Post post, Widget? parent = null) : base(parent)
    {
        Name = "A Post widget!!!";
        ShouldCache = true;

        m_apiPost = post;

        // UI Layout
        m_nameLabel = new Label(this)
        {
            X = 0,
            Y = 0,
            Text = $"<span class=\"name\">{post.Name}</span>",
            CatchCursorEvents = false,
        };

        m_dateLabel = new Label(this)
        {
            X = 0,
            Y = 0,
            Text = $"<span class=\"date\">{post.Now}</span>",
            CatchCursorEvents = false,
        };

        m_postIDLabel = new Label(this)
        {
            X = 0,
            Y = 0,
            Text = $"<span class=\"postID\">#{post.No}</span>",
            CatchCursorEvents = false,
        };

        m_repliesLabel = new Label(this)
        {
        };

        if (post.Tim != null)
        {
            m_previewInfoLabel = new Label(this)
            {
                X = 0,
                Y = 0,
            };
        }

        var rawComment = post.Com == null ? string.Empty : post.Com;
        var htmlEncoded = rawComment;
        var decoded = WebUtility.HtmlDecode(htmlEncoded);
        var commentInput = decoded;

        // sanitize html
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(decoded);

            foreach (var node in doc.DocumentNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "a":
                        switch (node.GetAttributeValue("class", ""))
                        {
                            case "quotelink":
                                ReferencedPosts.Add(node.InnerHtml.TrimStart('>'));
                                if (node.InnerText == $">>{Pellychan.ChanClient.CurrentThread.No}")
                                {
                                    node.InnerHtml = $"{node.InnerHtml} (OP)";
                                }
                                break;
                        }
                        break;
                }
            }

            commentInput = doc.DocumentNode.OuterHtml;
            // Console.WriteLine(commentInput);
        }

        var commentY = m_nameLabel.Y + m_nameLabel.Height + 4;

        m_previewBitmap = new(m_apiPost, this)
        {
            X = 0,
            Y = commentY,
        };

        m_commentLabel = new Label(this)
        {
            Y = commentY,

            Text = commentInput,
            WordWrap = true,

            Fitting = new(GUI.Layouts.FitPolicy.Policy.Fixed, GUI.Layouts.FitPolicy.Policy.Fixed),
            CatchCursorEvents = false,
        };

        SetHeight();
    }

    public void SetBitmapPreview(SKImage thumbnail)
    {
        m_previewBitmap.SetThumbnail(thumbnail);
        m_previewInfoLabel!.Text = $"<span class=\"date\">{((long)m_apiPost.Fsize!).FormatBytes()} {m_apiPost.Ext}</span>";

        SetHeight();
    }

    #region Widget events

    public bool OnMouseClick(MouseEvent evt)
    {
        if (evt.button == GUI.Input.MouseButton.Right)
        {
            var threadURL = $"https://boards.4chan.org/{Pellychan.ChanClient.CurrentBoard}/thread/{Pellychan.ChanClient.CurrentThread.No}";
            var postURL = $"{threadURL}#p{m_apiPost.No}";

            MenuPopup a = new(this);
            var m = new Menu(this);

            m.AddAction(MaterialIcons.Link, "Copy Post URL to Clipboard", () =>
            {
                Application.Clipboard.SetText(postURL);
            });
            m.AddAction(MaterialIcons.Public, "Open Post in Browser", () =>
            {
                Application.OpenURL(postURL);
            });
            m.AddAction(MaterialIcons.Feed, "Open Thread in Browser", () =>
            {
                Application.OpenURL(threadURL);
            });

            m.AddSeparator();
            m.AddAction(MaterialIcons.Reply, "Reply", null);

            a.SetMenu(m);
            a.SetPosition(evt.globalX, evt.globalY);

            a.Show();
        }


        return true;
    }

    #endregion

    internal void SetReplies(string replies)
    {
        m_repliesLabel.Text = replies;
    }

    #region Private methods

    internal void OnResize()
    {
        // Fit thumbnail
        var spaceForText = 200;
        m_previewBitmap.FitToMaxWidth(this.Width - spaceForText);

        SetPositions();
        SetHeight();
    }

    internal void SetPositions()
    {
        m_commentLabel.X = (m_previewBitmap.Bitmap != null ? (m_previewBitmap.Width + 8) : 0);

        // m_dateLabel.X = Width - m_dateLabel.Width - Padding.Right;
        m_dateLabel.X = m_nameLabel.X + m_nameLabel.Width + 2;
        // m_postIDLabel.X = m_dateLabel.X + m_dateLabel.Width + 2;
        m_postIDLabel.X = this.Width - m_postIDLabel.Width;

        if (m_previewInfoLabel != null)
        {
            m_previewInfoLabel.Y = (m_previewBitmap.Bitmap != null ? (m_previewBitmap.Y + m_previewBitmap.Height + 8) : 0);
        }

        m_repliesLabel.X = m_dateLabel.X + m_dateLabel.Width + 2;
    }

    internal void SetHeight()
    {
        m_commentLabel.Width = this.Width - m_commentLabel.X;
        m_commentLabel.Height = m_commentLabel.MeasureHeightFromWidth(m_commentLabel.Width);

        int newHeight = 0;
        if (m_commentLabel.Height > m_previewBitmap.Height)
        {
            newHeight += m_commentLabel.Height + 4;
        }
        else
        {
            newHeight = m_previewBitmap.Height + 4;
        }

        this.Height = newHeight + m_nameLabel.Height + ((m_previewInfoLabel?.Height + 8) ?? 0);
    }

    #endregion
}