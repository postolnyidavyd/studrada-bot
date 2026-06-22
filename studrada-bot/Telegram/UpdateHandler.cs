using Microsoft.Extensions.Options;
using studrada_bot.Infrastructure;
using studrada_bot.Services;
using studrada_bot.Telegram.Commands;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace studrada_bot.Telegram;

public class UpdateHandler(IServiceScopeFactory scopeFactory, IOptions<BotOptions> botOptions) : IUpdateHandler
{
    private readonly BotOptions _botOptions = botOptions.Value;

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var memberService = services.GetRequiredService<MemberService>();

        var telegramId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
        if (telegramId is null) return;

        var member = await memberService.FindAsync(telegramId.Value, ct);

        // Founder bootstrap
        if (member is null
            && telegramId.Value == _botOptions.FounderId
            && update.Message?.Text?.StartsWith("/start") == true)
        {
            member = await memberService.BootstrapFounderAsync(update.Message!, telegramId.Value, ct);
        }

        // Invite redemption для незнайомих користувачів
        if (member is null && update.Message is { } unknownMsg)
        {
            await HandleUnknownMessageAsync(bot, unknownMsg, memberService, ct);
            return;
        }

        if (!AccessControl.IsAllowed(member)) return;

        if (update.Message is { } msg)
        {
            var command = msg.Text?.Split(' ', '@')[0];
            await (command switch
            {
                "/start"  => StartCommand.HandleAsync(bot, msg, member!, services, ct),
                "/invite" => InviteCommand.HandleAsync(bot, msg, member!, services, ct),
                _         => Task.CompletedTask
            });
        }

        // TODO: CallbackQuery
    }

    private async Task HandleUnknownMessageAsync(ITelegramBotClient bot, Message msg, MemberService memberService, CancellationToken ct)
    {
        if (msg.Chat.Type != ChatType.Private) return;

        var text = msg.Text?.Trim();
        if (string.IsNullOrEmpty(text)) return;

        if (text.StartsWith('/'))
        {
            await bot.SendMessage(msg.Chat.Id, "Надішли код запрошення щоб приєднатись до команди.", cancellationToken: ct);
            return;
        }

        var result = await memberService.RedeemInviteAsync(msg, text, ct);

        switch (result.Status)
        {
            case RedeemResult.Success:
                await bot.SendMessage(msg.Chat.Id, "Вітаємо! Тебе додано до команди. Натисни /start щоб почати.", cancellationToken: ct);
                break;

            case RedeemResult.PendingAdminConfirmation:
                await HandlePendingAdminConfirmationAsync(bot, msg, result.PendingInviteId!.Value, ct);
                break;

            case RedeemResult.RateLimited:
                await bot.SendMessage(msg.Chat.Id, "Забагато спроб. Спробуй через годину.", cancellationToken: ct);
                break;

            // NotFound / Expired / AlreadyUsed — навмисно однакова відповідь, не розкриваємо чи код існує
            default:
                await bot.SendMessage(msg.Chat.Id, "Невірний або прострочений код запрошення.", cancellationToken: ct);
                break;
        }
    }

    private async Task HandlePendingAdminConfirmationAsync(ITelegramBotClient bot, Message msg, int inviteId, CancellationToken ct)
    {
        var from = msg.From!;
        var displayName = from.LastName is { } ln ? $"{from.FirstName} {ln}" : from.FirstName;

        await bot.SendMessage(
            _botOptions.FounderId,
            $"<b>{displayName}</b> хоче стати адміном. Підтвердити?",
            parseMode: ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup([[
                InlineKeyboardButton.WithCallbackData("✅ Підтвердити", $"confirm_admin:{inviteId}:{from.Id}:{msg.Chat.Id}"),
                InlineKeyboardButton.WithCallbackData("❌ Відхилити",   $"reject_admin:{inviteId}:{from.Id}")
            ]]),
            cancellationToken: ct);

        await bot.SendMessage(msg.Chat.Id, "Запит надіслано. Очікуй підтвердження від адміна.", cancellationToken: ct);
    }

    public Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken ct)
    {
        // TODO: логування помилок polling
        return Task.CompletedTask;
    }
}
