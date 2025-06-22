using Pellychan.API.Models;
using Pellychan.GUI;
using Pellychan.GUI.Widgets;
using SkiaSharp;
using System.Net;

namespace Pellychan.Widgets;

internal class ThreadWidget : Widget, IPaintHandler, IPostPaintHandler, IResizeHandler, IMouseEnterHandler, IMouseLeaveHandler, IMouseDownHandler
{
    private const int MaxImageWidth = 75;
    private static readonly Padding Padding = new(8);

    public CatalogThread Thread;

    private readonly Image m_previewImage;
    private readonly Label m_commentLabel;

    private bool m_hovering = false;

    public ThreadWidget(CatalogThread thread, Widget? parent = null) : base(parent)
    {
        Thread = thread;
        Name = "A thread widget!";
        ShouldCache = true;

        m_previewImage = new Image(this)
        {
            X = Padding.Left,
            Y = Padding.Top,

            CatchCursorEvents = false
        };

        var rawComment = thread.Com == null ? string.Empty : thread.Com;
        var htmlEncoded = rawComment;
        var decoded = WebUtility.HtmlDecode(htmlEncoded);

        m_commentLabel = new Label(Application.DefaultFont, this)
        {
            X = Padding.Left,
            Y = Padding.Top,

            Text = decoded,
            WordWrap = true,
            CatchCursorEvents = false,
            ShouldCache = false
        };

        PellychanWindow.ChanClient.LoadThumbnail(thread, (thumbnail) =>
        {
            if (thumbnail != null)
            {
                m_previewImage.Bitmap = thumbnail;

                var newWidth = thumbnail.Width;
                var newHeight = thumbnail.Height;

                if (newWidth > MaxImageWidth)
                {
                    newWidth = MaxImageWidth;
                    newHeight = (int)(((float)newWidth / thumbnail.Width) * thumbnail.Height);
                }

                m_previewImage.Width = newWidth;
                m_previewImage.Height = newHeight;

                updateLayout();
            }
        });
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();

        paint.Color = m_hovering ? Application.DefaultStyle.GetButtonHoverColor().Darker(1.1f) : Palette.Get(ColorRole.Button);
        canvas.DrawRect(new(0, 0, Width, Height), paint);
    }

    public void OnPostPaint(SKCanvas canvas)
    {
        Padding textPadding = new(8);

        using var paint = new SKPaint();

        var metaRect = new SKRectI(0, 0, 64, (int)Application.DefaultFont.Size + (textPadding.Top + textPadding.Bottom));
        metaRect.Left = Width - metaRect.Width;
        metaRect.Right = Width + 1;
        metaRect = metaRect.SetY(Height - metaRect.Height);
        // metaRect.Left = Width - metaRect.Right;

        using var roundRect = new SKRoundRect(metaRect);
        roundRect.SetRectRadii(metaRect,
        [
            new SKPoint(8, 4),
            new SKPoint(),
            new SKPoint(),
            new SKPoint(),
        ]);

        // paint.IsAntialias = true;
        paint.Color = Palette.Get(ColorRole.Button);
        canvas.DrawRoundRect(roundRect, paint);

        paint.IsStroke = true;
        paint.Color = Palette.Get(ColorRole.Window);
        canvas.DrawRoundRect(roundRect, paint);
        paint.IsAntialias = false;

        canvas.Save();
        canvas.Translate(metaRect.Left + textPadding.Left, metaRect.Top + textPadding.Top);

        paint.IsStroke = false;
        paint.Color = Palette.Get(ColorRole.Text);

        void drawIconText(string icon, string label)
        {
            var iconWidth = PellychanWindow.Instance.IconsFont.MeasureText(icon);
            var labelWidth = Application.DefaultFont.MeasureText(label);

            canvas.DrawText(icon, new SKPoint(0, PellychanWindow.Instance.IconsFont.Size - 2), PellychanWindow.Instance.IconsFont, paint);
            canvas.DrawText(label, new SKPoint(iconWidth + 4, Application.DefaultFont.Size - 1), Application.DefaultFont, paint);
        }

        drawIconText(MaterialDesign.MaterialIcons.Reply, Thread.Replies.ToString());

        canvas.Restore();
    }

    public void OnResize(int width, int height)
    {
        updateLayout();
    }

    public void OnMouseEnter()
    {
        m_hovering = true;

        TriggerRepaint();
    }

    public void OnMouseLeave()
    {
        m_hovering = false;

        TriggerRepaint();
    }

    public bool OnMouseDown(int x, int y)
    {
        PellychanWindow.Instance.LoadThread(Thread.No.ToString());

        return true;
    }

    #region Private methods

    private void updateLayout()
    {
        int newHeight = m_previewImage.Height;

        m_commentLabel.X = Padding.Left + (m_previewImage.Bitmap != null ? (m_previewImage.Width + 8) : 0);
        m_commentLabel.Width = Width - m_commentLabel.X - Padding.Right;
        m_commentLabel.Height = m_commentLabel.MeasureHeightFromWidth(m_commentLabel.Width);

        if (m_commentLabel.Height > newHeight)
        {
            newHeight = m_commentLabel.Height;
        }

        // newHeight = Math.Max(100, newHeight);
        Height = newHeight + Padding.Top /*+ m_nameLabel.Height*/ + Padding.Bottom + 24;
    }

    #endregion
}