using Newtonsoft.Json;
using Pellychan.API.Models;

namespace Pellychan.API.Responses;

public struct BoardsResponse
{
    [JsonProperty("boards")]
    public List<Board> Boards;
}