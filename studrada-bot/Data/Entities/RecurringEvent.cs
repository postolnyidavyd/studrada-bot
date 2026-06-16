namespace studrada_bot.Data.Entities;

public class RecurringEvent
{
    public int Id { get; set; }
    public string Title { get; set; }
    public RecurringType Type { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }
    public DayOfWeek? WeekDay;
    public int LeadDays { get; set; } // коли нагадати/створити(N днів до)

    public int? TargetChannelId { get; set; } // де публікувати

    public string? Template { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum RecurringType
{
    Birthday,
    Weekly,
    Monthly,
}