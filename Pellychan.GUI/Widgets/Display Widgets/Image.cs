using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class Image : Widget, IPaintHandler
{
    public SKImage? Bitmap { get; set; }

    public Image(Widget? parent = null) : base(parent)
    {
        ShouldCache = false;
    }

    public void OnPaint(SKCanvas canvas)
    {
        if (Bitmap != null)
        {
            SKSamplingOptions options = new();
            // @NOTE - pelly
            // How widgets are drawn should probably change in the future. It's odd that the
            // canvas' draw position starts at the widget position. It should be global
            // by default and the widget should take care of where to draw itself.
            canvas.DrawImage(Bitmap, new SKRect(0, 0, Width, Height), options);
        }
    }
}