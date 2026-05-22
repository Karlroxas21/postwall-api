using Domain.Entities;
using Domain.Exceptions;
using Domain.Ports;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class NoteRepository : INoteRepository
{
    private readonly PostWallDbContext _db;

    public NoteRepository(PostWallDbContext db) => _db = db;
    public async Task AddAsync(Note Note, CancellationToken ct = default)
    {
        await _db.Notes.AddAsync(Note, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid Id, CancellationToken ct = default)
    {
        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == Id && n.DeletedAt == null, ct)
            ?? throw new NotFoundException($"Note {Id} not found");

        note.SoftDelete();
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<Note>> GetAllAsync(int Page, int PageSize, bool? Pinned, CancellationToken ct = default)
    {
        var query = _db.Notes.Where(n => n.DeletedAt == null);
        if (Pinned.HasValue)
        {
            query = query.Where(n => n.IsPinned == Pinned.Value);
        }

        var Total = await query.CountAsync(ct);
        var Items = await query.OrderByDescending(n => n.CreatedAt)
            .Skip((Page - 1) * PageSize)
            .Take(PageSize)
            .Include(n => n.NoteTags)
            .ThenInclude(nt => nt.Tag)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);

        return new PagedResult<Note>(Items, Page, PageSize, Total);
    }

    public async Task<Note?> GetByIdAsync(Guid Id, CancellationToken ct = default)
    {
        return await _db.Notes
            .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
            .AsSplitQuery()
            .FirstOrDefaultAsync(n => n.Id == Id && n.DeletedAt == null, ct);
    }

    public async Task UpdateAsync(Note Note, CancellationToken ct = default)
    {
        var existing = await _db.Notes.FindAsync([Note.Id], ct)
            ?? throw new NotFoundException($"Note {Note.Id} not found");

        _db.Entry(existing).CurrentValues.SetValues(Note);

        await _db.SaveChangesAsync(ct);
    }

    public async Task AttachTagAsync(Guid noteId, Guid tagId, CancellationToken ct = default)
    {
        var noteExists = await _db.Notes.AnyAsync(n => n.Id == noteId && n.DeletedAt == null, ct);
        if (!noteExists)
        {
            throw new NotFoundException($"Note {noteId} not found");
        }

        var tagExists = await _db.Tags.AnyAsync(t => t.Id == tagId && t.DeletedAt == null, ct);
        if (!tagExists)
        {
            throw new NotFoundException($"Tag {tagId} not found");
        }

        var linked = await _db.Set<NoteTag>()
            .AnyAsync(nt => nt.NoteId == noteId && nt.TagId == tagId, ct);
        if (linked)
        {
            throw new ConflictException($"Tag {tagId} already attached to note {noteId}");
        }

        await _db.NoteTags.AddAsync(NoteTag.Create(noteId, tagId), ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DetachTagAsync(Guid noteId, Guid tagId, CancellationToken ct = default)
    {
        var noteExists = await _db.Notes.AnyAsync(n => n.Id == noteId && n.DeletedAt == null, ct);
        if (!noteExists)
        {
            throw new NotFoundException($"Note {noteId} not found");
        }

        var tagExists = await _db.Tags.AnyAsync(t => t.Id == tagId && t.DeletedAt == null, ct);
        if (!tagExists)
        {
            throw new NotFoundException($"Tag {tagId} not found");
        }

        await _db.NoteTags.Where(nt => nt.NoteId == noteId && nt.TagId == tagId).ExecuteDeleteAsync();
    }

    public async Task PinNoteAsync(Guid noteId, CancellationToken ct = default)
    {
        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.DeletedAt == null, ct)
            ?? throw new NotFoundException($"Note {noteId} not found");

        note.PinNote();
        await _db.SaveChangesAsync(ct);

    }

    public async Task UnpinNoteAsync(Guid noteId, CancellationToken ct = default)
    {
        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.DeletedAt == null, ct)
          ?? throw new NotFoundException($"Note {noteId} not found");

        note.UnpinNote();
        await _db.SaveChangesAsync(ct);
    }
}
