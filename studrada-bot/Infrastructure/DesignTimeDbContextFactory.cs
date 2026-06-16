using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using studrada_bot.Data;

namespace studrada_bot.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(config.GetConnectionString("Migration"))   // ← 5432, не pooler
            .Options;

        return new AppDbContext(opts);
    }
}