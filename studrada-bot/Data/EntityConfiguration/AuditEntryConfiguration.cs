using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using studrada_bot.Data.Entities;

namespace studrada_bot.Data.EntityConfiguration;

public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.HasKey(ae => ae.Id);
        
        builder.HasOne<Post>().WithMany()
            .HasForeignKey(ae => ae.PostId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Member>().WithMany()
            .HasForeignKey(ae => ae.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}