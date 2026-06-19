using studrada_bot.Data.Entities;
using studrada_bot.Data.Enums;
using studrada_bot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace studrada_bot.Telegram.Commands;

public static class StartCommand
{
    public static async Task HandleAsync( ITelegramBotClient bot, Message msg, Member member, MemberService memberService, CancellationToken ct)
    {
        if (msg.Chat.Type != ChatType.Private) return;
        
        await memberService.OnBotStartedAsync(member, msg.Chat.Id, ct);
        
        var roleText = member.Role switch
        {
            Role.Admin     => "адмін",
            Role.Member    => "член команди",
            Role.Requester => "замовник",
            _              => "?"
        };

        var commands = member.Role == Role.Admin
            ? "/new — створити пост\n/list — активні пости\n/mine — мої пости\n/stats — статистика\n/invite — запросити в команду"
            : "/new — створити пост\n/list — активні пости\n/mine — мої пости";

        var text = $"Привіт, {member.DisplayName}! Ти в системі як {roleText}.\n\n{commands}";
        
        await bot.SendMessage(msg.Chat.Id, text, cancellationToken: ct);
        
    }
}