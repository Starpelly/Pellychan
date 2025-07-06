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

        // LoadCatalog("v");
        // LoadThread("714085510");
        MainWindow.Show();
        MainWindow.T();

        // LoadCatalog("g");
        // LoadThread("105756382");

        app.Run();
    }

    public static void LoadCatalog(string board)
    {
        ChanClient.CurrentBoard = board;
        ChanClient.Catalog = ChanClient.GetCatalogAsync().GetAwaiter().GetResult();

        MainWindow.LoadBoardCatalog(board);
        MainWindow.SetWindowTitle($"Pellychan - /{board}/");

        MainWindow.T();
    }

    public static void LoadThread(string threadID)
    {
        ChanClient.CurrentThread = ChanClient.GetThreadPostsAsync(threadID).GetAwaiter().GetResult();

        MainWindow.LoadThreadPosts(threadID);
        MainWindow.SetWindowTitle($"Pellychan - /{ChanClient.CurrentBoard}/{threadID}/ - {ChanClient.CurrentThread.Posts[0].Sub}");
    }
}