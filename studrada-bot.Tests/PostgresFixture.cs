using Microsoft.EntityFrameworkCore;
using studrada_bot.Data;
using Testcontainers.PostgreSql;

namespace studrada_bot.Tests;

/// <summary>
/// Піднімає справжній Postgres у Docker на час тестів і застосовує всі міграції.
/// Тестуємо проти реальної схеми (FK, unique-індекси, enum-as-string), а не in-memory заглушки.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options);
}

[CollectionDefinition("postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture>;
