using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using studrada_bot.Data.Entities;

namespace studrada_bot.Data.EntityConfiguration;

public class PostTargetConfiguration : IEntityTypeConfiguration<PostTarget>
{
    public void Configure(EntityTypeBuilder<PostTarget> builder)
    {
        builder.HasKey(pt => new { pt.PostId, pt.ChannelId });

        // FK до Post визначено в PostConfiguration через HasMany(p => p.Targets)
        builder.HasOne<Channel>().WithMany()
            .HasForeignKey(pt => pt.ChannelId);

    }
}