using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class Image : Widget, IPaintHandler
{
    // @NOTE
    // There should be a flag to set if the widget owns the actual bitmap or not so it can dispose of it properly....
    public SKImage? Bitmap { get; set; }

    public bool PreserveAspectRatio { get; set; } = true;

    public Image(Widget? parent = null) : base(parent)
    {
        ShouldCache = false;
    }

    public void OnPaint(SKCanvas canvas)
    {
        if (Bitmap != null)
        {
            SKSamplingOptions options = new(SKFilterMode.Linear);

            // @NOTE - pelly
            // How widgets are drawn should probably change in the future. It's odd that the
            // canvas' draw position starts at the widget position. It should be global
            // by default and the widget should take care of where to draw itself.
            SKRect destRect;

            if (PreserveAspectRatio)
            {
                float imageWidth = Bitmap.Width;
                float imageHeight = Bitmap.Height;

                float widgetAspect = Width / Height;
                float imageAspect = imageWidth / imageHeight;

                float drawWidth, drawHeight;

                if (imageAspect > widgetAspect)
                {
                    // Image is wider than widget — fit to width
                    drawWidth = Width;
                    drawHeight = Width / imageAspect;
                }
                else
                {
                    // Image is taller than widget — fit to height
                    drawHeight = Height;
                    drawWidth = Height * imageAspect;
                }

                float offsetX = (Width - drawWidth) / 2;
                float offsetY = (Height - drawHeight) / 2;

                destRect = new SKRect(offsetX, offsetY, offsetX + drawWidth, offsetY + drawHeight);
            }
            else
            {
                destRect = new SKRect(0, 0, Width, Height);
            }

            canvas.DrawImage(Bitmap, destRect, options);
        }
    }
}