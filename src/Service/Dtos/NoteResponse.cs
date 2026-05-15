namespace Service.Dtos;

public record NoteResponse(
    Guid Id,
    string Title,
    string Content,
    string Color,
    bool IsPinned,
    bool IsArchived,
    DateOnly? DueDate,
    Guid? FolderId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

