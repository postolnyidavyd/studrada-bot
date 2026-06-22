using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using studrada_bot.Data;
using studrada_bot.Data.Entities;
using studrada_bot.Data.Enums;
using studrada_bot.Services;
using Telegram.Bot.Types;

namespace studrada_bot.Tests;

[Collection("postgres")]
public class MemberServiceInviteTests(PostgresFixture fixture) : IAsyncLifetime
{
    // Чистимо таблиці перед кожним тестом — контейнер один на всі тести, дані ізолюємо самі
    public async Task InitializeAsync()
    {
        await using var db = fixture.CreateContext();
        await db.Database.ExecuteSqlRawAsync(
            """TRUNCATE "Members", "Invites" RESTART IDENTITY CASCADE;""");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateInvite_StoresHashNotPlaintext()
    {
        await using var db = fixture.CreateContext();
        var admin = await SeedAdminAsync(db);
        var service = new MemberService(db);

        var code = await service.CreateInviteAsync(admin, Role.Member, default);

        var invite = await db.Invites.SingleAsync();
        Assert.NotEqual(code, invite.Code);            // не plaintext
        Assert.Equal(Sha256Hex(code), invite.Code);    // саме SHA-256 хеш
    }

    [Fact]
    public async Task Redeem_ValidMemberCode_CreatesMemberAndMarksUsed()
    {
        await using var db = fixture.CreateContext();
        var admin = await SeedAdminAsync(db);
        var service = new MemberService(db);
        var code = await service.CreateInviteAsync(admin, Role.Member, default);

        var result = await service.RedeemInviteAsync(MsgFrom(555, "Іван", "Петренко"), code, default);

        Assert.Equal(RedeemResult.Success, result.Status);

        var member = await db.Members.SingleAsync(m => m.TelegramId == 555);
        Assert.Equal(Role.Member, member.Role);
        Assert.Equal("Іван Петренко", member.DisplayName);

        var invite = await db.Invites.SingleAsync();
        Assert.Equal(member.Id, invite.UsedById);
        Assert.NotNull(invite.UsedAt);
    }

    [Fact]
    public async Task Redeem_SameCodeTwice_ReturnsAlreadyUsed()
    {
        await using var db = fixture.CreateContext();
        var admin = await SeedAdminAsync(db);
        var service = new MemberService(db);
        var code = await service.CreateInviteAsync(admin, Role.Member, default);

        await service.RedeemInviteAsync(MsgFrom(555), code, default);
        var second = await service.RedeemInviteAsync(MsgFrom(777), code, default);

        Assert.Equal(RedeemResult.AlreadyUsed, second.Status);
        Assert.False(await db.Members.AnyAsync(m => m.TelegramId == 777));
    }

    [Fact]
    public async Task Redeem_ExpiredCode_ReturnsExpired()
    {
        await using var db = fixture.CreateContext();
        var admin = await SeedAdminAsync(db);
        var service = new MemberService(db);
        var code = await service.CreateInviteAsync(admin, Role.Member, default);

        // зсуваємо термін у минуле напряму в БД
        var invite = await db.Invites.SingleAsync();
        invite.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();

        var result = await service.RedeemInviteAsync(MsgFrom(555), code, default);

        Assert.Equal(RedeemResult.Expired, result.Status);
    }

    [Fact]
    public async Task Redeem_AdminCode_ReturnsPendingAndDoesNotCreateMember()
    {
        await using var db = fixture.CreateContext();
        var admin = await SeedAdminAsync(db);
        var service = new MemberService(db);
        var code = await service.CreateInviteAsync(admin, Role.Admin, default);

        var result = await service.RedeemInviteAsync(MsgFrom(555), code, default);

        Assert.Equal(RedeemResult.PendingAdminConfirmation, result.Status);
        Assert.NotNull(result.PendingInviteId);
        Assert.False(await db.Members.AnyAsync(m => m.TelegramId == 555));  // ще не активований

        var invite = await db.Invites.SingleAsync();
        Assert.Null(invite.UsedById);                                       // інвайт ще не використаний
    }

    [Fact]
    public async Task Redeem_RateLimited_AfterFiveFailures()
    {
        await using var db = fixture.CreateContext();
        var service = new MemberService(db);
        var msg = MsgFrom(555);

        // 5 невалідних спроб — усі NotFound
        for (int i = 0; i < 5; i++)
        {
            var r = await service.RedeemInviteAsync(msg, "WRON-GXXX", default);
            Assert.Equal(RedeemResult.NotFound, r.Status);
        }

        // 6-та — заблоковано
        var blocked = await service.RedeemInviteAsync(msg, "WRON-GXXX", default);
        Assert.Equal(RedeemResult.RateLimited, blocked.Status);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static async Task<Member> SeedAdminAsync(AppDbContext db)
    {
        var admin = new Member
        {
            TelegramId = 1,
            DisplayName = "Founder",
            Role = Role.Admin,
            StartedBot = true,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Members.Add(admin);
        await db.SaveChangesAsync();
        return admin;
    }

    private static Message MsgFrom(long telegramId, string firstName = "Test", string? lastName = null) =>
        new()
        {
            From = new User { Id = telegramId, FirstName = firstName, LastName = lastName },
            Chat = new Chat { Id = telegramId }
        };

    private static string Sha256Hex(string s) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLower();
}
