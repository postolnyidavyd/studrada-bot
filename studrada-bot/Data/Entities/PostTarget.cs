using studrada_bot.Data.Enums;

namespace studrada_bot.Data.Entities;

public class PostTarget
{
    public int PostId { get; set; }
    public int ChannelId { get; set; }

    public TargetStatus Status { get; set; }
    public int? PublishedMessageId { get; set; } // доказ публікації

    public DateTimeOffset? PublishedAt { get; set; }
    public string? LastError { get; set; }
}