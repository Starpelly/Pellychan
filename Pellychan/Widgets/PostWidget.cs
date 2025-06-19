using Pellychan.API;
using Pellychan.API.Models;
using Pellychan.GUI;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using SkiaSharp;
using System.Net;

namespace Pellychan.Widgets;

public class Thumbnail : Bitmap, IPaintHandler, IMouseDownHandler, IMouseEnterHandler, IMouseLeaveHandler
{
    private readonly Post m_ApiPost;

    private SKBitmap? m_thumbnailBitmap;
    private SKBitmap? m_fullBitmap;

    private bool m_usingThumbnail = true;
    private bool m_loadedFull = false;
    private bool m_triedLoadingFull = false;

    private GifPlayer? m_gifPlayer;

    public Thumbnail(Post post, Widget? parent = null) : base(parent)
    {
        m_ApiPost = post;

        updateImage(null);
    }

    public void SetThumbnail(SKBitmap? thumbnail)
    {
        m_thumbnailBitmap = thumbnail;

        updateImage(m_thumbnailBitmap);
    }

    public void OnMouseDown(int x, int y)
    {
        if (m_usingThumbnail)
        {
            if (!m_loadedFull)
            {
                loadFull();
            }
        }

        if (!m_loadedFull) return;

        m_usingThumbnail = !m_usingThumbnail;

        if (!m_usingThumbnail)
        {
            m_gifPlayer?.Stop();
        }
        else
        {
            m_gifPlayer?.Start();
        }
        updateImage((m_usingThumbnail) ? m_thumbnailBitmap : m_fullBitmap);
    }

    public new void OnPaint(SKCanvas canvas)
    {
        canvas.Save();

        /*
        using var path = new SKPath();
        path.AddRoundRect(new SKRect(0, 0, Width, Height), 6, 6);
        canvas.ClipPath(path, SKClipOperation.Intersect, true);
        */

        base.OnPaint(canvas);

        canvas.Restore();
        
        using var paint = new SKPaint();
        paint.Color = Application.DefaultStyle.GetFrameColor();
        paint.IsStroke = true;
        paint.StrokeWidth = 1;
        canvas.DrawRoundRect(new SKRect(0, 0, Width - 1, Height - 1), 0, 0, paint);
    }

    public void OnMouseEnter()
    {
        MouseCursor.Set(MouseCursor.CursorType.Hand);
    }

    public void OnMouseLeave()
    {
        MouseCursor.Set(MouseCursor.CursorType.Arrow);
    }

    private void updateImage(SKBitmap? bitmap)
    {
        Image = bitmap;

        if (Image == null)
        {
            Resize(0, 0);
            return;
        }

        Resize(Image!.Width, Image.Height);

        (Parent as PostWidget)?.SetHeight();
    }

    private void loadFull()
    {
        if (m_triedLoadingFull) return;
        m_triedLoadingFull = true;

        if (m_ApiPost.Ext == ".gif")
        {
            m_gifPlayer = new();

            var post = m_ApiPost;
            string url = $"https://{Domains.UserContent}/{PellychanWindow.ChanClient.CurrentBoard}/{post.Tim}{post.Ext}";

            Console.WriteLine(url);
            Task.Run(async () =>
            {
                await m_gifPlayer.LoadAsync(url);
                m_loadedFull = true;
                m_usingThumbnail = !m_usingThumbnail;
            });

            m_gifPlayer.OnFrameChanged += delegate ()
            {
                if (!m_usingThumbnail)
                    updateImage(m_gifPlayer.CurrentBitmap);
            };
        }
        else
        {
            PellychanWindow.ChanClient.LoadAttachment(m_ApiPost, (thumbnail) =>
            {
                if (thumbnail != null)
                {
                    m_fullBitmap = thumbnail;

                    m_usingThumbnail = !m_usingThumbnail;
                    updateImage(m_fullBitmap);

                    m_loadedFull = true;
                }
            });
        }
    }
}

public class PostWidget : NullWidget, IPaintHandler, IResizeHandler
{
    private static readonly Padding Padding = new(8);

    private readonly Post m_apiPost;

    private readonly Thumbnail m_previewBitmap;
    private readonly Label m_nameLabel;
    private readonly Label m_dateLabel;
    private readonly Label m_commentLabel;

    public PostWidget(API.Models.Post post, Widget? parent = null) : base(parent)
    {
        Name = "A Post widget!!!";

        m_apiPost = post;

        // UI Layout
        m_nameLabel = new Label(Application.DefaultFont, this)
        {
            X = Padding.Left,
            Y = Padding.Top,
            Text = $"<span class=\"name\">{post.Name}</span>",
            CatchCursorEvents = false
        };

        m_dateLabel = new Label(Application.DefaultFont, this)
        {
            X = Padding.Left,
            Y = Padding.Top,
            Text = $"<span class=\"date\">{post.Now}</span>",
            CatchCursorEvents = false
        };

        var rawComment = post.Com == null ? string.Empty : post.Com;
        var htmlEncoded = rawComment;
        var decoded = WebUtility.HtmlDecode(htmlEncoded);

        var commentY = m_nameLabel.Y + m_nameLabel.Height + 4;

        m_previewBitmap = new(m_apiPost, this)
        {
            X = Padding.Left,
            Y = commentY,
        };

        m_commentLabel = new Label(Application.DefaultFont, this)
        {
            Y = commentY,

            Text = decoded,
            WordWrap = true,

            Fitting = new(GUI.Layouts.FitPolicy.Policy.Fixed, GUI.Layouts.FitPolicy.Policy.Fixed),
            CatchCursorEvents = false,
        };
    }

    public void SetBitmapPreview(SKBitmap thumbnail)
    {
        m_previewBitmap.SetThumbnail(thumbnail);

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
        SetHeight();
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
        Height = newHeight + Padding.Top + m_nameLabel.Height + Padding.Bottom;
    }

    internal void SetPositions()
    {
        m_commentLabel.X = Padding.Left + (m_previewBitmap.Image != null ? (m_previewBitmap.Width + 8) : 0);

        // m_dateLabel.X = Width - m_dateLabel.Width - Padding.Right;
        m_dateLabel.X = m_nameLabel.X + m_nameLabel.Width + 2;
    }

    #endregion
}