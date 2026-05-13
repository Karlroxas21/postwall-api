using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Content).HasColumnType("nvarchar(max)");

        builder.Property(n => n.Color)
            .HasColumnType("nvarchar(7)")
            .IsRequired();

        builder.HasOne(n => n.Folder)
               .WithMany(f => f.Notes)
               .HasForeignKey(n => n.FolderId)
               .OnDelete(DeleteBehavior.SetNull);

    }
}
