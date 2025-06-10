using Newtonsoft.Json;

namespace Pellychan.API.Models;

public struct Board
{
    /// <summary>
    /// The directory the board is located in.
    /// </summary>
    [JsonProperty("board")]
    public string URL;

    /// <summary>
    /// The readable title at the top of the board.
    /// </summary>
    [JsonProperty("title")]
    public string Title;

    /// <summary>
    /// Is the board worksafe.
    /// </summary>
    [JsonProperty("ws_board")]
    public int Worksafe;

    /// <summary>
    /// How many threads are on a single index page.
    /// </summary>
    [JsonProperty("per_page")]
    public int ThreadsPerPage;

    /// <summary>
    /// How many index pages does the board have.
    /// </summary>
    [JsonProperty("pages")]
    public int PageCount;

    /// <summary>
    /// Maximum file size allowed for non-webm attachments (in KB).
    /// </summary>
    [JsonProperty("max_filesize")]
    public int MaxFilesizeKB;

    /// <summary>
    /// Maximum file size allowed for .webm attachments (in KB).
    /// </summary>
    [JsonProperty("max_webm_filesize")]
    public int MaxWebmFilesizeKB;

    /// <summary>
    /// Maximum number of characters allowed in a post comment.
    /// </summary>
    [JsonProperty("max_comment_chars")]
    public int MaxCommentChars;

    /// <summary>
    /// Maximum duration of a .webm attachment (in seconds).
    /// </summary>
    [JsonProperty("max_webm_duration")]
    public int MaxWebmDuration;

    /// <summary>
    /// Maximum number of replies allowed to a thread before it stops bumping.
    /// </summary>
    [JsonProperty("bump_limit")]
    public int BumpLimit;

    /// <summary>
    /// Maximum number of image replies per thread before they are discarded.
    /// </summary>
    [JsonProperty("image_limit")]
    public int ImageLimit;

    /// <summary>
    /// Cooldown information for posting.
    /// </summary>
    [JsonProperty("cooldowns")]
    public Cooldowns Cooldowns;

    /// <summary>
    /// SEO meta description content for a board.
    /// </summary>
    [JsonProperty("meta_description")]
    public string MetaDescription;

    /// <summary>
    /// Are spoilers enabled.
    /// </summary>
    [JsonProperty("spoilers")]
    public int? Spoilers;

    /// <summary>
    /// Number of custom spoilers.
    /// </summary>
    [JsonProperty("custom_spoilers")]
    public int? CustomSpoilers;

    /// <summary>
    /// Are archives enabled for the board.
    /// </summary>
    [JsonProperty("is_archived")]
    public int? IsArchived;

    /// <summary>
    /// Array of flag codes mapped to flag names.
    /// </summary>
    [JsonProperty("board_flags")]
    public Dictionary<string, string>? BoardFlags;

    /// <summary>
    /// Are flags showing poster's country enabled.
    /// </summary>
    [JsonProperty("country_flags")]
    public int? CountryFlags;

    /// <summary>
    /// Are poster ID tags enabled.
    /// </summary>
    [JsonProperty("user_ids")]
    public int? UserIds;

    /// <summary>
    /// Can users submit drawings via Oekaki app.
    /// </summary>
    [JsonProperty("oekaki")]
    public int? Oekaki;

    /// <summary>
    /// Can users submit sjis drawings.
    /// </summary>
    [JsonProperty("sjis_tags")]
    public int? SjisTags;

    /// <summary>
    /// Supports code syntax highlighting with [code] tags.
    /// </summary>
    [JsonProperty("code_tags")]
    public int? CodeTags;

    /// <summary>
    /// Supports TeX-style [math] and [eqn] tags.
    /// </summary>
    [JsonProperty("math_tags")]
    public int? MathTags;

    /// <summary>
    /// Is image posting disabled.
    /// </summary>
    [JsonProperty("text_only")]
    public int? TextOnly;

    /// <summary>
    /// Is the name field disabled on the board.
    /// </summary>
    [JsonProperty("forced_anon")]
    public int? ForcedAnon;

    /// <summary>
    /// Are webms with audio allowed.
    /// </summary>
    [JsonProperty("webm_audio")]
    public int? WebmAudio;

    /// <summary>
    /// Do OPs require a subject.
    /// </summary>
    [JsonProperty("require_subject")]
    public int? RequireSubject;

    /// <summary>
    /// Minimum image width in pixels.
    /// </summary>
    [JsonProperty("min_image_width")]
    public int? MinImageWidth;

    /// <summary>
    /// Minimum image height in pixels.
    /// </summary>
    [JsonProperty("min_image_height")]
    public int? MinImageHeight;
}