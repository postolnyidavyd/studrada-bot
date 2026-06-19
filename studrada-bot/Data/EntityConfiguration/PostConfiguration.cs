using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using studrada_bot.Data.Entities;

namespace studrada_bot.Data.EntityConfiguration;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasOne<Member>().WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne<Member>().WithMany()
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne<Member>().WithMany()
            .HasForeignKey(p => p.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RecurringEvent>().WithMany()
            .HasForeignKey(p => p.SourceRecurringEventId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Targets).WithOne()
            .HasForeignKey(t => t.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.ScheduledFor);
        builder.HasIndex(p => p.Deadline);
        builder.HasIndex(p => p.HangfireJobId);
    }
}