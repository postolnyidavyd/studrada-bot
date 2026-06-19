using Microsoft.Extensions.Options;
using studrada_bot.Infrastructure;
using studrada_bot.Services;
using studrada_bot.Telegram.Commands;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace studrada_bot.Telegram;

public class UpdateHandler(IServiceScopeFactory scopeFactory, IOptions<BotOptions> botOptions) : IUpdateHandler
{
    private readonly BotOptions _botOptions = botOptions.Value;

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var memberService = scope.ServiceProvider.GetRequiredService<MemberService>();

        var telegramId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
        if (telegramId is null) return;

        var member = await memberService.FindAsync(telegramId.Value, ct);

        // Якщо founder ще не в БД — реєструємо тільки при /start
        if (member is null
            && telegramId.Value == _botOptions.FounderId
            && update.Message?.Text?.StartsWith("/start") == true)
        {
            member = await memberService.BootstrapFounderAsync(update.Message!, telegramId.Value, ct);
        }

        if (!AccessControl.IsAllowed(member)) return;

        if (update.Message is { } msg)
        {
            // "/start@botname аргумент" → "/start"
            var command = msg.Text?.Split(' ', '@')[0];
            await (command switch
            {
                "/start" => StartCommand.HandleAsync(bot, msg, member!, memberService, ct),
                _        => Task.CompletedTask
            });
        }

        // TODO: CallbackQuery
    }

    public Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken ct)
    {
        // TODO: логування помилок polling
        return Task.CompletedTask;
    }
}
