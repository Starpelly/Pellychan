using Newtonsoft.Json;

namespace Pellychan.API.Models;

/// <summary>
/// A full thread, consisting of the OP and all replies.
/// </summary>
public struct Thread
{
    [JsonProperty("posts")]
    public List<Post> Posts;
}

/// <summary>
/// A single post within a thread, including the OP.
/// </summary>
public struct Post
{
    [JsonProperty("no")]
    public int No;

    [JsonProperty("resto")]
    public int Resto;

    [JsonProperty("sticky")]
    public int? Sticky;

    [JsonProperty("closed")]
    public int? Closed;

    [JsonProperty("now")]
    public string Now;

    [JsonProperty("time")]
    public int Time;

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("trip")]
    public string Trip;

    [JsonProperty("id")]
    public string Id;

    [JsonProperty("capcode")]
    public string Capcode;

    [JsonProperty("country")]
    public string Country;

    [JsonProperty("country_name")]
    public string CountryName;

    [JsonProperty("board_flag")]
    public string BoardFlag;

    [JsonProperty("flag_name")]
    public string FlagName;

    [JsonProperty("sub")]
    public string Sub;

    [JsonProperty("com")]
    public string Com;

    /// <summary>
    /// Unix timestamp + microtime that an image was uploaded
    /// This can also be used to grab the image attached to the post.
    /// </summary>
    [JsonProperty("tim")]
    public long? Tim;

    [JsonProperty("filename")]
    public string Filename;

    [JsonProperty("ext")]
    public string Ext;

    [JsonProperty("fsize")]
    public int? Fsize;

    [JsonProperty("md5")]
    public string Md5;

    [JsonProperty("w")]
    public int? W;

    [JsonProperty("h")]
    public int? H;

    [JsonProperty("tn_w")]
    public int? TnW;

    [JsonProperty("tn_h")]
    public int? TnH;

    [JsonProperty("filedeleted")]
    public int? FileDeleted;

    [JsonProperty("spoiler")]
    public int? Spoiler;

    [JsonProperty("custom_spoiler")]
    public int? CustomSpoiler;

    [JsonProperty("replies")]
    public int? Replies;

    [JsonProperty("images")]
    public int? Images;

    [JsonProperty("bumplimit")]
    public int? BumpLimit;

    [JsonProperty("imagelimit")]
    public int? ImageLimit;

    [JsonProperty("tag")]
    public string Tag;

    [JsonProperty("semantic_url")]
    public string SemanticUrl;

    [JsonProperty("since4pass")]
    public int? Since4Pass;

    [JsonProperty("unique_ips")]
    public int? UniqueIps;

    [JsonProperty("m_img")]
    public int? MobileImage;

    [JsonProperty("archived")]
    public int? Archived;

    [JsonProperty("archived_on")]
    public int? ArchivedOn;
}