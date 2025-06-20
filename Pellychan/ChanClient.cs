using ImageMagick;
using Newtonsoft.Json;
using Pellychan.API;
using Pellychan.API.Models;
using Pellychan.API.Responses;
using SkiaSharp;
using Thread = Pellychan.API.Models.Thread;

namespace Pellychan;

public class ChanClient
{
    private readonly HttpClient m_httpClient = new();

    public string CurrentBoard { get; set; }

    public BoardsResponse Boards;
    public CatalogResponse Catalog;

    public ChanClient()
    {
        // The 4chan API requires a UserAgent or else it won't work.
        m_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "My4ChanClient/1.0 (+https://github.com/Starpelly/pellychan)"
        );
    }

    public async Task<BoardsResponse> GetBoardsAsync()
    {
        var url = $"https://{Domains.API}/boards.json";
        var json = await m_httpClient.GetStringAsync(url);

        var result = JsonConvert.DeserializeObject<BoardsResponse>(json);
        return result;
    }

    public async Task<CatalogResponse> GetCatalogAsync()
    {
        var url = $"https://{Domains.API}/{CurrentBoard}/catalog.json";
        var json = await m_httpClient.GetStringAsync(url);

        var result = JsonConvert.DeserializeObject<List<CatalogPage>>(json);
        return new()
        {
            Pages = result!
        };
    }

    public async Task<Thread> GetThreadPostsAsync(string threadID)
    {
        var url = $"https://{Domains.API}/{CurrentBoard}/thread/{threadID}.json";
        var json = await m_httpClient.GetStringAsync(url);

        var result = JsonConvert.DeserializeObject<Thread>(json);
        return result;
    }

    public async Task<SKImage?> DownloadThumbnailAsync(long tim)
    {
        string url = $"https://{Domains.UserContent}/{CurrentBoard}/{tim}s.jpg";

        try
        {
            byte[] imageBytes = await m_httpClient.GetByteArrayAsync(url);
            using MemoryStream ms = new MemoryStream(imageBytes);
            return SKImage.FromEncodedData(ms); // Decode into SKBitmap
        }
        catch
        {
            return null; // Handle gracefully if image isn't available
        }
    }

    public async Task<SKImage?> DownloadAttachmentAsync(Post post)
    {
        string url = $"https://{Domains.UserContent}/{CurrentBoard}/{post.Tim}{post.Ext}";

        try
        {
            byte[] imageBytes = await m_httpClient.GetByteArrayAsync(url);
            using MemoryStream ms = new MemoryStream(imageBytes);
            return SKImage.FromEncodedData(ms); // Decode into SKBitmap
        }
        catch
        {
            return null; // Handle gracefully if image isn't available
        }
    }

    public void LoadThumbnail(Post post, Action<SKImage?> onComplete)
    {
        Task.Run(async () =>
        {
            var thumb = await DownloadThumbnailAsync((long)post.Tim!);
            onComplete?.Invoke(thumb);
        });
    }

    public void LoadThumbnail(CatalogThread post, Action<SKImage?> onComplete)
    {
        Task.Run(async () =>
        {
            var thumb = await DownloadThumbnailAsync((long)post.Tim!);
            onComplete?.Invoke(thumb);
        });
    }

    public void LoadAttachment(Post post, Action<SKImage?> onComplete)
    {
        Task.Run(async () =>
        {
            var attachment = await DownloadAttachmentAsync(post);
            onComplete?.Invoke(attachment);
        });
    }

    public class GifFrame
    {
        public SKImage Image;
        public int Delay; // in milliseconds
    }

    public async Task<List<GifFrame>> LoadGifFromUrlAsync(string url)
    {
        byte[] data = await m_httpClient.GetByteArrayAsync(url);

        var frames = new List<GifFrame>();
        using var collection = new MagickImageCollection(data);
        collection.Coalesce();

        foreach (var frame in collection)
        {
            using var ms = new MemoryStream();
            frame.Format = MagickFormat.Png; // Convert each frame to PNG for Skia
            frame.Write(ms);
            ms.Position = 0;

            frames.Add(new GifFrame
            {
                Image = SKImage.FromEncodedData(ms),
                Delay = (int)(frame.AnimationDelay * 10) // ms
            });
        }

        return frames;
    }
}