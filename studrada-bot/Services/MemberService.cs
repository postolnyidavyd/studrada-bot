using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using studrada_bot.Data;
using studrada_bot.Data.Entities;
using studrada_bot.Data.Enums;
using Telegram.Bot.Types;

namespace studrada_bot.Services;

public class MemberService(AppDbContext appDbContext)
{
    private Dictionary<long, (int count, DateTime window)> _redeemAttempts = new();

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
            DisplayName = GetDisplayName(msg.From!),
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

    public async Task<string> CreateInviteAsync(Member admin, Role role, CancellationToken ct)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        char[] result = new char[8];
        for (int i = 0; i < result.Length; i++)
            result[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];

        string code = $"{new string(result.AsSpan(0, 4))}-{new string(result.AsSpan(4, 4))}";

        var invite = new Invite
        {
            Code = HashInviteCode(code),
            GrantsRole = role,
            CreatedById = admin.Id,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(2),
        };

        appDbContext.Invites.Add(invite);
        await appDbContext.SaveChangesAsync(ct);

        return code;
    }

    public async Task<RedeemInviteResult> RedeemInviteAsync(Message msg, string plainCode, CancellationToken ct)
    {
        var telegramId = msg.From!.Id;

        if (_redeemAttempts.TryGetValue(telegramId, out var existing))
        {
            if (existing.count >= 5 && DateTime.UtcNow - existing.window < TimeSpan.FromHours(1))
            {
                return new RedeemInviteResult(RedeemResult.RateLimited);
            }

            if (DateTime.UtcNow - existing.window >= TimeSpan.FromHours(1))
                _redeemAttempts.Remove(telegramId); // вікно минуло — скидаємо
        }

        var hashedCode = HashInviteCode(plainCode);
        var invite = await appDbContext.Invites
            .FirstOrDefaultAsync(i => i.Code == hashedCode, ct);


        if (invite == null)
        {
            IncrementRedeemAttempts(telegramId);
            return new RedeemInviteResult(RedeemResult.NotFound);
        }

        if (invite.ExpiresAt < DateTimeOffset.UtcNow)
        {
            IncrementRedeemAttempts(telegramId);
            return new RedeemInviteResult(RedeemResult.Expired);
        }

        if (invite.UsedById.HasValue)
        {
            IncrementRedeemAttempts(telegramId);
            return new RedeemInviteResult(RedeemResult.AlreadyUsed);
        }


        if (invite.GrantsRole == Role.Admin)
            return new RedeemInviteResult(RedeemResult.PendingAdminConfirmation, invite.Id);

        var newMember = new Member()
        {
            TelegramId = telegramId,
            DisplayName = GetDisplayName(msg.From!),
            Role = invite.GrantsRole,
            StartedBot = true,
            PrivateChatId = msg.Chat.Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await using var tx = await appDbContext.Database.BeginTransactionAsync(ct);

        appDbContext.Members.Add(newMember);
        await appDbContext
            .SaveChangesAsync(
                ct); // тепер newMember.Id заповнений                                                                                                                                                                                  

        invite.UsedById = newMember.Id;
        invite.UsedAt = DateTimeOffset.UtcNow;
        await appDbContext.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return new RedeemInviteResult(RedeemResult.Success);
    }

    private void IncrementRedeemAttempts(long telegramId)
    {
        if (_redeemAttempts.TryGetValue(telegramId, out var existing))
        {
            _redeemAttempts[telegramId] = (existing.count + 1, existing.window);
        }
        else
        {
            _redeemAttempts[telegramId] = (1, DateTime.UtcNow);
        }
    }

    public async Task<Member?> ConfirmAdminInviteAsync(int inviteId, User from, long chatId, CancellationToken ct)
    {
        var invite = await appDbContext.Invites.FindAsync([inviteId], ct);;
        if (invite is null || invite.UsedById.HasValue || invite.ExpiresAt < DateTimeOffset.UtcNow)
            return null;

        var newMember = new Member
        {
            TelegramId = from.Id,
            DisplayName = GetDisplayName(from),
            Role = Role.Admin,
            StartedBot = true,
            PrivateChatId = chatId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await using var tx = await appDbContext.Database.BeginTransactionAsync(ct);
        appDbContext.Members.Add(newMember);
        await appDbContext.SaveChangesAsync(ct);

        invite.UsedById = newMember.Id;
        invite.UsedAt = DateTimeOffset.UtcNow;
        await appDbContext.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        
        return newMember;
    }

    private static string GetDisplayName(User from) =>
        from.LastName is { } ln ? $"{from.FirstName} {ln}" : from.FirstName;

    private static string HashInviteCode(string code)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(code);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }
}

public record RedeemInviteResult(RedeemResult Status, int? PendingInviteId = null);
public enum RedeemResult
{
    Success,
    PendingAdminConfirmation,
    NotFound,
    Expired,
    AlreadyUsed,
    RateLimited,
}