namespace Domain.Entities;

public class Tag : Base
{

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#FAF3C0";
    public ICollection<NoteTag> NoteTags { get; private set; } = [];

    public static Tag Create(string name, string color)
    {
        var Tag = new Tag();

        Tag.Name = name;
        Tag.Color = color;

        return Tag;
    }

    public void Update(string name, string color, DateTime updatedAt)
    {
        Name = name;
        Color = color;
        UpdatedAt = updatedAt;
    }
    public void SoftDelete() => DeletedAt = DateTime.UtcNow;
}
