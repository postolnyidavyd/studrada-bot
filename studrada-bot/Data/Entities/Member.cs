using studrada_bot.Data.Enums;

namespace studrada_bot.Data.Entities;

public class Member
{
    public int Id { get; set; }
    
    public long TelegramId { get; set; }
    public string DisplayName { get; set; }
    public Role Role { get; set; } // Requester | Member | Admin
    
    public bool StartedBot { get; set; } // натиснув /start
    public long? PrivateChatId { get; set; }

    public bool IsActive { get; set; } = true; // false - пішов нахер
    
    public DateTimeOffset CreatedAt { get; set; }
}