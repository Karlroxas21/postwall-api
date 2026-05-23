namespace Domain.ValueObjects;

public record NoteQuery(
    bool? Pinned = null,
    bool? Archived = null,
    Guid? TagId = null,
    string? Search = null
);