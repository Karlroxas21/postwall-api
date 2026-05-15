using Domain.ValueObjects;

namespace Domain.Entities;

public class Note : Base
{

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Color { get; private set; } = NotePalette.Butter.Bg;
    public bool IsPinned { get; private set; }
    public bool IsArchived { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public Guid? FolderId { get; private set; }

    public Folder? Folder { get; private set; }
    public WallPosition? WallPosition { get; private set; }
    public ICollection<NoteTag> NoteTags { get; private set; } = [];

    public static Note Create(string title, string content, string color, bool isPinned, bool isArchived, DateOnly? dueDate, Guid? folderId)
    {
        var Note = new Note();

        Note.Title = title;
        Note.Content = content;
        Note.Color = color;
        Note.IsPinned = isPinned;
        Note.IsArchived = isArchived;
        Note.DueDate = dueDate;
        Note.FolderId = folderId;

        return Note;
    }

    public void Update(string title, string content, string color, bool isPinned, bool isArchived, DateOnly? dueDate, Guid? folderId)
    {
        Title = title;
        Content = content;
        Color = color;
        IsPinned = isPinned;
        IsArchived = isArchived;
        DueDate = dueDate;
        FolderId = folderId;
    }
}
