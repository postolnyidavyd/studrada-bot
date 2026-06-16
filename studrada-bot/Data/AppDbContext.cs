using Microsoft.EntityFrameworkCore;
using studrada_bot.Data.Entities;
using studrada_bot.Data.EntityConfiguration;

namespace studrada_bot.Data;

public class AppDbContext : DbContext
{
    public DbSet<Member> Members { get; set; }
    public DbSet<Channel> Channels { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new MemberConfiguration());
        modelBuilder.ApplyConfiguration(new ChannelConfiguration());
    }
}