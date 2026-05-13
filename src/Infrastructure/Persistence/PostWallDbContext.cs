using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class PostWallDbContext(DbContextOptions<PostWallDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<NoteTag> NoteTags => Set<NoteTag>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<WallPosition> WallPositions => Set<WallPosition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostWallDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
