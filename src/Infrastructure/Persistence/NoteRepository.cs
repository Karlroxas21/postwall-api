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
        await _db.Notes.AddAsync(Note);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid Id, CancellationToken ct = default)
    {
        await _db.Notes.Where(n => n.Id == Id).ExecuteDeleteAsync(ct);
    }

    public async Task<PagedResult<Note>> GetAllAsync(int Page, int PageSize, CancellationToken ct = default)
    {
        var Total = await _db.Notes.CountAsync(ct);
        var Items = await _db.Notes.OrderByDescending(n => n.CreatedAt)
            .Skip((Page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync(ct);

        return new PagedResult<Note>(Items, Page, PageSize, Total);
    }

    public async Task<Note?> GetByIdAsync(Guid Id, CancellationToken ct = default)
    {
        return await _db.Notes.FindAsync([Id], ct);
    }

    public async Task UpdateAsync(Note Note, CancellationToken ct = default)
    {
        var existing = await _db.Notes.FindAsync([Note.Id], ct)
            ?? throw new NotFoundException($"Note {Note.Id} not found");

        _db.Entry(existing).CurrentValues.SetValues(Note);

        await _db.SaveChangesAsync(ct);
    }
}
