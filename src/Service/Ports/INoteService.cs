using Domain.Entities;
using Domain.ValueObjects;
using Service.Dtos;

namespace Service.Ports;

public interface INoteService
{
    Task<CreateNoteRequest> CreateAsync(CreateNoteRequest request, CancellationToken ct);
    Task DeleteAsync(Guid Id, CancellationToken ct);
    Task<PagedResult<NoteResponse>> GetAllAsync(int Page, int PageSize, CancellationToken ct);
    Task<NoteResponse?> GetByIdAsync(Guid Id, CancellationToken ct);
    Task<NoteResponse> UpdateAsync(Guid Id, UpdateNoteRequest Request, CancellationToken ct);
}
