using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using studrada_bot.Data;
using studrada_bot.Infrastructure;
using studrada_bot.Services;
using studrada_bot.Telegram;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

var builder = WebApplication.CreateBuilder(args);

// ── Конфіг ──────────────────────────────────────────────────────────────────
builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("Bot"));

// ── Telegram ─────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<ITelegramBotClient>(_ =>
    new TelegramBotClient(
        builder.Configuration["Telegram:BotToken"]
            ?? throw new InvalidOperationException("Telegram:BotToken не задано")));

// ── База даних ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Default connection string не задано"),
        npgsql => npgsql.EnableRetryOnFailure()));

// ── Hangfire ─────────────────────────────────────────────────────────────────
builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(
        opts => opts.UseNpgsqlConnection(
            builder.Configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Default connection string не задано")),
        new PostgreSqlStorageOptions { SchemaName = "hangfire" }));

builder.Services.AddHangfireServer();

// ── Хендлери ─────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<UpdateHandler>();
builder.Services.AddScoped<MemberService>();

var app = builder.Build();

// ── 1. Міграції через прямий конект (5432) — до старту воркерів ─────────────
var migrationCs = builder.Configuration.GetConnectionString("Migration")
    ?? throw new InvalidOperationException("Migration connection string не задано");

await using (var migrationDb = new AppDbContext(
    new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(migrationCs).Options))
    await migrationDb.Database.MigrateAsync();

// ── 2. Перевірка конфігу ──────────────────────────────────────────────────────
var botOpts = app.Services.GetRequiredService<IOptions<BotOptions>>().Value;
if (botOpts.GroupChatId == 0)
    app.Logger.LogWarning("GroupChatId не задано — повідомлення в групу не надсилатимуться");

// ── 3. Hangfire dashboard (тільки локально — §13.10) ─────────────────────────
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new LocalRequestsOnlyAuthorizationFilter()]
});

// ── 4. Health check для Docker ────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok("ok"));

// ── 5. Telegram long polling (вогонь і забудь — не await) ────────────────────
var bot = app.Services.GetRequiredService<ITelegramBotClient>();

using var cts = new CancellationTokenSource();
bot.StartReceiving(
    app.Services.GetRequiredService<UpdateHandler>(),
    receiverOptions: new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
    cancellationToken: cts.Token);

app.Run();
