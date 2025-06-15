using SkiaSharp;

namespace Pellychan.GUI.Styles.Phantom;

/// <summary>
/// C# port of HSLuv-C: Human-friendly HSL
/// <http://github.com/hsluv/hsluv-c>
/// <http://www.hsluv.org/>
/// </summary>
internal static class Hsluv
{
    struct Triplet
    {
        public double a, b, c;
        public Triplet(double a, double b, double c) => (this.a, this.b, this.c) = (a, b, c);
    }

    struct Bounds
    {
        public double a, b;
    }

    static readonly Triplet[] m = [
            new( 3.2409699419045213, -1.5373831775700935, -0.49861076029300328),
            new(-0.9692436362808798,  1.8759675015077207,  0.041555057407175612),
            new( 0.0556300796969936, -0.20397695888897656, 1.0569715142428786)
        ];

    static readonly Triplet[] m_inv = [
            new(0.4123907992659595, 0.35758433938387796, 0.18048078840183429),
            new(0.21263900587151036, 0.71516867876775593, 0.072192315360733715),
            new(0.01933081871559185, 0.11919477979462599, 0.95053215224966058)
        ];

    const double ref_u = 0.1978300066428368;
    const double ref_v = 0.468319994938791;
    const double kappa = 903.2962962962963;
    const double epsilon = 0.00885645167903563;

    static void GetBounds(double l, Bounds[] bounds)
    {
        double tl = l + 16.0;
        double sub1 = tl * tl * tl / 1560896.0;
        double sub2 = sub1 > epsilon ? sub1 : l / kappa;

        for (int channel = 0; channel < 3; channel++)
        {
            double m1 = m[channel].a;
            double m2 = m[channel].b;
            double m3 = m[channel].c;

            for (int t = 0; t < 2; t++)
            {
                double top1 = (284517.0 * m1 - 94839.0 * m3) * sub2;
                double top2 = (838422.0 * m3 + 769860.0 * m2 + 731718.0 * m1) * l * sub2 - 769860.0 * t * l;
                double bottom = (632260.0 * m3 - 126452.0 * m2) * sub2 + 126452.0 * t;

                bounds[channel * 2 + t].a = top1 / bottom;
                bounds[channel * 2 + t].b = top2 / bottom;
            }
        }
    }

    static double RayLengthUntilIntersect(double theta, Bounds line) =>
        line.b / (Math.Sin(theta) - line.a * Math.Cos(theta));

    static double MaxChromaForLh(double l, double h)
    {
        double minLen = double.MaxValue;
        double hRad = h * Math.PI / 180.0;
        Bounds[] bounds = new Bounds[6];
        GetBounds(l, bounds);
        foreach (var bound in bounds)
        {
            double len = RayLengthUntilIntersect(hRad, bound);
            if (len >= 0 && len < minLen) minLen = len;
        }
        return minLen;
    }

    static double DotProduct(Triplet t1, Triplet t2) =>
        t1.a * t2.a + t1.b * t2.b + t1.c * t2.c;

    static void XyzToRgb(ref Triplet t)
    {
        double r = DotProduct(m[0], t);
        double g = DotProduct(m[1], t);
        double b = DotProduct(m[2], t);
        t = new Triplet(r, g, b);
    }

    static void RgbToXyz(ref Triplet t)
    {
        Triplet rgbl = t;
        double x = DotProduct(m_inv[0], rgbl);
        double y = DotProduct(m_inv[1], rgbl);
        double z = DotProduct(m_inv[2], rgbl);
        t = new Triplet(x, y, z);
    }

    static double YToL(double y) =>
        y <= epsilon ? y * kappa : 116.0 * Math.Cbrt(y) - 16.0;

    static double LToY(double l)
    {
        if (l <= 8.0)
            return l / kappa;
        double x = (l + 16.0) / 116.0;
        return x * x * x;
    }

    static void XyzToLuv(ref Triplet t)
    {
        double denom = t.a + 15.0 * t.b + 3.0 * t.c;
        if (denom <= 1e-8)
        {
            t = new Triplet(0, 0, 0);
            return;
        }

        double u = 4.0 * t.a / denom;
        double v = 9.0 * t.b / denom;
        double l = YToL(t.b);

        double uu = 13.0 * l * (u - ref_u);
        double vv = 13.0 * l * (v - ref_v);
        t = new Triplet(l, l < 1e-8 ? 0 : uu, l < 1e-8 ? 0 : vv);
    }

    static void LuvToXyz(ref Triplet t)
    {
        if (t.a <= 1e-8)
        {
            t = new Triplet(0, 0, 0);
            return;
        }

        double u = t.b / (13.0 * t.a) + ref_u;
        double v = t.c / (13.0 * t.a) + ref_v;
        double y = LToY(t.a);
        double x = -(9.0 * y * u) / ((u - 4.0) * v - u * v);
        double z = (9.0 * y - 15.0 * v * y - v * x) / (3.0 * v);
        t = new Triplet(x, y, z);
    }

    static void LuvToLch(ref Triplet t)
    {
        double c = Math.Sqrt(t.b * t.b + t.c * t.c);
        double h = c < 1e-8 ? 0 : Math.Atan2(t.c, t.b) * 180.0 / Math.PI;
        if (h < 0) h += 360.0;
        t = new Triplet(t.a, c, h);
    }

