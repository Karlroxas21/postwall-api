namespace Service.Dtos.Notes;

public record NoteResponse(
    Guid Id,
    string Title,
    string Content,
    string Color,
    bool IsPinned,
    bool IsArchived,
    IReadOnlyList<NoteTagSummary> Tags,
    DateOnly? DueDate,
    Guid? FolderId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

