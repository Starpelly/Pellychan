using HtmlAgilityPack;
using SkiaSharp;

namespace Pellychan.GUI.Widgets;

public class Label : Widget, IPaintHandler
{
    private readonly int m_maxWidth = int.MaxValue;

    private readonly SKPaint m_paint = new()
    {
        Color = Application.Palette.Get(ColorRole.Text),
    };

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

    public class TextFragment
    {
        private string m_text;
        private string[] m_words;

        public string Text
        {
            get => m_text;
            set
            {
                m_text = value;
                m_words = m_text.Split(' ');
            }
        }
        public string[] Words => m_words;

        public SKColor TextColor { get; set; }
        public bool IsBold { get; set; } = false;
        public bool IsUnderline { get; set; } = false;
        public bool IsBig { get; set; } = false;

        public SKFont GetFont()
        {
            if (IsBold)
                return (IsBig) ? Application.DefaultFontBoldBig : Application.DefaultFontBold;
            else
                return Application.DefaultFont;
        }

        public float GetFontSize()
        {
            return GetFont().Size + ((IsUnderline) ? 1 : 0);
        }
    }
    private readonly List<TextFragment> m_textFragments = [];

    private string m_text = string.Empty;
    public string Text
    {
        get => m_text;
        set
        {
            m_text = value;

            parseHtml(value);
            updateSize();
            TriggerRepaint();
        }
    }

    public int LineSpacing { get; set; } = 4;

    public bool WordWrap { get; set; } = false;
    public bool ElideRight { get; set; } = false;

    public TextAnchor Anchor = TextAnchor.TopLeft;

    public Label(Widget? parent = null) : base(parent)
    {
    }

    public void OnPaint(SKCanvas canvas)
    {
        float x = 0, y = 0;

        float yStart = 0;

        if (m_textFragments.Count > 0)
        {
            var fontSize = m_textFragments[0].GetFontSize();
            y = fontSize;
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
                    yStart = ((Height - fontSize) / 2) - 2;
                    break;
                case TextAnchor.BottomLeft:
                case TextAnchor.BottomCenter:
                case TextAnchor.BottomRight:
                    yStart = Height - fontSize;
                    break;
            }
        }

        foreach (var frag in m_textFragments)
        {
            m_paint.Color = frag.TextColor;

            // @INVESTIGATE
            // This draws a line even when the text is split on different lines.
            // Hrmmmm
            if (frag.IsUnderline)
            {
                var realTextWidth = frag.GetFont().MeasureText(frag.Text);
                m_paint.IsAntialias = false;
                canvas.DrawLine(new SKPoint(x, y + yStart + 1), new SKPoint(x + realTextWidth, y + yStart + 1), m_paint);
            }

            foreach (var word in frag.Words)
            {
                if (word == "\n")
                {
                    x = 0;
                    y += frag.GetFontSize() + LineSpacing;
                    continue;
                }

                var textWidth = frag.GetFont().MeasureText(word + " ");
                if (WordWrap && x + textWidth > Width)
                {
                    x = 0;
                    y += frag.GetFontSize() + LineSpacing;
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

                canvas.DrawText(word + " ", x + xStart, y + yStart, frag.GetFont(), m_paint);

                x += textWidth;
            }
        }
    }

    public int MeasureHeightFromWidth(int width)
    {
        float x = 0;
        //float retHeight = m_font.Size + LineSpacing;
        float retHeight = 0;

        if (m_textFragments.Count > 0)
        {
            retHeight = m_textFragments[0].GetFontSize();
        }

        float lastLineDescent = 0;
        foreach (var frag in m_textFragments)
        {
            var words = frag.Text.Split(' ');

            foreach (var word in words)
            {
                if (word == "\n")
                {
                    x = 0;
                    retHeight += frag.GetFontSize() + LineSpacing;
                    continue;
                }

                var textWidth = frag.GetFont().MeasureText(word + " ");
                if (WordWrap && x + textWidth > width)
                {
                    x = 0;
                    retHeight += frag.GetFontSize() + LineSpacing;
                }

                x += textWidth;

            }
            lastLineDescent = frag.GetFont().Metrics.Descent;
        }

        return (int)(retHeight + lastLineDescent);
    }

    public (int, int) MeasureSizeFromText()
    {
        float maxLineWidth = 0;
        float currentLineWidth = 0;
        // float totalHeight = m_font.Size + LineSpacing;
        float totalHeight = 0;

        if (m_textFragments.Count > 0)
        {
            totalHeight = m_textFragments[0].GetFontSize();
        }

        void onNewLine(TextFragment frag)
        {
            maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);
            totalHeight += frag.GetFontSize() + LineSpacing;
            currentLineWidth = 0;
        }

        float lastLineDescent = 0;
        foreach (var frag in m_textFragments)
        {
            var words = frag.Text.Split(' ');

            foreach (var word in words)
            {
                if (word == "\n")
                {
                    onNewLine(frag);
                    continue;
                }

                var textWidth = frag.GetFont().MeasureText(word + " ");
                if (WordWrap && currentLineWidth + textWidth > m_maxWidth)
                {
                    onNewLine(frag);
                }

                currentLineWidth += textWidth;
            }
            lastLineDescent = frag.GetFont().Metrics.Descent;
        }

        maxLineWidth = Math.Max(maxLineWidth, currentLineWidth);

        int width = WordWrap ? Math.Min((int)maxLineWidth, m_maxWidth) : (int)maxLineWidth;
        int height = (int)(totalHeight + lastLineDescent);
        return (width, height);
    }

    #region Private methods

    private void parseHtml(string input)
    {
        m_textFragments.Clear();
        var doc = new HtmlDocument();
        // doc.LoadHtml($"<body>{input}</body>");
        doc.LoadHtml(input);

        foreach (var node in doc.DocumentNode.ChildNodes)
        {
            var text = System.Net.WebUtility.HtmlDecode(node.InnerText);
            var color = m_paint.Color;
            var bold = false;
            var underline = false;
            var big = false;

            switch (node.Name)
            {
                case "#text":
                    break;

                case "br":
                    text = "\n";
                    break;

                case "a":
                    switch (node.GetAttributeValue("class", ""))
                    {
                        case "quotelink":
                            color = SKColor.Parse("#5F89AC");
                            underline = true;
                            break;
                    }
                    break;

                case "span":

                    switch (node.GetAttributeValue("class", ""))
                    {
                        case "quote":
                            color = SKColor.Parse("#b5bd68");
                            break;
                        case "name":
                            // color = SKColor.Parse("#5F89AC");
                            bold = true;
                            break;
                        case "date":
                        case "postID":
                            color = color.WithAlpha(50);
                            // color = SKColor.Parse("#5F89AC").WithAlpha(150);
                            break;
                        case "header":
                            bold = true;
                            big = true;
                            break;
                    }
                    break;
            }

            m_textFragments.Add(new TextFragment { Text = text, TextColor = color, IsBold = bold, IsUnderline = underline, IsBig = big });
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