using studrada_bot.Data.Enums;
using Telegram.Bot.Types.Enums;

namespace studrada_bot.Data.Entities;

public class Post
{
    public int Id { get; set; }
    
    //Поставлене завдання
    public string? Brief { get; set; }// опис
    public string? BriefMediaJson { get; set;} // матеріали
    
    // Готовий контент, хоче хтось опублікувати або виконане завдання
    public string? Content { get; set; }
    public MediaType ContentMediaType { get; set; }
    public string? ContentMediaFileId { get; set; }
    public ParseMode ParseMode { get; set; } = ParseMode.Html;

    public PublishMode PublishMode { get; set; }
    public PostOrigin Origin { get; set; }
    public PostStatus Status { get; set; }
    
    public int AuthorId { get; set; }// хто створив задачу/пост
    public int? OwnerId { get; set; }// хто виконує завдання (OpenTask: claim; SelfMade: = AuthorId)
    
    public int? ApprovedById { get; set;} // хто апрувнув
    public string? RejectionReason { get; set; }// остання причина відхилення

    public DateTimeOffset? Deadline { get; set; }// дедлайн до якого здаати\запостити
    public DateTimeOffset? ScheduledFor { get; set; } // точний час коли опублікувати; null - одразу по апруву

    public DateTimeOffset? PublishedAt { get; set; } // фактичний час публікації

    public string? HangfireJobId { get; set; }
    
    public int? SourceRecurringEventId { get; set; }

    public int? GroupMessageId { get; set; }
    
    public DateTimeOffset? EscalatedAt { get; set; }
    public DateTimeOffset? HumanNudgedOn { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }

    public IEnumerable<PostTarget> Targets { get; set; } = new List<PostTarget>();

}