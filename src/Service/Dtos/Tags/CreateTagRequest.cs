namespace Service.Dtos.Tags;

public record CreateTagRequest(
    string Name,
    string? Color
);