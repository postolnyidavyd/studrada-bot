using studrada_bot.Data.Entities;
using studrada_bot.Data.Enums;
using studrada_bot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace studrada_bot.Telegram.Commands;

public static class InviteCommand
{
    private static readonly Dictionary<Role, string> RoleNames = new()
    {
        { Role.Admin, "адміна" },
        { Role.Member, "учасника" },
    };

    public static async Task HandleAsync(ITelegramBotClient bot, Message msg, Member member, IServiceProvider services, CancellationToken ct)
    {
        if (msg.Chat.Type != ChatType.Private) return;

        var memberService = services.GetRequiredService<MemberService>();

        if (member.Role != Role.Admin)
        {
            await bot.SendMessage(msg.Chat.Id, "Запрошувати нових учасників може тільки адмін.", cancellationToken: ct);
            return;
        }

        var args = msg.Text!.Split(' ');
        var roleArg = args.Length > 1 ? args[1].ToLower() : null;

        if (roleArg is null)
        {
            await bot.SendMessage(msg.Chat.Id, "Вкажи роль: /invite member або /invite admin", cancellationToken: ct);
            return;
        }

        if (!Enum.TryParse<Role>(roleArg, ignoreCase: true, out var role) || role == Role.Requester)
        {
            await bot.SendMessage(msg.Chat.Id, "Невідома роль. Доступні: member, admin", cancellationToken: ct);
            return;
        }

        var code = await memberService.CreateInviteAsync(member, role, ct);
        
        var text = $"Ось код запрошення для {RoleNames[role]}:\n<tg-spoiler>{code}</tg-spoiler>\nКод перестане працювати через <b>2 дні</b>.";
        await bot.SendMessage(msg.Chat.Id, text, ParseMode.Html, cancellationToken: ct);
    }
}