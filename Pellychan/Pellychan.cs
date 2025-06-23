using Pellychan.GUI;
using Pellychan.Resources;
using SkiaSharp;

namespace Pellychan;

public class Pellychan
{
    private static Pellychan s_instance = new();
    private readonly ChanClient m_chanClient = new();

    public static PellychanWindow MainWindow { get; private set; }
    public static ChanClient ChanClient => s_instance.m_chanClient;

    #region Resources

    private SKFont? m_fontIcon;
    public static SKFont FontIcon => s_instance.m_fontIcon!;

    #endregion

    public Pellychan()
    {
        if (s_instance != null)
            throw new Exception("There can only be one Pellychan instance!");
        s_instance = this;

        m_chanClient.Boards = m_chanClient.GetBoardsAsync().GetAwaiter().GetResult();

        InitializeFonts();
    }

    public static void Start()
    {
        using var app = new Application();

        MainWindow = new PellychanWindow();
        MainWindow.CreateWinID();

        MainWindow.SetWindowTitle("Pellychan");
        MainWindow.Resize(1280, 720);

        // Main Window Icon
        using var iconStream = PellychanResources.ResourceAssembly.GetManifestResourceStream("Pellychan.Resources.Images.4channy.ico");
        MainWindow.SetIconFromStream(iconStream!);

        MainWindow.Show();
        LoadCatalog("g");

        app.Run();
    }

    public static void LoadCatalog(string board)
    {
        MainWindow.LoadBoardCatalog(board);
    }

    public static void LoadThread(string threadID)
    {
        MainWindow.LoadThreadPosts(threadID);
    }

    private void InitializeFonts()
    {
        using var iconsStream = PellychanResources.ResourceAssembly.GetManifestResourceStream("Pellychan.Resources.Fonts.MaterialIconsRound-Regular.otf");
        using var iconsTypeface = SKTypeface.FromStream(iconsStream);

        const int dpi = 96;
        const float pixelsPerPoint = dpi / 72.0f;
        const float skiaFontSize = 12 * pixelsPerPoint;

        m_fontIcon = new SKFont
        {
            Edging = SKFontEdging.SubpixelAntialias,
            Hinting = SKFontHinting.Slight,
            Subpixel = true,
            Typeface = iconsTypeface,
            Size = skiaFontSize
        };
    }
}