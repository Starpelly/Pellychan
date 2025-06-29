using HtmlAgilityPack;
using MaterialDesign;
using Pellychan.API;
using Pellychan.API.Models;
using Pellychan.GUI;
using Pellychan.GUI.Widgets;
using Pellychan.Utils;
using SkiaSharp;
using System.Net;

namespace Pellychan.Widgets;

public class PostWidget : Widget, IPaintHandler, IResizeHandler, IMouseClickHandler
{
    private static readonly Padding Padding = new(8);

    private readonly Post m_apiPost;

    private readonly PostThumbnail m_previewBitmap;
    private readonly Label m_nameLabel;
    private readonly Label m_dateLabel;
    private readonly Label? m_previewInfoLabel;
    private readonly Label m_postIDLabel;
    private readonly Label m_commentLabel;

    public PostWidget(API.Models.Post post, Widget? parent = null) : base(parent)
    {
        Name = "A Post widget!!!";
        ShouldCache = true;

        m_apiPost = post;

        // UI Layout
        m_nameLabel = new Label(this)
        {
            X = Padding.Left,
            Y = Padding.Top,
            Text = $"<span class=\"name\">{post.Name}</span>",
            CatchCursorEvents = false,
        };

        m_dateLabel = new Label(this)
        {
            X = Padding.Left,
            Y = Padding.Top,
            Text = $"<span class=\"date\">{post.Now}</span>",
            CatchCursorEvents = false,
        };


        m_postIDLabel = new Label(this)
        {
            X = Padding.Left,
            Y = Padding.Top,
            Text = $"<span class=\"postID\">#{post.No}</span>",
            CatchCursorEvents = false,
        };

        if (post.Tim != null)
        {
            m_previewInfoLabel = new Label(this)
            {
                X = Padding.Left,
                Y = Padding.Top,
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
            X = Padding.Left,
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
    }

    public void SetBitmapPreview(SKImage thumbnail)
    {
        m_previewBitmap.SetThumbnail(thumbnail);
        m_previewInfoLabel!.Text = $"<span class=\"date\">{((long)m_apiPost.Fsize!).FormatBytes()} {m_apiPost.Ext}</span>";

        SetHeight();
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();

        paint.Color = Palette.Get(ColorRole.Base);
        canvas.DrawRect(new(0, 0, Width, Height), paint);
    }

    public void OnResize(int width, int height)
    {
        // Fit thumbnail
        var spaceForText = 200;
        m_previewBitmap.FitToMaxWidth(Width - spaceForText);

        SetHeight();
    }

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

    #region Private methods

    internal void SetHeight()
    {
        SetPositions();

        m_commentLabel.Width = Width - m_commentLabel.X - Padding.Right;
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

        // newHeight = Math.Max(100, newHeight);
        Height = newHeight + Padding.Top + m_nameLabel.Height + Padding.Bottom + ((m_previewInfoLabel?.Height + 8) ?? 0);
    }

    internal void SetPositions()
    {
        m_commentLabel.X = Padding.Left + (m_previewBitmap.Bitmap != null ? (m_previewBitmap.Width + 8) : 0);

        // m_dateLabel.X = Width - m_dateLabel.Width - Padding.Right;
        m_dateLabel.X = m_nameLabel.X + m_nameLabel.Width + 2;
        // m_postIDLabel.X = m_dateLabel.X + m_dateLabel.Width + 2;
        m_postIDLabel.X = Width - Padding.Right - m_postIDLabel.Width;

        if (m_previewInfoLabel != null)
        {
            m_previewInfoLabel.Y = (m_previewBitmap.Bitmap != null ? (m_previewBitmap.Y + m_previewBitmap.Height + 8) : 0);
        }
    }

    #endregion
}