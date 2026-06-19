using Microsoft.EntityFrameworkCore;
using Npgsql;
using studrada_bot.Data;
using studrada_bot.Data.Entities;
using studrada_bot.Data.Enums;
using Telegram.Bot.Types;

namespace studrada_bot.Services;

public class MemberService (AppDbContext appDbContext)
{
    public Task<Member?> FindAsync(long telegramId, CancellationToken ct = default) =>
        appDbContext.Members.FirstOrDefaultAsync(m => m.TelegramId == telegramId, ct);
    
    public async Task OnBotStartedAsync(Member member, long privateChatId, CancellationToken ct)
    {
        member.PrivateChatId = privateChatId;
        member.StartedBot = true;
        await appDbContext.SaveChangesAsync(ct);
    }

    public async Task<Member> BootstrapFounderAsync(Message msg, long founderId, CancellationToken ct)
    {
        var founder = new Member()
        {
            TelegramId = founderId,
            DisplayName = msg.From!.LastName is { } ln ? $"{msg.From.FirstName} {ln}" : msg.From.FirstName,
            Role = Role.Admin,
            StartedBot = true,
            PrivateChatId = msg.Chat.Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        appDbContext.Members.Add(founder);

        try
        {
            await appDbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            // Retry після timeout вставив дубль — повернути вже існуючий запис
            appDbContext.ChangeTracker.Clear();
            return (await FindAsync(founderId, ct))!;
        }

        return founder;
    }
}