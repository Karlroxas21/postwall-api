namespace Domain.Entities;

public class Folder : Base
{

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ICollection<Note> Notes { get; private set; } = [];

}
