using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Domain.Tests.Entities;

public class NoteTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var dueDate = new DateOnly(2026, 6, 1);
        var folderId = Guid.NewGuid();

        var note = Note.Create(
            title: "Buy milk",
            content: "2L whole milk",
            color: NotePalette.Butter.Bg,
            isPinned: true,
            isArchived: false,
            dueDate: dueDate,
            folderId: folderId);

        note.Title.Should().Be("Buy milk");
        note.Content.Should().Be("2L whole milk");
        note.Color.Should().Be(NotePalette.Butter.Bg);
        note.IsPinned.Should().BeTrue();
        note.IsArchived.Should().BeFalse();
        note.DueDate.Should().Be(dueDate);
        note.FolderId.Should().Be(folderId);
        note.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldMutateFieldsAndUpdatedAt()
    {
        var note = Note.Create("old", "old", NotePalette.Butter.Bg, false, false, null, null);
        var ts = new DateTime(2026, 5, 19, 12, 0, 0, DateTimeKind.Utc);

        note.Update("new", "new content", NotePalette.Butter.Bg, true, true, null, null, ts);

        note.Title.Should().Be("new");
        note.Content.Should().Be("new content");
        note.IsPinned.Should().BeTrue();
        note.IsArchived.Should().BeTrue();
        note.UpdatedAt.Should().Be(ts);
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAt()
    {
        var note = Note.Create("t", "c", NotePalette.Butter.Bg, false, false, null, null);

        note.DeletedAt.Should().BeNull();

        note.SoftDelete();

        note.DeletedAt.Should().NotBeNull();
        note.DeletedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
