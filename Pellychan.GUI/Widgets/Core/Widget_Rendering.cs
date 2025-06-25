using Pellychan.GUI.Framework.Platform.Skia;
using SDL;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public partial class Widget
{
    private void renderDebug(SKCanvas canvas)
    {
        if (m_height <= 0 || m_height <= 0 || !ShouldDrawFast)
            return;

        // Cache debug mode?
        // Multiple debug modes?
        // Idk yet...
        //if (!ShouldCache)
        //    return;

        var globalPos = getGlobalPosition();

        canvas.Save();
        canvas.ResetMatrix();

        static SKColor Lerp(SKColor from, SKColor to, float t)
        {
            // Clamp t between 0 and 1
            t = Math.Clamp(t, 0f, 1f);

            byte r = (byte)(from.Red + (to.Red - from.Red) * t);
            byte g = (byte)(from.Green + (to.Green - from.Green) * t);
            byte b = (byte)(from.Blue + (to.Blue - from.Blue) * t);
            byte a = (byte)(from.Alpha + (to.Alpha - from.Alpha) * t);

            return new SKColor(r, g, b, a);
        }

        var framesSinceLastPaint = Application.CurrentFrame - m_lastPaintFrame;
        var maxCounter = 60;

        s_debugPaint.Color = (ShouldCache ? Lerp(SKColors.Green, SKColors.Red, (float)framesSinceLastPaint / maxCounter) : SKColors.Blue);

        canvas.DrawRect(new SKRect(globalPos.X, globalPos.Y, globalPos.X + (m_width - 1), globalPos.Y + (m_height - 1)), s_debugPaint);

        canvas.Restore();

        if (m_children.Count > 0)
        {
            foreach (var child in m_children)
            {
                if (!child.VisibleWidget)
                    continue;

                child.renderDebug(canvas);
            }
        }
    }

    private unsafe void renderWidget(SDL_Renderer* renderer, int x, int y, SKRect clipRect)
    {
        var newX = m_x + x;
        var newY = m_y + y;

        var thisRect = new SKRect(newX, newY, newX + m_width, newY + m_height);
        var currentClip = SKRect.Intersect(clipRect, thisRect);

        if (currentClip.IsEmpty)
            return;

        if (m_cachedRenderTexture != null)
        {
            if (Application.HardwareAccel)
            {

            }
            else
            {
                SDL.SDL3.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
                var destRect = new SDL_FRect
                {
                    x = newX,
                    y = newY,
                    w = m_width,
                    h = m_height
                };
                /*

                SDL.SDL3.SDL_RenderRect(renderer, &test);
                */

                SDL3.SDL_RenderTexture(renderer, m_cachedRenderTexture, null, &destRect);
            }
        }

        foreach (var child in m_children)
        {
            if (!child.ShouldDraw)
                continue;

            unsafe
            {
                child.renderWidget(renderer, newX, newY, currentClip);
            }
        }
    }

    /// <summary>
    /// Paints to the cache canvas, then paints the cache to the canvas.
    /// </summary>
    private void paintCache(SKCanvas canvas, SKRect clipRect, SkiaWindow window)
    {
        if (m_height <= 0 || m_height <= 0 || !ShouldDrawFast)
            return;

        var globalPos = getGlobalPosition();

        var thisRect = new SKRect(globalPos.X, globalPos.Y, globalPos.X + m_width, globalPos.Y + m_height);
        var currentClip = SKRect.Intersect(clipRect, thisRect);

        if (currentClip.IsEmpty)
            return;

        /*
        foreach (var clip in clipStack)
        {
            var a = this;
            if (!clip.IntersectsWith(thisRect))
                return;
        }

        clipStack.Push(thisRect);
        */

        canvas.Save();
        canvas.Translate(m_x, m_y);
        canvas.ClipRect(new(0, 0, m_width, m_height));

        if (m_isDirty || m_hasDirtyDescendants)
        {
            m_lastPaintFrame = Application.CurrentFrame;
            m_isDirty = false;

            bool recreateTexture = false;

            if (!IsTopLevel)
            {
                /*
                if (m_cachedBitmap != null || m_width != m_cachedWidth || m_height != m_cachedHeight)
                {
                    m_cachedBitmap?.Dispose();
                    m_cachedBitmap = new SKBitmap(m_width, m_height);
                    m_cachedWidth = m_width;
                    m_cachedHeight = m_height;
                }
                */

                if (m_width != m_cachedWidth || m_height != m_cachedHeight)
                {
                    recreateTexture = true;
                    m_cachedBitmap?.Dispose();
                    m_cachedBitmap = new SKBitmap(m_width, m_height);

                    m_cachedWidth = m_width;
                    m_cachedHeight = m_height;
                }
            }

            // using var recorder = new SKPictureRecorder();
            // var paintCanvas = recorder.BeginRecording(new SKRect(0, 0, m_width, m_height));

            using (var paintCanvas = new SKCanvas(m_cachedBitmap))
            {
                Console.WriteLine("OnPaint");
                paintCanvas.Clear(SKColors.Transparent);
                (this as IPaintHandler)?.OnPaint(paintCanvas);

                if (m_children.Count > 0)
                {
                    foreach (var child in m_children)
                    {
                        if (!child.VisibleWidget)
                            continue;

                        child.Paint(paintCanvas, clipRect, window);
                    }
                }
            }

            // m_cachedPicture = recorder.EndRecording();

            unsafe
            {
                if (recreateTexture)
                {
                    var surface = SDL3.SDL_CreateSurfaceFrom(m_width, m_height, SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888, m_cachedBitmap!.GetPixels(), m_cachedBitmap.RowBytes);

                    if (m_cachedRenderTexture != null)
                    {
                        SDL3.SDL_DestroyTexture(m_cachedRenderTexture);
                    }
                    m_cachedRenderTexture = SDL3.SDL_CreateTextureFromSurface(window.SDLRenderer, surface);

                    SDL3.SDL_DestroySurface(surface);
                }
                else
                {
                    SDL3.SDL_UpdateTexture(m_cachedRenderTexture, null, m_cachedBitmap!.GetPixels(), m_cachedBitmap.RowBytes);
                }
            }
        }

        if (m_cachedBitmap != null)
        {
            // canvas.DrawBitmap(m_cachedBitmap, m_x, m_y);
        }
        if (m_cachedPicture != null)
        {
            // m_cachedPicture.Playback(canvas);
            // canvas.DrawPicture(m_cachedPicture, 0, 0);
        }

        canvas.Restore();
    }

    /// <summary>
    /// Paints to the canvas directly.
    /// </summary>
    private void paintNoCache(SKCanvas canvas, SKRect clipRect, SkiaWindow window)
    {
        if (m_height <= 0 || m_height <= 0 || !ShouldDrawFast)
            return;

        var globalPos = getGlobalPosition();

        var thisRect = new SKRect(globalPos.X, globalPos.Y, globalPos.X + m_width, globalPos.Y + m_height);
        var currentClip = SKRect.Intersect(clipRect, thisRect);

        if (currentClip.IsEmpty)
            return;

        /*
        foreach (var clip in clipStack)
        {
            var a = this;
            if (!clip.IntersectsWith(thisRect))
                return;
        }

        clipStack.Push(thisRect);
        */

        canvas.Save();
        if (!IsWindow)
        {
            // @INVESTIGATE
            // This should be acknolwedged at least, the position probably shouldn't change if the widget is a window?
            canvas.Translate(m_x, m_y);
        }
        canvas.ClipRect(new(0, 0, m_width, m_height));

        (this as IPaintHandler)?.OnPaint(canvas);

        if (m_children.Count > 0)
        {
            foreach (var child in m_children)
            {
                if (!child.VisibleWidget)
                    continue;


                child.Paint(canvas, clipRect, window);
            }
        }

        (this as IPostPaintHandler)?.OnPostPaint(canvas);

        // clipStack.Pop();

        canvas.Restore();
    }
}