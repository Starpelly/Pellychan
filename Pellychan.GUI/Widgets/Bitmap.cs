using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class Bitmap : Widget, IPaintHandler
{
    public SKBitmap? Image { get; set; }

    public Bitmap(Widget? parent = null) : base(parent) { }

    public void OnPaint(SKCanvas canvas)
    {
        if (Image != null)
        {
            // @NOTE - pelly
            // How widgets are drawn should probably change in the future. It's odd that the
            // canvas' draw position starts at the widget position. It should be global
            // by default and the widget should take care of where to draw itself.
            canvas.DrawBitmap(Image, new SKRect(0, 0, Width, Height));
        }
    }
}