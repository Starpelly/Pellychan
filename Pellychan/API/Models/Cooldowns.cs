using Newtonsoft.Json;

namespace Pellychan.API.Models;

public struct Cooldowns
{
    /// <summary>
    /// Seconds between creating new threads.
    /// </summary>
    [JsonProperty("threads")]
    public int Threads;

    /// <summary>
    /// Seconds between posting replies.
    /// </summary>
    [JsonProperty("replies")]
    public int Replies;

    /// <summary>
    /// Seconds between posting images.
    /// </summary>
    [JsonProperty("images")]
    public int Images;
}
