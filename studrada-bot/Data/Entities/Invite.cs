using studrada_bot.Data.Enums;

namespace studrada_bot.Data.Entities;

public class Invite
{
    public int Id { get; set; }
    
    public string Code { get; set; }
    public Role GrantsRole { get; set; }
    
    public int CreatedById { get; set; }
    
    public DateTimeOffset ExpiresAt { get; set; }
    
    public int? UsedById { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
}