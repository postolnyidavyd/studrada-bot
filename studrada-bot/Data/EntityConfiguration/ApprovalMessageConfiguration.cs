using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using studrada_bot.Data.Entities;

namespace studrada_bot.Data.EntityConfiguration;

public class ApprovalMessageConfiguration : IEntityTypeConfiguration<ApprovalMessage>
{
    public void Configure(EntityTypeBuilder<ApprovalMessage> builder)
    {
        builder.HasKey(am => new { am.PostId, am.ChatId });
        
        builder.HasOne<Post>().WithMany()
            .HasForeignKey(am=> am.PostId);
    }
}