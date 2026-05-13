namespace Domain.Entities;

public class Note : Base
{

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#FAF3C0";
    public bool IsPinned { get; private set; }
    public bool IsArchived { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public Guid? FolderId { get; private set; }
    
    public Folder? Folder { get; private set; }
    public WallPosition? WallPosition { get; private set; }
    public ICollection<NoteTag> NoteTags { get; private set; } = [];

}
