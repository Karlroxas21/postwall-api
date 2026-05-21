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

    public async Task<CreateNoteRequest> CreateAsync(CreateNoteRequest request, CancellationToken ct)
    {
        var note = Note.Create(
            request.Title,
            request.Content,
            request.Color ?? NotePalette.Butter.Bg,
            request.IsPinned,
            request.IsArchived,
            request.DueDate,
            request.FolderId
        );

        await _noteRepository.AddAsync(note, ct);

        return request;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await _noteRepository.DeleteAsync(id, ct);
    }

    public async Task<PagedResult<NoteResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        var pageRes = await _noteRepository.GetAllAsync(page, pageSize, ct);

        var items = pageRes.Items.Select(n => new NoteResponse(
            n.Id,
            n.Title,
            n.Content,
            n.Color,
            n.IsPinned,
            n.IsArchived,
            n.DueDate,
            n.FolderId,
            n.CreatedAt,
            n.UpdatedAt
        )).ToList();

        return new PagedResult<NoteResponse>(items, pageRes.Page, pageRes.PageSize, pageRes.TotalCount);
    }

    public async Task<NoteResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var note = await _noteRepository.GetByIdAsync(id, ct);

        if (note is null)
        {
            throw new NotFoundException($"Note {id} not found");
        }

        return new NoteResponse(
            note.Id,
            note.Title,
            note.Content,
            note.Color,
            note.IsPinned,
            note.IsArchived,
            note.DueDate,
            note.FolderId,
            note.CreatedAt,
            note.UpdatedAt
        );
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
          request.Color ?? NotePalette.Butter.Bg,
          request.IsPinned,
          request.IsArchived,
          request.DueDate,
          request.FolderId,
          DateTime.UtcNow
      );

        await _noteRepository.UpdateAsync(existingNote, ct);

        return ToNoteReponse(existingNote);
    }

    private static NoteResponse ToNoteReponse(Note note)
    {
        return new NoteResponse(
            note.Id,
            note.Title,
            note.Content,
            note.Color,
            note.IsPinned,
            note.IsArchived,
            note.DueDate,
            note.FolderId,
            note.CreatedAt,
            note.UpdatedAt
        );
    }
}
