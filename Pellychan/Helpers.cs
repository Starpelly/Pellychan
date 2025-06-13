using Pellychan.Resources;
using SkiaSharp;
using Svg.Skia;

namespace Pellychan;

public static class Helpers
{
    /// <summary>
    /// Gets the filename for a country flag based on country code.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public static string FlagURL(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            throw new ArgumentException("Country code must be exactly two characters.", nameof(countryCode));

        countryCode = countryCode.ToUpperInvariant();
        var emojiCodepoints = new string[2];

        for (var i = 0; i < 2; i++)
        {
            var unicode = char.ConvertToUtf32(countryCode, i);
            emojiCodepoints[i] = (unicode + 127397).ToString("x").ToLowerInvariant();
        }

        var baseFileName = string.Join("-", emojiCodepoints);
        return $"{baseFileName}.svg";
    }

    public static SKPicture? LoadSvgPicture(string resourceName)
    {
        var assembly = PellychanResources.ResourceAssembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;

        var svg = new SKSvg();
        svg.Load(stream);
        return svg.Picture;
    }

    public static void DrawSvg(SKCanvas canvas, SKPicture? picture, SKRect targetBounds)
    {
        if (picture == null) return;

        // Calculate scale
        var originalSize = picture.CullRect;
        var matrix = SKMatrix.CreateScale(
            targetBounds.Width / originalSize.Width,
            targetBounds.Height / originalSize.Height);

        canvas.Save();
        canvas.Translate(targetBounds.Left, targetBounds.Top);
        canvas.DrawPicture(picture, in matrix);
        canvas.Restore();
    }
}