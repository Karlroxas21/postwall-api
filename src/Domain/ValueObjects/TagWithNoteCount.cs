using Domain.Entities;

namespace Domain.ValueObjects;

public record TagWithNoteCount(Tag Tag, int NoteCount);
