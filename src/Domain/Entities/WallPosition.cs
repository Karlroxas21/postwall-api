namespace Domain.Entities;

public class WallPosition
{

    public Guid NoteId { get; private set; }
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Width { get; private set; }
    public double Height { get; private set; }
    public Note Note {get; private set;} = null!;

}
