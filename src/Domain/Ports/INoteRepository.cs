using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Ports;

public interface INoteRepository
{
    Task<Note?> GetByIdAsync(Guid Id, CancellationToken ct = default);
    Task<PagedResult<Note>> GetAllAsync(int Page, int PageSize, NoteQuery query, CancellationToken ct = default);
    Task AddAsync(Note Note, CancellationToken ct = default);
    Task UpdateAsync(Note Note, CancellationToken ct = default);
    Task DeleteAsync(Guid Id, CancellationToken ct = default);
    Task AttachTagAsync(Guid noteId, Guid tagId, CancellationToken ct = default);
    Task DetachTagAsync(Guid noteId, Guid tagId, CancellationToken ct = default);
    Task PinNoteAsync(Guid noteId, CancellationToken ct = default);
    Task UnpinNoteAsync(Guid noteId, CancellationToken ct = default);
    Task ArchiveNoteAsync(Guid noteId, CancellationToken ct = default);
    Task UnarchiveNoteAsync(Guid noteId, CancellationToken ct = default);

}
