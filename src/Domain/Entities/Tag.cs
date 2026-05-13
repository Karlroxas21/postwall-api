namespace Domain.Entities;

public class Tag : Base
{

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#FAF3C0";
    public ICollection<NoteTag> NoteTags { get; private set; } = [];

}
