namespace Service.Dtos;

public record CreateNoteRequest(
    string Title,
    string Content,
    string? Color,
    bool IsPinned,
    bool IsArchived,
    DateOnly? DueDate,
    Guid? FolderId
);