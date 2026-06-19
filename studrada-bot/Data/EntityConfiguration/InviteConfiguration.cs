using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using studrada_bot.Data.Entities;

namespace studrada_bot.Data.EntityConfiguration;

public class InviteConfiguration: IEntityTypeConfiguration<Invite>
{
    public void Configure(EntityTypeBuilder<Invite> builder)
    {
        builder.HasKey(i => i.Id);

        builder.HasIndex(i => i.Code).IsUnique();
        builder.Property(i => i.Code).HasMaxLength(64);

        builder.HasOne<Member>().WithMany()
            .HasForeignKey(i => i.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Member>().WithMany()
            .HasForeignKey(i => i.UsedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}