using Pellychan.GUI;
using Pellychan.GUI.Layouts;
using Pellychan.GUI.Widgets;
using Pellychan.Resources;
using SkiaSharp;

namespace Pellychan.Widgets;

public class AboutDialog : DialogWindow, IPaintHandler
{
    public class Fire
    {
        private const int c_width = 320;
        private const int c_height = 240;
        private const int c_randSize = 3777;

        private readonly byte[][] m_fire = new byte[c_height][];
        private readonly byte[] m_rand = new byte[c_randSize];
        private readonly uint[] m_palette = new uint[256];
        private int m_randIdx;

        private readonly SKBitmap m_bitmap;
        public SKBitmap Bitmap => m_bitmap;

        public Fire()
        {
            for (int y = 0; y < c_height; y++)
                m_fire[y] = new byte[c_width];

            for (int x = 0; x < c_width; x++)
                m_fire[c_height - 1][x] = 255;

            var rnd = new Random(0xBEEF);
            rnd.NextBytes(m_rand);

            uint[] mainColors =
            [
                0x00000000,
                0xFF000040,
                (uint)new SKColor(75, 105, 47),
                (uint)new SKColor(106, 190, 48),
                (uint)new SKColor(153, 229, 80),
                0xFFFFFFFF
            ];

            for (int i = 0; i < 256; i++)
            {
                float pos = i * 5f / 256f;  // range (0,5)
                int idx = (int)Math.Floor(pos);
                float frac = pos - idx;
                uint c0 = mainColors[idx];
                uint c1 = mainColors[idx + 1];

                float a0 = (c0 >> 24) & 0xFF;
                float r0 = (c0 >> 16) & 0xFF;
                float g0 = (c0 >> 8) & 0xFF;
                float b0 = (c0) & 0xFF;

                float a1 = (c1 >> 24) & 0xFF;
                float r1 = (c1 >> 16) & 0xFF;
                float g1 = (c1 >> 8) & 0xFF;
                float b1 = (c1) & 0xFF;

                byte A = (byte)(a0 + (a1 - a0) * frac);
                byte R = (byte)(r0 + (r1 - r0) * frac);
                byte G = (byte)(g0 + (g1 - g0) * frac);
                byte B = (byte)(b0 + (b1 - b0) * frac);

                // store as 0xAARRGGBB (Skia BGRA8888 pixel layout is little‑endian)
                m_palette[i] = (uint)(A << 24 | R << 16 | G << 8 | B);
            }

            m_bitmap = new SKBitmap(c_width, c_height, SKColorType.Bgra8888, SKAlphaType.Premul);
        }

        public void Update()
        {
            if (m_randIdx > 0x4000_0000) m_randIdx = 0;

            for (int y = 1; y < c_height; y++)
            {
                byte[] row = m_fire[y];
                byte[] rowUp = m_fire[y - 1];

                for (int x = 0; x < c_width; x++)
                {
                    byte pix = row[x];
                    if (pix <= 8)
                    {
                        rowUp[x] = 0;
                        continue;
                    }

                    byte r = m_rand[(m_randIdx++) % c_randSize];
                    int dstX = x - (r & 3) + 1;
                    if (dstX < 0 || dstX >= c_width)
                    {
                        continue;
                    }

                    byte decay = (byte)(r % 7);
                    rowUp[dstX] = (byte)(pix - decay);
                }
            }
        }

        public void Render()
        {
            IntPtr ptr = m_bitmap.GetPixels();
            unsafe
            {
                uint* dst = (uint*)ptr.ToPointer();
                for (int y = 0; y < c_height; y++)
                {
                    byte[] row = m_fire[y];
                    for (int x = 0; x < c_width; x++)
                    {
                        *dst++ = m_palette[row[x]];
                    }
                }
            }
        }
    }

    private readonly Image m_bannerImg;
    private readonly Fire m_doomFire = new();

    private SKFont m_fntBig;

    public AboutDialog(Widget? parent = null) : base(parent)
    {
        m_fntBig = Application.CreateUIFont(SKFontStyleWeight.Normal, 2);

        using var bannerStream = PellychanResources.ResourceAssembly.GetManifestResourceStream("Pellychan.Resources.Images.banner.png");
        var img = SKImage.FromEncodedData(bannerStream);

        Layout = new HBoxLayout()
        {
        };

        m_bannerImg = new Image(this)
        {
            Bitmap = img,
            Width = img.Width,
            Height = img.Height,
            PreserveAspectRatio = true,

            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
        };

        /*
        new Label(this)
        {
            Position = new(4, 4),
            Text = "GPLv3",
        };
        */

        Resize(640, 480);
    }

    public new void OnPaint(SKCanvas canvas)
    {
        m_doomFire.Update();
        m_doomFire.Render();

        canvas.Clear(EffectivePalette.Get(ColorGroup.Active, ColorRole.Base).Darker(1.3f));

        canvas.DrawBitmap(m_doomFire.Bitmap, new SKRect(0, 0, Width, Height));

        using var paint = new SKPaint();
        paint.Color = Palette.Get(ColorRole.Text);

        var txt = "Copyright 2025 Pellyware";
        
        canvas.DrawText(txt, (Width / 2) - (m_fntBig.MeasureText(txt) / 2), m_fntBig.Size + 245, m_fntBig, paint);
    }

    public override void OnShown()
    {
        Title = "About Pellychan";
    }
}