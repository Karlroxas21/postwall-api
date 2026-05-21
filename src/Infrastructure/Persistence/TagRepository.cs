using Domain.Entities;
using Domain.Exceptions;
using Domain.Ports;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class TagRepository(PostWallDbContext db) : ITagRepository
{
    private readonly PostWallDbContext _db = db;

    public async Task AddAsync(Tag Tag, CancellationToken ct = default)
    {
        var exists = await _db.Tags
            .AnyAsync(t => t.DeletedAt == null && t.Name == Tag.Name, ct);

        if (exists)
        {
            throw new ConflictException($"Tag {Tag.Name} already exists");
        }

        await _db.Tags.AddAsync(Tag, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid Id, CancellationToken ct = default)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(n => n.Id == Id && n.DeletedAt == null, ct)
            ?? throw new NotFoundException($"Tag {Id} not found");

        tag.SoftDelete();
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<TagWithNoteCount>> GetAllWithTagCountAsync(int Page, int PageSize, CancellationToken ct = default)
    {
        var baseQuery = _db.Tags.Where(t => t.DeletedAt == null);

        var Total = await baseQuery.CountAsync(ct);

        var Items = await baseQuery
            .OrderBy(t => t.Name)
            .Skip((Page - 1) * PageSize)
            .Take(PageSize)
            .Select(t => new TagWithNoteCount(
                t,
                t.NoteTags.Count(nt => nt.Note.DeletedAt == null)
            )).ToListAsync(ct);

        return new PagedResult<TagWithNoteCount>(Items, Total, Page, PageSize);
    }

    public async Task<Tag?> GetByIdAsync(Guid Id, CancellationToken ct = default)
    {
        return await _db.Tags.FirstOrDefaultAsync(t => t.Id == Id && t.DeletedAt == null, ct);

    }

    public async Task UpdateAsync(Tag Tag, CancellationToken ct = default)
    {
        var existing = await _db.Tags.FindAsync([Tag.Id], ct)
            ?? throw new NotFoundException($"Tag {Tag.Id} not found");

        var nameTaken = await _db.Tags
            .AnyAsync(t => t.Id != Tag.Id && t.DeletedAt == null && t.Name == Tag.Name, ct);

        if (nameTaken)
        {
            throw new ConflictException($"Tag {Tag.Name} already exists");
        }

        _db.Entry(existing).CurrentValues.SetValues(Tag);

        await _db.SaveChangesAsync(ct);
    }
}
