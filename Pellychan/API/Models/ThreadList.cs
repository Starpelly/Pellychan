using Newtonsoft.Json;

namespace Pellychan.API.Models;

public struct ThreadList
{
    public struct Thread
    {
        /// <summary>
        /// The OP ID of a thread.
        /// </summary>
        [JsonProperty("no")]
        public int ID { get; set; }

        /// <summary>
        /// The UNIX timestamp marking the last time the thread was modified.
        /// (post added/modified/deleted, thread closed/sticky settings modified)
        /// </summary>
        [JsonProperty("last_modified")]
        public int LastModified { get; set; }

        /// <summary>
        /// A numeric count of the number of replies in the thread.
        /// </summary>
        [JsonProperty("replies")]
        public int Replies { get; set; }
    }

    [JsonProperty("page")]
    public int Page { get; set; }

    [JsonProperty("threads")]
    public List<Thread> Threads { get; set; }
}