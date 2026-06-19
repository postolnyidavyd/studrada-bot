using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using studrada_bot.Data.Entities;

namespace studrada_bot.Data.EntityConfiguration;

public class RecurringEventConfiguration : IEntityTypeConfiguration<RecurringEvent>
{
    public void Configure(EntityTypeBuilder<RecurringEvent> builder)
    {
        builder.HasKey(re => re.Id);

        builder.HasOne<Channel>().WithMany()
            .HasForeignKey(re => re.TargetChannelId);
    }
}