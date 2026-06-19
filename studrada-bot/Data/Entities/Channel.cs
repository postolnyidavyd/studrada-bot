namespace studrada_bot.Data.Entities;

public class Channel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public long TelegramChatId { get; set; } // є адміном в цьому каналі
}