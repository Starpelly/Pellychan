using Pellychan.GUI;
using Pellychan.Resources;
using Pellychan.Widgets;

namespace Pellychan;

public static class Pellychan
{
    public static Settings Settings { get; private set; }

    public static PellychanWindow MainWindow { get; private set; }
    public static ChanClient ChanClient { get; private set; }

    private static void init()
    {
        // Load settings first
        Settings = Settings.Load();

        ChanClient = new();
        ChanClient.Boards = ChanClient.GetBoardsAsync().GetAwaiter().GetResult();
    }

    public static void Start()
    {
        init();

        using var app = new Application();

        MainWindow = new PellychanWindow();
        MainWindow.CreateWinID();

        MainWindow.SetWindowTitle("Pellychan");
        MainWindow.Resize(1280, 720);

        // Main Window Icon
        using var iconStream = PellychanResources.ResourceAssembly.GetManifestResourceStream("Pellychan.Resources.Images.4channy.ico");
        MainWindow.SetIconFromStream(iconStream!);

        MainWindow.Show();
        // LoadCatalog("g");
        // LoadThread("105724992");

        app.Run();
    }

    public static void LoadCatalog(string board)
    {
        MainWindow.LoadBoardCatalog(board);
        MainWindow.SetWindowTitle($"Pellychan - /{board}/");
    }

    public static void LoadThread(string threadID)
    {
        MainWindow.LoadThreadPosts(threadID);
        MainWindow.SetWindowTitle($"Pellychan - /{ChanClient.CurrentBoard}/{threadID}/ - {ChanClient.CurrentThread.Posts[0].Sub}");
    }
}