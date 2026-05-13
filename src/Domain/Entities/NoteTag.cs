namespace Domain.Entities;

public class NoteTag
{
    public Guid NoteId { get; private set; }

    public Guid TagId { get; private set; }
    public Note Note { get; private set; } = null!;
    public Tag Tag { get; private set; } = null!;

}
