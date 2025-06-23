using HtmlAgilityPack;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class Label : Widget, IPaintHandler
{
    private SKFont m_font;

    public enum TextAnchor
    {
        TopLeft,
        TopRight,
        TopCenter,
        CenterLeft,
        CenterCenter,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
    }

    private string m_text = string.Empty;
    public string Text
    {
        get => m_text;
        set
        {
            m_text = value;

            parseHtml(value, m_font);
            updateSize();
            TriggerRepaint();
        }
    }

    private SKPaint m_paint { get; set; } = new SKPaint
    {
        Color = Application.Palette.Get(ColorRole.Text),
        IsAntialias = true
    };

    public const int LineSpacing = 4;

    public bool WordWrap { get; set; } = false;
    public bool ElideRight { get; set; } = false;
    public SKFontMetrics FontMetrics;

    public TextAnchor Anchor = TextAnchor.TopLeft;

    private List<TextFragment> m_textFragments = [];

    private int m_maxWidth = int.MaxValue;

    public class TextFragment
    {
        public string Text { get; set; } = "";

        public SKColor TextColor { get; set; }
        public bool IsBold { get; set; } = false;
    }

    public Label(SKFont font, Widget? parent = null) : base(parent)
    {
        m_font = font;
    }

    public void OnPaint(SKCanvas canvas)
    {
        float x = 0, y = m_font.Size;

        float yStart = 0;
        switch (Anchor)
        {
            case TextAnchor.TopLeft:
            case TextAnchor.TopCenter:
            case TextAnchor.TopRight:
                yStart = 0;
                break;
            case TextAnchor.CenterLeft:
            case TextAnchor.CenterCenter:
            case TextAnchor.CenterRight:
                yStart = ((Height - m_font.Size) / 2) - 2;
                break;
            case TextAnchor.BottomLeft:
            case TextAnchor.BottomCenter:
            case TextAnchor.BottomRight:
                yStart = Height - m_font.Size;
                break;
        }

        // canvas.DrawText(Text, new SKPoint(0, m_font.Size), m_font, m_paint);
        // return;

        foreach (var frag in m_textFragments)
        {
            var words = frag.Text.Split(' ');

            m_font.Embolden = frag.IsBold;
            m_paint.Color = frag.TextColor;

            foreach (var word in words)
            {
                if (word == "\n")
                {
                    x = 0;
                    y += m_font.Size + LineSpacing;
                    continue;
                }

                var textWidth = m_font.MeasureText(word + " ");
                if (WordWrap && x + textWidth > Width)
                {
                    x = 0;
                    y += m_font.Size + LineSpacing;
                }

                float xStart = 0;
                switch (Anchor)
                {
                    case TextAnchor.TopLeft:
                    case TextAnchor.CenterLeft:
                    case TextAnchor.BottomLeft:
                        xStart = 0;
                        break;
                    case TextAnchor.TopCenter:
                    case TextAnchor.CenterCenter:
                    case TextAnchor.BottomCenter:
                        xStart = (Width - textWidth) / 2;
                        break;
                    case TextAnchor.TopRight:
                    case TextAnchor.CenterRight:
                    case TextAnchor.BottomRight:
                        xStart = Width - textWidth;
                        break;
                }

                canvas.DrawText(word + " ", x + xStart, y + yStart, m_font, m_paint);
                x += textWidth;
            }
        }
    }

    public int MeasureHeightFromWidth(int width)
    {
        float x = 0;
        float retHeight = m_font.Size + LineSpacing;
        foreach (var frag in m_textFragments)
        {
            var words = frag.Text.Split(' ');

            foreach (var word in words)
            {
                if (word == "\n")
                {
                    x = 0;
                    retHeight += m_font.Size + LineSpacing;
                    continue;
                }

                var textWidth = m_font.MeasureText(word + " ");
                if (WordWrap && x + textWidth > width)
                {
                    x = 0;
                    retHeight += m_font.Size + LineSpacing;
                }

                x += textWidth;
            }
        }
        return (int)retHeight;
    }

    public (int, int) MeasureSizeFromText()
    {
        float maxLineWidth = 0;
        float currentLineWidth = 0;
        float totalHeight = m_font.Size + LineSpacing;

        void onNewLine()
        {
            maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);
            totalHeight += m_font.Size + LineSpacing;
            currentLineWidth = 0;
        }

        foreach (var frag in m_textFragments)
        {
            var words = frag.Text.Split(' ');

            foreach (var word in words)
            {
                if (word == "\n")
                {
                    onNewLine();
                    continue;
                }

                var textWidth = m_font.MeasureText(word + " ");
                if (WordWrap && currentLineWidth + textWidth > m_maxWidth)
                {
                    onNewLine();
                }

                currentLineWidth += textWidth;
            }
        }

        maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);

        int width = WordWrap ? Math.Min((int)maxLineWidth, m_maxWidth) : (int)maxLineWidth;
        int height = (int)totalHeight;
        return (width, height);
    }

    #region Private methods

    public void parseHtml(string input, SKFont font)
    {
        m_textFragments.Clear();
        var doc = new HtmlDocument();
        doc.LoadHtml($"<body>{input}</body>");

        foreach (var node in doc.DocumentNode.SelectSingleNode("//body").ChildNodes)
        {
            var color = m_paint.Color;
            var bold = false;

            switch (node.Name)
            {
                case "#text":
                    m_textFragments.Add(new TextFragment { Text = System.Net.WebUtility.HtmlDecode(node.InnerText), TextColor = color });
                    break;

                case "br":
                    m_textFragments.Add(new TextFragment { Text = "\n", TextColor = color });
                    break;

                case "a":
                    switch (node.GetAttributeValue("class", ""))
                    {
                        case "quotelink":
                            color = SKColor.Parse("#5F89AC");
                            break;
                    }
                    m_textFragments.Add(new TextFragment { Text = System.Net.WebUtility.HtmlDecode(node.InnerText), TextColor = color, IsBold = bold });
                    break;

                case "span":

                    switch (node.GetAttributeValue("class", ""))
                    {
                        case "quote":
                            color = SKColor.Parse("#b5bd68");
                            break;
                        case "name":
                            // color = SKColor.Parse("#5F89AC");
                            // bold = true;
                            break;
                        case "date":
                            color = color.WithAlpha(50);
                            break;
                        case "postID":
                            color = SKColor.Parse("#5F89AC").WithAlpha(150);
                            break;
                    }

                    m_textFragments.Add(new TextFragment { Text = System.Net.WebUtility.HtmlDecode(node.InnerText), TextColor = color, IsBold = bold });
                    break;
            }
        }
    }

    private void updateSize()
    {
        if (string.IsNullOrEmpty(m_text))
        {
            Resize(0, 0);
            return;
        }

        var res = MeasureSizeFromText();
        Resize(res.Item1, res.Item2);

        /*
        float maxLineWidth = 0;
        float currentLineWidth = 0;
        float totalHeight = m_font.Size;
        float spaceWidth = m_font.Size / 4; // Rough estimate

        foreach (var frag in m_textFragments)
        {
            var words = frag.Text.Split(' ');

            foreach (var word in words)
            {
                if (word == "\n")
                {
                    maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);
                    currentLineWidth = 0;
                    totalHeight += m_font.Size + LineSpacing;
                    continue;
                }

                string displayWord = word + " ";
                // float wordWidth = frag.Paint.MeasureText(displayWord);
                float wordWidth = Application.DefaultFont.MeasureText(displayWord);

                if (WordWrap && currentLineWidth + wordWidth > m_maxWidth)
                {
                    maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);
                    currentLineWidth = wordWidth;
                    totalHeight += m_font.Size + LineSpacing;
                }
                else
                {
                    currentLineWidth += wordWidth;
                }
            }
        }

        maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);
        int width = WordWrap ? Math.Min((int)maxLineWidth, m_maxWidth) : (int)maxLineWidth;
        int height = (int)totalHeight + 4;

        Resize(width, height);
        */
    }

    // Truncate text to fit with "..." at the end
    private static string elide(string text, int maxWidth, SKFont font)
    {
        string ellipsis = "...";
        float ellipsisWidth = font.MeasureText(ellipsis);
        if (font.MeasureText(text) <= maxWidth)
            return text;

        for (int i = text.Length - 1; i >= 0; i--)
        {
            string sub = text.Substring(0, i);
            if (font.MeasureText(sub) + ellipsisWidth <= maxWidth)
                return sub + ellipsis;
        }
        return ellipsis;
    }

    // Basic word wrapping
    private static List<string> breakLines(string text, int maxWidth, SKFont font)
    {
        var lines = new List<string>();
        var words = text.Split(' ');
        string line = "";

        foreach (var word in words)
        {
            string testLine = string.IsNullOrEmpty(line) ? word : line + " " + word;
            if (font.MeasureText(testLine) <= maxWidth)
            {
                line = testLine;
            }
            else
            {
                if (!string.IsNullOrEmpty(line)) lines.Add(line);
                line = word;
            }
        }

        if (!string.IsNullOrEmpty(line))
            lines.Add(line);

        return lines;
    }

    #endregion
}