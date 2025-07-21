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

        var gs = getGlobalPosition(this);
        var globalPos = new SKPoint(gs.Item1, gs.Item2);

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
            if (!Config.HardwareAccel)
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
    /// Paints to the canvas directly.
    /// </summary>
    private void paintNoCache(SKCanvas canvas, SKRect clipRect, SkiaWindow window)
    {
        if (m_width <= 0 || m_height <= 0 || !ShouldDrawFast)
            return;

        if (m_windowType == WindowType.Popup)
        {
            var a = 0;
        }

        var gs = getGlobalPosition(this);
        var globalPos = new SKPoint(gs.Item1, gs.Item2);

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

        // Popups have a cool dropshadow
        if (m_windowType == WindowType.Popup && !IsTopLevel && !IsWindow)
        {
            var dropShadowFilter = SKImageFilter.CreateDropShadowOnly(
                dx: 4,
                dy: 3,
                sigmaX: 2,
                sigmaY: 2,
                color: SKColors.Black.WithAlpha(100)
            );

            using var paint = new SKPaint
            {
                Color = SKColors.Red,
                ImageFilter = dropShadowFilter,
                IsAntialias = true
            };

            var rect = new SKRect(0, 0, m_width, m_height);
            canvas.DrawRect(rect, paint);
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