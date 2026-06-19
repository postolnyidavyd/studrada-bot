using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using studrada_bot.Data.Entities;

namespace studrada_bot.Data.EntityConfiguration;

public class AppStateConfiguration : IEntityTypeConfiguration<AppState>
{
    public void Configure(EntityTypeBuilder<AppState> builder)
    {
        builder.HasKey(a => a.Key);
        builder.Property(a => a.Key)
                .HasMaxLength(100);
    }
}