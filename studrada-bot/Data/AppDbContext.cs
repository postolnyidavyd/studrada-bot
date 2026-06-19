using Microsoft.EntityFrameworkCore;
using studrada_bot.Data.Entities;
using studrada_bot.Data.EntityConfiguration;

namespace studrada_bot.Data;

public class AppDbContext : DbContext
{
    public DbSet<ApprovalMessage> ApprovalMessages { get; set; }
    public DbSet<AppState> AppStates {get; set; }
    public DbSet<AuditEntry> AuditEntries { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<Invite> Invites { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostTarget> PostTargets { get; set; }
    public DbSet<RecurringEvent> RecurringEvents { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);


        configurationBuilder.Properties<Enum>()
            .HaveConversion<string>()
            .HaveMaxLength(32);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ApprovalMessageConfiguration());
        modelBuilder.ApplyConfiguration(new AppStateConfiguration());
        modelBuilder.ApplyConfiguration(new AuditEntryConfiguration());
        modelBuilder.ApplyConfiguration(new ChannelConfiguration());
        modelBuilder.ApplyConfiguration(new InviteConfiguration());
        modelBuilder.ApplyConfiguration(new MemberConfiguration());
        modelBuilder.ApplyConfiguration(new PostConfiguration());
        modelBuilder.ApplyConfiguration(new PostTargetConfiguration());
        modelBuilder.ApplyConfiguration(new RecurringEventConfiguration());
    }
}