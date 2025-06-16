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

    public BoardsResponse Boards;

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

    public async Task<Thread> GetThreadAsync(string board, string post)
    {
        var url = $"https://{Domains.API}/{board}/thread/{post}.json";
        var json = await m_httpClient.GetStringAsync(url);

        var result = JsonConvert.DeserializeObject<Thread>(json);
        return result;
    }

    public async Task<SKBitmap?> DownloadThumbnailAsync(string board, long tim)
    {
        string url = $"https://{Domains.UserContent}/{board}/{tim}s.jpg";

        try
        {
            byte[] imageBytes = await m_httpClient.GetByteArrayAsync(url);
            using MemoryStream ms = new MemoryStream(imageBytes);
            return SKBitmap.Decode(ms); // Decode into SKBitmap
        }
        catch
        {
            return null; // Handle gracefully if image isn't available
        }
    }

    public void LoadThumbnail(string board, Post post, int storeIndex, Action<SKBitmap?, int> onComplete)
    {
        Task.Run(async () =>
        {
            var thumb = await DownloadThumbnailAsync(board, (long)post.Tim!);
            onComplete?.Invoke(thumb, storeIndex);
        });
    }
}