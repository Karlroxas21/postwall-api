using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WallPositionConfiguration : IEntityTypeConfiguration<WallPosition>
{
    public void Configure(EntityTypeBuilder<WallPosition> builder)
    {
        builder.HasKey(wp => wp.NoteId);

        builder.HasOne(wp => wp.Note)
            .WithOne(n => n.WallPosition)
            .HasForeignKey<WallPosition>(wp => wp.NoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(wp => wp.X).IsRequired();

        builder.Property(wp => wp.Y).IsRequired();

        builder.Property(wp => wp.Width).IsRequired();

        builder.Property(wp => wp.Height).IsRequired();
    }
}
