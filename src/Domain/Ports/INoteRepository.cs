using System;
using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Ports;

public interface INoteRepository
{
    Task<Note?> GetByIdAsync(Guid Id, CancellationToken ct = default);
    Task<PagedResult<Note>> GetAllAsync(int Page, int PageSize, CancellationToken ct = default);
    Task AddAsync(Note Note, CancellationToken ct = default);
    Task UpdateAsync(Note Note, CancellationToken ct = default);
    Task DeleteAsync(Guid Id, CancellationToken ct = default);

}
