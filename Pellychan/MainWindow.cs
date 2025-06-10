using Pellychan.Resources;
using SkiaSharp;

namespace Pellychan;

public class MainWindow : GUI.Widgets.MainWindow
{
    private readonly ChanClient m_chanClient = new();

    private readonly SKPaint m_labelPaint = new();
    private readonly SKPicture m_flag;
    private readonly SKFont m_font;

    public MainWindow()
    {
        m_chanClient.Boards = m_chanClient.GetBoardsAsync().GetAwaiter().GetResult();
        m_labelPaint.Color = SKColors.White;

        m_flag = Helpers.LoadSvgPicture($"Pellychan.Resources.Images.Flags.{Helpers.FlagURL("US")}")!;

        // using var fontStream = PellychanResources.ResourceAssembly.GetManifestResourceStream("Pellychan.Resources.Fonts.Roboto-VariableFont_wdth,wght.ttf");

        using var typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        m_font = new SKFont
        {
            Edging = SKFontEdging.SubpixelAntialias,
            Hinting = SKFontHinting.Full,
            Subpixel = true,
            Typeface = typeface,
            Size = 12 * 1.2f
        };
    }

    public override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        canvas.Clear(new(15, 15, 15, 255));

        // Helpers.DrawSvg(canvas, m_flag, new SKRect(0, 0, 256, 256));

        for (var i = 0; i < m_chanClient.Boards.Count; i++)
        {
            var board = m_chanClient.Boards[i];
            canvas.DrawText(board.Title, new SKPoint(16, (i * 16) + 16 + 8), m_font, m_labelPaint);
        }
    }
}