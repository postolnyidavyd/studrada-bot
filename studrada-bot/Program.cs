using Microsoft.EntityFrameworkCore;
using studrada_bot.Data;
using studrada_bot.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// конфіг бота
builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("Bot"));
builder.Services.AddDbContext<AppDbContext>(option =>
    option.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        npgsql => npgsql.EnableRetryOnFailure()
    ));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var canConnect = await db.Database.CanConnectAsync();
    Console.WriteLine(canConnect
        ? "✅ Supabase: конект є"
        : "❌ Supabase: конекту НЕМА — перевір рядок/пароль/SSL");
}
app.Run();
