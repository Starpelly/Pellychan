using Pellychan.API.Models;
using Pellychan.GUI;
using Pellychan.GUI.Widgets;
using SkiaSharp;
using System.Net;

namespace Pellychan.Widgets;

public class Thumbnail : Bitmap, IPaintHandler, IMouseDownHandler, IMouseEnterHandler, IMouseLeaveHandler
{
    public readonly Post APIPost;

    private SKBitmap? m_thumbnailBitmap;
    private SKBitmap? m_fullBitmap;

    private bool m_usingThumbnail = true;
    private bool m_loadedFull = false;

    public Thumbnail(API.Models.Post post, Widget? parent = null) : base(parent)
    {
        APIPost = post;

        updateImage(m_usingThumbnail);
    }

    public void SetThumbnail(SKBitmap? thumbnail)
    {
        m_thumbnailBitmap = thumbnail;
        updateImage(m_usingThumbnail);
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
        updateImage(m_usingThumbnail);
    }

    public new void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        using var paint = new SKPaint() { Color = Palette.Get(ColorRole.Base), IsStroke = true, StrokeWidth = 1 };
        canvas.DrawRect(new SKRect(0, 0, Width - 1, Height - 1), paint);
    }

    public void OnMouseEnter()
    {
        MouseCursor.Set(MouseCursor.CursorType.Hand);
    }

    public void OnMouseLeave()
    {
        MouseCursor.Set(MouseCursor.CursorType.Arrow);
    }

    private void updateImage(bool thumbnail)
    {
        Image = thumbnail ? m_thumbnailBitmap : m_fullBitmap;

        if (Image == null)
        {
            Resize(0, 0);
            return;
        }

        Resize(Image!.Width, Image.Height);

        (Parent as PostWidget)?.SetTempShit(true);
    }

    private void loadFull()
    {
        PellychanWindow.ChanClient.LoadAttachment(APIPost, (thumbnail) =>
        {
            if (thumbnail != null)
            {
                m_fullBitmap = thumbnail;

                m_usingThumbnail = !m_usingThumbnail;
                updateImage(m_usingThumbnail);

                m_loadedFull = true;
            }
        });
    }
}

public class PostWidget : Widget, IPaintHandler, IResizeHandler
{
    private static readonly Padding Padding = new(8);

    public API.Models.Post APIPost { get; set; }

    private readonly Thumbnail m_previewBitmap;
    private readonly Label m_nameLabel;

    private Label m_commentLabel;

    private NullWidget m_commentHolder;

    public PostWidget(API.Models.Post post, Widget? parent = null) : base(parent)
    {
        APIPost = post;

        // UI Layout
        m_nameLabel = new Label(Application.DefaultFont, this)
        {
            X = Padding.Left,
            Y = Padding.Top,
            Text = $"<span class=\"name\">{post.Name}</span>"
        };

        m_commentHolder = new(this)
        {
            Y = m_nameLabel.Y + m_nameLabel.Height + 4
        };

        var rawComment = post.Com == null ? string.Empty : post.Com;
        var htmlEncoded = rawComment;
        var decoded = WebUtility.HtmlDecode(htmlEncoded);

        m_previewBitmap = new(APIPost, this)
        {
            X = Padding.Left,
            Y = m_commentHolder.Y,
        };

        m_commentLabel = new Label(Application.DefaultFont, m_commentHolder)
        {
            Text = decoded,
        };

        // Height = m_previewBitmap.Height + (Padding.Bottom * 2) + 1;

        SetTempShit(true);
    }

    public void SetBitmapPreview(SKBitmap thumbnail)
    {
        m_previewBitmap.SetThumbnail(thumbnail);

        SetTempShit(true);
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();

        // BG
        // paint.Color = Palette.Get(ColorRole.Window).Lighter(0.9f);
        paint.Color = Palette.Get(ColorRole.Base);
        canvas.DrawRect(new(0, 0, Width, Height), paint);

        // Separator
        // paint.Color = Palette.Get(ColorRole.HighlightedText);
        // canvas.DrawLine(0, Rect.Bottom - 1, Rect.Right, Rect.Bottom - 1, paint);
    }

    public void SetTempShit(bool setHeight)
    {
        if (setHeight)
        {
            if (m_previewBitmap != null)
            {
                Height = m_previewBitmap.Height + (m_previewBitmap.Y) + Padding.Bottom;
                Height = Math.Max(100, Height);
            }
            else
            {
                Height = 100;
            }
        }

        m_commentHolder.X = Padding.Left + (m_previewBitmap.Image != null ? (m_previewBitmap.Width + 8) : 0);
        m_commentHolder.Width = Width - m_commentHolder.X - Padding.Right;
        m_commentHolder.Height = Height - m_commentHolder.Y - Padding.Bottom;
    }

    public void OnResize(int width, int height)
    {
        SetTempShit(false);
    }
}