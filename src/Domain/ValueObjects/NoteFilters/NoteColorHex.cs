namespace Domain.ValueObjects.NoteFilters;

public static class NoteColorHex
{
    public static readonly Dictionary<Color, string> ToHex = new()
    {
        [Color.Butter] = "#f5e6a8",
        [Color.Peach] = "#f0c8a0",
        [Color.Sage] = "#c8d8b8",
        [Color.Sky] = "#b8c8d8",
        [Color.Blush] = "#d8c0c8",
        [Color.Sand] = "#e8e0d0"
    };
}