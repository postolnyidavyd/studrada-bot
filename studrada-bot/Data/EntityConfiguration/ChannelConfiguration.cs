using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using studrada_bot.Data.Entities;

namespace studrada_bot.Data.EntityConfiguration;

public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.TelegramChatId).IsUnique();
    }
}