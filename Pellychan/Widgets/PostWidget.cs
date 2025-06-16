using Pellychan.GUI;
using Pellychan.GUI.Widgets;
using SkiaSharp;

namespace Pellychan.Widgets;

public class PostWidget : Widget, IPaintHandler
{
    private static readonly Padding Padding = new(8);

    public API.Models.Post APIPost { get; set; }

    private Bitmap m_previewBitmap;
    private Label m_nameLabel;
    private Label m_commentLabel;

    private string m_content;

    public PostWidget(API.Models.Post post, Widget? parent = null) : base(parent)
    {
        APIPost = post;
        m_content = post.Com;

        // UI Layout
        m_nameLabel = new Label(Application.DefaultFont, this)
        {
            X = Padding.Left,
            Y = Padding.Top,
            Text = post.Name
        };

        var contentY = m_nameLabel.Y + m_nameLabel.Height + 4;
        m_previewBitmap = new Bitmap(this)
        {
            X = Padding.Left,
            Y = contentY,
        };
        m_commentLabel = new Label(Application.DefaultFont, this)
        {
            X = Padding.Left + m_previewBitmap.Width,
            Y = contentY,
            Text = (post.Com == null) ? string.Empty : post.Com,
        };

        // Height = m_previewBitmap.Height + (Padding.Bottom * 2) + 1;

        setTempShit();
    }

    public void SetBitmapPreview(SKBitmap thumbnail)
    {
        m_previewBitmap.Image = thumbnail;
        m_previewBitmap.Resize(thumbnail.Width, thumbnail.Height);

        setTempShit();
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();

        // BG
        paint.Color = Palette.Get(ColorRole.Button);
        // paint.Color = SKColors.Red.WithAlpha(50);
        canvas.DrawRect(new(0, 0, Width, Height), paint);

        // Separator
        // paint.Color = Palette.Get(ColorRole.HighlightedText);
        // canvas.DrawLine(0, Rect.Bottom - 1, Rect.Right, Rect.Bottom - 1, paint);
    }

    private void setTempShit()
    {
        Height = m_previewBitmap.Height + (m_previewBitmap.Y) + Padding.Bottom;
        Height = Math.Max(100, Height);

        m_commentLabel.X = Padding.Left + (m_previewBitmap.Image != null ? (m_previewBitmap.Width + 8) : 0);
        m_commentLabel.Height = Height - m_commentLabel.Y - Padding.Bottom;
    }
}