    static void LchToLuv(ref Triplet t)
    {
        double hRad = t.c * Math.PI / 180.0;
        t = new Triplet(t.a, Math.Cos(hRad) * t.b, Math.Sin(hRad) * t.b);
    }

    static void HsluvToLch(ref Triplet t)
    {
        double h = t.a, s = t.b, l = t.c;
        double c = l > 99.9999999 || l < 1e-8 ? 0.0 : MaxChromaForLh(l, h) * s / 100.0;
        t = new Triplet(l, c, s < 1e-8 ? 0.0 : h);
    }

    static void LchToHsluv(ref Triplet t)
    {
        double l = t.a, c = t.b, h = t.c;
        double s = l > 99.9999999 || l < 1e-8 ? 0.0 : c / MaxChromaForLh(l, h) * 100.0;
        t = new Triplet(c < 1e-8 ? 0.0 : h, s, l);
    }

    public static void HsluvToRgb(double h, double s, double l, out double r, out double g, out double b)
    {
        Triplet t = new(h, s, l);
        HsluvToLch(ref t);
        LchToLuv(ref t);
        LuvToXyz(ref t);
        XyzToRgb(ref t);
        (r, g, b) = (t.a, t.b, t.c);
    }

    public static void RgbToHsluv(double r, double g, double b, out double h, out double s, out double l)
    {
        Triplet t = new(r, g, b);
        RgbToXyz(ref t);
        XyzToLuv(ref t);
        LuvToLch(ref t);
        LchToHsluv(ref t);
        (h, s, l) = (t.a, t.b, t.c);
    }

    public static double LinearOfSrgb(double x) =>
        x < 0.0404482362771082 ? x / 12.92 : Math.Pow((x + 0.055) / 1.055, 2.4);

    public static double SrgbOfLinear(double x) =>
        x < 0.00313066844250063 ? x * 12.92 : Math.Pow(x, 1.0 / 2.4) * 1.055 - 0.055;
}

public struct Rgb
{
    public double r, g, b;
    public Rgb(double r, double g, double b) => (this.r, this.g, this.b) = (r, g, b);

    public static Rgb Lerp(Rgb x, Rgb y, double a) =>
        new(
            (1.0 - a) * x.r + a * y.r,
            (1.0 - a) * x.g + a * y.g,
            (1.0 - a) * x.b + a * y.b
        );
}

public struct Hsl
{
    public double h, s, l;
    public Hsl(double h, double s, double l) => (this.h, this.s, this.l) = (h, s, l);
}

public static class SkiaSharpHelpers
{
    /// <summary>
    /// Convert SKColor to linear-light Rgb (each channel in [0,1]).
    /// </summary>
    public static Rgb ToLinearRgb(SKColor c)
    {
        double r = Hsluv.LinearOfSrgb(c.Red / 255.0);
        double g = Hsluv.LinearOfSrgb(c.Green / 255.0);
        double b = Hsluv.LinearOfSrgb(c.Blue / 255.0);
        return new Rgb(r, g, b);
    }

    /// <summary>
    /// Convert linear-light Rgb to an Skia SKColor.
    /// </summary>
    public static SKColor FromLinearRgb(Rgb rgb, byte? alpha = null)
    {
        byte a = alpha ?? 255;
        byte r = (byte)Math.Round(Hsluv.SrgbOfLinear(rgb.r) * 255);
        byte g = (byte)Math.Round(Hsluv.SrgbOfLinear(rgb.g) * 255);
        byte b = (byte)Math.Round(Hsluv.SrgbOfLinear(rgb.b) * 255);
        return new SKColor(r, g, b, a);
    }

    /// <summary>
    /// Convert SKColor to HSLuv.
    /// </summary>
    public static Hsl ToHsluv(SKColor c)
    {
        var lin = ToLinearRgb(c);
        Hsluv.RgbToHsluv(lin.r, lin.g, lin.b, out double h, out double s, out double l);
        return new Hsl(h, s / 100.0, l / 100.0);
    }

    /// <summary>
    /// Convert HSLuv to SKColor.
    /// </summary>
    public static SKColor FromHsluv(Hsl hsl, byte? alpha = null)
    {
        // s and l in [0,1] → scale to [0,100]
        Hsluv.HsluvToRgb(hsl.h, hsl.s * 100.0, hsl.l * 100.0, out double r, out double g, out double b);
        return FromLinearRgb(new Rgb(r, g, b), alpha);
    }

    /// <summary>
    /// Linear interpolate between two SKColors in HSLuv space.
    /// </summary>
    public static SKColor LerpHsluv(SKColor a, SKColor b, double t, byte? alpha = null)
    {
        var ha = ToHsluv(a);
        var hb = ToHsluv(b);

        // Simple lerp on h, s, l
        double h = (1 - t) * ha.h + t * hb.h;
        double s = (1 - t) * ha.s + t * hb.s;
        double l = (1 - t) * ha.l + t * hb.l;

        return FromHsluv(new Hsl(h, s, l), alpha);
    }
}