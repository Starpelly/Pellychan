using Newtonsoft.Json;
using Pellychan.API;
using Pellychan.API.Models;
using Pellychan.API.Responses;

namespace Pellychan;

public class ChanClient
{
    private readonly HttpClient m_httpClient = new();

    public ChanClient()
    {
        // The 4chan API requires a UserAgent or else it won't work.
        m_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "My4ChanClient/1.0 (+https://github.com/Starpelly/pellychan)"
        );
    }

    public async Task<List<Board>> GetBoardsAsync()
    {
        var url = $"https://{Domains.API}/boards.json";
        var json = await m_httpClient.GetStringAsync(url);

        var result = JsonConvert.DeserializeObject<BoardsResponse>(json);
        return result.Boards ?? [];
    }
}