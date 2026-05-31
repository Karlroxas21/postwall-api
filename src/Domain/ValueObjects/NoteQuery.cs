using Domain.ValueObjects.NoteFilters;

namespace Domain.ValueObjects;

public record NoteQuery(
    bool? Pinned = null,
    bool? Archived = null,
    Guid? TagId = null,
    string? Search = null,
    Color? Color = null,
    Due? Due = null,
    Sort? Sort = null,
    Order? Order = null
);