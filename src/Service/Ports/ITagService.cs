using Domain.ValueObjects;
using Service.Dtos.Tags;

namespace Service.Ports;

public interface ITagService
{
    Task<CreateTagRequest> CreateAsync(CreateTagRequest request, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<PagedResult<TagWithNoteCount>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<TagResponse?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<TagResponse> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken ct);
}
