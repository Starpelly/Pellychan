using ExCSS;
using Pellychan.GUI.Widgets;
using Pellychan.Resources;
using SkiaSharp;
using System.Text;

namespace Pellychan;

public class MainWindow : GUI.Widgets.MainWindow
{
    private readonly ChanClient m_chanClient = new();

    private readonly SKPaint m_labelPaint = new();
    private readonly SKPicture m_flag;
    private readonly SKFont m_font;

    private List<Label> m_labels = [];

    public MainWindow()
    {
        // m_chanClient.Boards = m_chanClient.GetBoardsAsync().GetAwaiter().GetResult();
        m_labelPaint.Color = SKColors.White;

        m_flag = Helpers.LoadSvgPicture($"Pellychan.Resources.Images.Flags.{Helpers.FlagURL("US")}")!;

        using var fontStream = PellychanResources.ResourceAssembly.GetManifestResourceStream("Pellychan.Resources.Fonts.lucidagrande.ttf");

        // using var typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        using var typeface = SKTypeface.FromStream(fontStream);
        m_font = new SKFont
        {
            Edging = SKFontEdging.SubpixelAntialias,
            Hinting = SKFontHinting.Full,
            Subpixel = true,
            Typeface = typeface,
            Size = 13
        };

        /*
        void createLabel(string text, int x, int y)
        {
            var label = new Label(m_font)
            {
                Text = text,
                X = x,
                Y = y
            };
            this.AddChild(label);

            m_labels.Add(label);
        }

        for (int i = 0; i < m_chanClient.Boards.Count; i++)
        {
            var board = m_chanClient.Boards[i];

            createLabel(board.Title, 16, (i * 16) + 16);
        }
        for (var i = 0; i < 20; i++)
        createLabel("test", 0, 0);
        */

        AddChild(new Rect()
        {
            X = 16,
            Y = 16,
            Width = 100,
            Height = 100,
        });
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        canvas.Clear(new(15, 15, 15, 255));

        // Helpers.DrawSvg(canvas, m_flag, new SKRect(0, 0, 256, 256));
        
        /*
        for (int i = 0; i < m_chanClient.Boards.Count; i++)
        {
            var board = m_chanClient.Boards[i];
            var pos = new SKPoint(16, (i * 16) + 16 + 8);

            canvas.DrawText($"/{board.URL}/", pos, m_font, m_labelPaint);
            canvas.DrawText(board.Title, pos + new SKPoint(62, 0), m_font, m_labelPaint);
        }
        */
    }
}