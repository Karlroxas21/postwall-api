namespace Domain.Entities;

public class NoteTag
{
    public Guid NoteId { get; private set; }

    public Guid TagId { get; private set; }
    public Note Note { get; private set; } = null!;
    public Tag Tag { get; private set; } = null!;

    public static NoteTag Create(Guid noteId, Guid tagId)
    {
        var NoteTag = new NoteTag();

        NoteTag.NoteId = noteId;
        NoteTag.TagId = tagId;

        return NoteTag;
    }

}
