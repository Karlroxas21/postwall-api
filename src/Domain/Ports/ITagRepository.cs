using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Ports;

public interface ITagRepository
{
    Task AddAsync(Tag Tag, CancellationToken ct = default);
    Task<PagedResult<TagWithNoteCount>> GetAllWithTagCountAsync(int Page, int PageSize, CancellationToken ct = default);
    Task<Tag?> GetByIdAsync(Guid Id, CancellationToken ct = default);
    Task UpdateAsync(Tag Tag, CancellationToken ct = default);
    Task DeleteAsync(Guid Id, CancellationToken ct = default);

}
