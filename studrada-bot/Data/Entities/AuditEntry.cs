namespace studrada_bot.Data.Entities;
//Це для логів якщо опять забудеш, щоб не тупив хвилину
public class AuditEntry
{
    public int Id { get; set; }
    public int? PostId { get; set; }
    public int MemberId { get; set; }
    public string Action { get; set; } = null!; // "Claimed" | "Submitted" | "Approved" | "Rejected" | "Published" | ...
    public string? Details { get; set; }
    public DateTimeOffset At { get; set; }
}