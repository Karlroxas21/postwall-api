using Domain.Entities;

namespace Service.Dtos.Tags;

public record TagResponse
(
    Guid Id,
    string Name,
    string Color,
    ICollection<NoteTag> NoteTags,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);