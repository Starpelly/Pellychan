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

        for (int i = 0; i < 2; i++)
        {
            int unicode = char.ConvertToUtf32(countryCode, i);
            emojiCodepoints[i] = (unicode + 127397).ToString("x").ToLowerInvariant();
        }

        var baseFileName = string.Join("-", emojiCodepoints);
        return $"{baseFileName}.svg";
    }
}