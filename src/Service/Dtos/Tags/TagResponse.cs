namespace Service.Dtos.Tags;

public record TagResponse
(
    Guid Id,
    string Name,
    string Color,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);