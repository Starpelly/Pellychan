using SkiaSharp;

namespace Pellychan.GUI.Widgets
{
    public class Label : Widget, IPaintHandler
    {
        private SKFont m_font;

        private string m_text = string.Empty;
        public string Text
        {
            get => m_text;
            set
            {
                m_text = value;

                updateSize();
                Invalidate();
            }
        }

        public new SKPaint Paint { get; set; } = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true
        };

        public bool WordWrap { get; set; } = false;
        public bool ElideRight { get; set; } = false;
        public SKTextAlign HorizontalAlignment { get; set; } = SKTextAlign.Left;
        public SKFontMetrics FontMetrics;

        private int m_maxWidth = int.MaxValue;

        public Label(SKFont font, Widget? parent = null) : base(parent)
        {
            m_font = font;
        }

        public void OnPaint(SKCanvas canvas)
        {
            canvas.Save();

            Paint.Color = Application.Palette.Get(ColorRole.Text);

            // canvas.DrawRect(new SKRect(0, 0, Width, Height), new SKPaint() { Color = SKColors.Red });

            if (WordWrap)
            {
                var lines = breakLines(Text, m_maxWidth, m_font);
                float y = m_font.Size;

                foreach (var line in lines)
                {
                    int x = HorizontalAlignment switch
                    {
                        SKTextAlign.Left => 0,
                        SKTextAlign.Center => Width / 2,
                        SKTextAlign.Right => Width,
                        _ => 0
                    };

                    canvas.DrawText(line, new SKPoint(x, y), HorizontalAlignment, m_font, Paint);
                    y += m_font.Size + 2;
                }
            }
            else
            {
                string displayText = ElideRight ? elide(Text, m_maxWidth, m_font) : Text;

                int x = HorizontalAlignment switch
                {
                    SKTextAlign.Left => 0,
                    SKTextAlign.Center => Width / 2,
                    SKTextAlign.Right => Width,
                    _ => 0
                };

                float y = m_font.Size;
                canvas.DrawText(displayText, new SKPoint(x, y), m_font, Paint);
            }

            canvas.Restore();
        }

        private void updateSize()
        {
            if (string.IsNullOrEmpty(Text))
            {
                Resize(0, 0);
                return;
            }

            int width;
            int height;

            if (WordWrap)
            {
                var lines = breakLines(Text, m_maxWidth, m_font);
                width = m_maxWidth;
                height = (int)((lines.Count) * (m_font.Size + 2));
            }
            else
            {
                string displayText = ElideRight ? elide(Text, m_maxWidth, m_font) : Text;
                width = (int)m_font.MeasureText(displayText) + 2;
                height = (int)(m_font.Size + 2);
            }

            Resize(width, height);
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
    }
}
