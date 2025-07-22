﻿using Pellychan.API.Models;
using Pellychan.GUI;
using Pellychan.GUI.Input;
using Pellychan.GUI.Widgets;
using SkiaSharp;
using System.Net;

namespace Pellychan.Widgets;

internal class ThreadWidget : Widget, IPaintHandler, IPostPaintHandler, IMouseEnterHandler, IMouseLeaveHandler, IMouseDownHandler
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

        var rawComment = thread.Com ?? string.Empty;
        var htmlEncoded = rawComment;
        var decoded = WebUtility.HtmlDecode(htmlEncoded);

        m_commentLabel = new Label(this)
        {
            X = Padding.Left,
            Y = Padding.Top,

            Text = decoded,
            WordWrap = true,
            CatchCursorEvents = false,
            ShouldCache = false
        };
    }

    public void SetBitmapPreview(SKImage image)
    {
        m_previewImage.Bitmap = image;

        var newWidth = image.Width;
        var newHeight = image.Height;

        if (newWidth > MaxImageWidth)
        {
            newWidth = MaxImageWidth;
            newHeight = (int)(((float)newWidth / image.Width) * image.Height);
        }

        m_previewImage.Width = newWidth;
        m_previewImage.Height = newHeight;

        updateLayout();
    }

    public void OnPaint(SKCanvas canvas)
    {
        using var paint = new SKPaint();

        paint.Color = m_hovering ? Application.DefaultStyle.GetButtonHoverColor().Lighter(1.5f) : Palette.Get(ColorRole.Button);
        canvas.DrawRect(new(0, 0, Width, Height), paint);
    }

    public void OnPostPaint(SKCanvas canvas)
    {
        Padding textPadding = new(8);

        using var paint = new SKPaint();

        var metaRect = new SKRectI(0, 0, 200, (int)Application.DefaultFont.Size + (textPadding.Top + textPadding.Bottom));
        metaRect.Left = Width - metaRect.Width;
        metaRect.Right = Width + 1;
        metaRect = metaRect.SetY(Height - metaRect.Height);
        // metaRect.Left = Width - metaRect.Right;

        using var roundRect = new SKRoundRect(metaRect);
        roundRect.SetRectRadii(metaRect,
        [
            new SKPoint(0, 0),
            new SKPoint(),
            new SKPoint(),
            new SKPoint(),
        ]);

        // paint.IsAntialias = true;
        paint.Color = Palette.Get(ColorRole.Window).Lighter(1.1f);
        canvas.DrawRoundRect(roundRect, paint);

        paint.IsStroke = true;
        paint.Color = Palette.Get(ColorRole.Window);
        canvas.DrawRoundRect(roundRect, paint);
        paint.IsAntialias = false;

        canvas.Save();
        canvas.Translate(metaRect.Left + textPadding.Left, metaRect.Top + textPadding.Top);

        paint.IsStroke = false;
        paint.Color = Palette.Get(ColorRole.Text);

        var iconX = 0;
        void drawIconText(string icon, string label)
        {
            var iconWidth = Application.FontIcon.MeasureText(icon);
            var labelWidth = Application.DefaultFont.MeasureText(label);
            var spacing = 4;

            canvas.DrawText(icon, new SKPoint(iconX, Application.FontIcon.Size - 2), Application.FontIcon, paint);
            canvas.DrawText(label, new SKPoint(iconX + iconWidth + spacing, Application.DefaultFont.Size - 1), Application.DefaultFont, paint);

            iconX += (int)(iconWidth + labelWidth + spacing + 8);
        }

        drawIconText(MaterialDesign.MaterialIcons.ModeComment, Thread.Replies.ToString());
        drawIconText(MaterialDesign.MaterialIcons.Image, Thread.Images.ToString());
        drawIconText(MaterialDesign.MaterialIcons.AccessTime, unixToTimeAgo(Thread.Time));

        canvas.Restore();
    }

    private string unixToTimeAgo(long unixTime)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
        DateTime dateTime = dateTimeOffset.UtcDateTime;

        DateTime now = DateTime.UtcNow;
        TimeSpan diff = now - dateTime;

        if (diff.TotalSeconds < 60)
        {
            return $"{(int)diff.TotalSeconds} second{(diff.Seconds != 1 ? "s" : "")} ago";
        }
        else if (diff.TotalMinutes < 60)
        {
            return $"{(int)diff.TotalMinutes} minute{(diff.Minutes != 1 ? "s" : "")} ago";
        }
        else if (diff.TotalHours < 24)
        {
            return $"{(int)diff.TotalHours} hour{(diff.Hours != 1 ? "s" : "")} ago";
        }
        else
        {
            return $"{(int)diff.TotalDays} day{(diff.Days != 1 ? "s" : "")} ago";
        }
    }

    public override void OnPostLayout()
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

    public bool OnMouseDown(MouseEvent evt)
    {
        if (evt.button == MouseButton.Left)
        {
            ChanApp.LoadThread(Thread.No.ToString());
        }

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