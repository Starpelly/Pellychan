﻿using Pellychan.API.Models;
using Pellychan.API;
using Pellychan.GUI.Widgets;
using Pellychan.GUI;
using SkiaSharp;
using Pellychan.Utils;

namespace Pellychan.Widgets;

public class PostThumbnail : Image, IPaintHandler, IMouseDownHandler, IMouseEnterHandler, IMouseLeaveHandler
{
    public const int MaxImageWidth = 1280;

    private readonly Post m_ApiPost;

    private SKImage? m_thumbnailImage;
    private SKImage? m_fullImage;

    private bool m_usingThumbnail = true;
    private bool m_loadedFull = false;
    private bool m_triedLoadingFull = false;

    private GifPlayer? m_gifPlayer;

    public PostThumbnail(Post post, Widget? parent = null) : base(parent)
    {
        m_ApiPost = post;

        updateImage(null);
    }

    public void SetThumbnail(SKImage thumbnail)
    {
        m_thumbnailImage = thumbnail;

        updateImage(m_thumbnailImage);
    }

    public bool OnMouseDown(MouseEvent evt)
    {
        if (evt.button != GUI.Input.MouseButton.Left)
            return false;

        if (m_usingThumbnail)
        {
            if (!m_loadedFull)
            {
                loadFull();
            }
        }

        if (!m_loadedFull) return false;

        if (!m_usingThumbnail)
        {
            m_gifPlayer?.Stop();
        }
        else
        {
            m_gifPlayer?.Start();
        }
        m_usingThumbnail = !m_usingThumbnail;
        updateImage((m_usingThumbnail) ? m_thumbnailImage : m_fullImage);

        return true;
    }

    public new void OnPaint(SKCanvas canvas)
    {
        canvas.Save();

        base.OnPaint(canvas);

        canvas.Restore();

        using var paint = new SKPaint();
        paint.Color = Application.DefaultStyle.GetFrameColor();
        paint.IsStroke = true;
        canvas.DrawRoundRect(new SKRect(0, 0, Width - 1, Height - 1), 0, 0, paint);

        // Idk if we wanna update the gif while it isn't painted?
        m_gifPlayer?.Update();
    }

    public void OnMouseEnter()
    {
        MouseCursor.Set(MouseCursor.CursorType.Hand);
    }

    public void OnMouseLeave()
    {
        MouseCursor.Set(MouseCursor.CursorType.Arrow);
    }

    internal void FitToMaxWidth(int maxWidth)
    {
        if (Bitmap == null)
            return;

        var newWidth = Bitmap.Width;
        var newHeight = Bitmap.Height;

        var fullPreviewWidth = m_fullImage != null ? m_fullImage.Width : newWidth;
        if (maxWidth > newWidth)
        {
            maxWidth = newWidth;
        }

        if (newWidth > maxWidth || newWidth > PostThumbnail.MaxImageWidth)
        {
            newWidth = maxWidth;
            newHeight = (int)(((float)newWidth / Bitmap.Width) * Bitmap.Height);
        }
        Resize(newWidth, newHeight);
    }

    #region Private methods

    private void updateImage(SKImage? bitmap)
    {
        Bitmap = bitmap;

        if (Bitmap == null)
        {
            Resize(0, 0);
            return;
        }

        FitToMaxWidth(m_fullImage != null ? m_fullImage.Width : MaxImageWidth);

        (Parent as PostWidget)?.OnResize();
    }

    private void loadFull()
    {
        if (m_triedLoadingFull) return;
        m_triedLoadingFull = true;

        if (m_ApiPost.Ext == ".gif")
        {
            m_gifPlayer = new();

            var post = m_ApiPost;
            string url = $"https://{Domains.UserContent}/{Pellychan.ChanClient.CurrentBoard}/{post.Tim}{post.Ext}";

            Console.WriteLine(url);
            _ = m_gifPlayer.LoadAsync(url, () =>
            {
                m_loadedFull = true;
                m_usingThumbnail = !m_usingThumbnail;

                // Fallback to the first frame in the gif for the full image
                m_fullImage = m_gifPlayer.CurrentImage;
            });

            m_gifPlayer.OnFrameChanged = () =>
            {
                if (!m_usingThumbnail)
                    updateImage(m_gifPlayer.CurrentImage);
            };
        }
        else
        {
            _ = Pellychan.ChanClient.DownloadAttachmentAsync(m_ApiPost, (thumbnail) =>
            {
                if (thumbnail != null)
                {
                    m_fullImage = thumbnail;

                    m_usingThumbnail = !m_usingThumbnail;
                    updateImage(m_fullImage);

                    m_loadedFull = true;
                }
            });
        }
    }

    #endregion

    public override void Dispose()
    {
        base.Dispose();

        m_thumbnailImage?.Dispose();
        m_fullImage?.Dispose();
        m_gifPlayer?.Dispose();

        Console.WriteLine("Dispose");
    }
}