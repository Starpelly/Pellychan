using Newtonsoft.Json;
using Pellychan.API;
using Pellychan.API.Models;
using Pellychan.API.Responses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System.Runtime.InteropServices;
using Thread = Pellychan.API.Models.Thread;

namespace Pellychan;

public class ChanClient
{
    private readonly HttpClient m_httpClient = new();
    private readonly SemaphoreSlim m_throttler = new(8); // 8 concurrent downloads

    public string CurrentBoard { get; set; }
    public Thread CurrentThread { get; set; }

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
            using var ms = new MemoryStream(imageBytes);
            return SKImage.FromEncodedData(ms); // Decode into SKBitmap
        }
        catch
        {
            return null; // Handle gracefully if image isn't available
        }
    }

    public async Task DownloadAttachmentAsync(Post post, Action<SKImage?> onComplete)
    {
        string url = $"https://{Domains.UserContent}/{CurrentBoard}/{post.Tim}{post.Ext}";

        try
        {
            byte[] imageBytes = await m_httpClient.GetByteArrayAsync(url);
            using var ms = new MemoryStream(imageBytes);
            var ret = SKImage.FromEncodedData(ms); // Decode into SKBitmap
            onComplete.Invoke(ret);
        }
        catch
        {
            onComplete?.Invoke(null);
            // return null; // Handle gracefully if image isn't available
        }
    }

    public async Task LoadThumbnailsAsync(IEnumerable<long> imageIds, Action<long, SKImage?> onComplete)
    {
        var tasks = imageIds.Select(async tim =>
        {
            await m_throttler.WaitAsync();
            try
            {
                var img = await DownloadThumbnailAsync(tim);

                if (img != null)
                {
                    onComplete.Invoke(tim, img);
                }
            }
            finally
            {
                m_throttler.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    public class GifFrame
    {
        public required SKImage Image;
        public int Delay; // in milliseconds
    }

    public async Task<List<GifFrame>> LoadGifFromUrlAsync(string url)
    {
        byte[] data = await m_httpClient.GetByteArrayAsync(url);
        using var stream = new MemoryStream(data);

        var frames = new List<GifFrame>();

        using var image = Image.Load<Rgba32>(stream);

        foreach (var frame in image.Frames)
        {
            // Delay (10ms units)
            int delay = 100;
            if (frame.Metadata.TryGetGifMetadata(out var gifMeta))
            {
                delay = gifMeta.FrameDelay * 10; // 1 unit = 10 ms
            }

            using var skBitmap = new SKBitmap(frame.Width, frame.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            frame.CopyPixelDataTo(MemoryMarshal.Cast<byte, Rgba32>(skBitmap.GetPixelSpan()));

            frames.Add(new GifFrame
            {
                Image = SKImage.FromBitmap(skBitmap),
                Delay = delay
            });
        }

        return frames;
    }
}