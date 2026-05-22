using Domain.Entities;
using Domain.Exceptions;
using Domain.Ports;
using Domain.ValueObjects;
using Service.Dtos.Notes;
using Service.Ports;

namespace Service.UseCases;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;

    public NoteService(INoteRepository noteRepository) => _noteRepository = noteRepository;

    public async Task<NoteResponse> CreateAsync(CreateNoteRequest request, CancellationToken ct)
    {
        var note = Note.Create(
            request.Title,
            request.Content,
            request.Color,
            request.IsPinned,
            request.IsArchived,
            request.DueDate,
            request.FolderId
        );

        await _noteRepository.AddAsync(note, ct);

        if (request.TagIds is not null)
        {
            foreach (var tagId in request.TagIds.Distinct())
            {
                await _noteRepository.AttachTagAsync(note.Id, tagId, ct);
            }

            note = await _noteRepository.GetByIdAsync(note.Id, ct) ?? note;
        }

        return ToNoteResponse(note);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await _noteRepository.DeleteAsync(id, ct);
    }

    public async Task<PagedResult<NoteResponse>> GetAllAsync(int page, int pageSize, bool? pinned, CancellationToken ct)
    {
        var pageRes = await _noteRepository.GetAllAsync(page, pageSize, pinned, ct);

        var items = pageRes.Items.Select(n => ToNoteResponse(n)).ToList();

        return new PagedResult<NoteResponse>(items, pageRes.Page, pageRes.PageSize, pageRes.TotalCount);
    }

    public async Task<NoteResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var note = await _noteRepository.GetByIdAsync(id, ct);

        if (note is null)
        {
            throw new NotFoundException($"Note {id} not found");
        }

        return ToNoteResponse(note);
    }

    public async Task<NoteResponse> UpdateAsync(Guid id, UpdateNoteRequest request, CancellationToken ct)
    {
        var existingNote = await _noteRepository.GetByIdAsync(id, ct);

        if (existingNote is null)
        {
            throw new NotFoundException($"Note {id} not found");
        }

        existingNote.Update(
          request.Title,
          request.Content,
          request.Color,
          request.IsPinned,
          request.IsArchived,
          request.DueDate,
          request.FolderId,
          DateTime.UtcNow
      );

        await _noteRepository.UpdateAsync(existingNote, ct);

        return ToNoteResponse(existingNote);
    }

    public async Task AttachTagAsync(Guid noteId, Guid tagId, CancellationToken ct)
    {
        await _noteRepository.AttachTagAsync(noteId, tagId, ct);
    }

    public async Task DetachTagAsync(Guid noteId, Guid tagId, CancellationToken ct)
    {
        await _noteRepository.DetachTagAsync(noteId, tagId, ct);
    }

    public async Task PinNote(Guid noteId, CancellationToken ct)
    {
        await _noteRepository.PinNoteAsync(noteId, ct);
    }

    public async Task UnpinNote(Guid noteId, CancellationToken ct)
    {
        await _noteRepository.UnpinNoteAsync(noteId, ct);
    }
    private static NoteResponse ToNoteResponse(Note note)
    {
        return new NoteResponse(
            note.Id,
            note.Title,
            note.Content,
            note.Color,
            note.IsPinned,
            note.IsArchived,
            note.NoteTags
                .Where(nt => nt.Tag is not null && nt.Tag.DeletedAt == null)
                .Select(nt => new NoteTagSummary(nt.TagId, nt.Tag.Name, nt.Tag.Color))
                .ToList(),
            note.DueDate,
            note.FolderId,
            note.CreatedAt,
            note.UpdatedAt
        );
    }
}
