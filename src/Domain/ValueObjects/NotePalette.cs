namespace Domain.ValueObjects;

public record NotePalette(string Bg, string Text, string TagDot)
{
    public static readonly NotePalette Butter = new("#f5e6a8", "#5a4a1a", "#d9c270");
    public static readonly NotePalette Peach = new("#f0c8a0", "#6a4628", "#d99e6e");
    public static readonly NotePalette Sage = new("#c8d8b8", "#3e5230", "#9eb588");
    public static readonly NotePalette Sky = new("#b8c8d8", "#2e4258", "#8ba6c2");
    public static readonly NotePalette Blush = new("#d8c0c8", "#5a3848", "#bf99a6");
    public static readonly NotePalette Sand = new("#e8e0d0", "#5a4e38", " 	#c8baa0");
}
