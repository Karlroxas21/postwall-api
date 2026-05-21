using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace DOmain.Tests.Entities;

public class TagTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var tag = Tag.Create(
            name: "Learning .NET",
            color: NotePalette.Butter.Bg
        );

        tag.Name.Should().Be("Learning .NET");
        tag.Color.Should().Be(NotePalette.Butter.Bg);
    }

    [Fact]
    public void Update_ShouldMutateFieldsAndUpdatedAt()
    {
        var tag = Tag.Create(
            name: "old",
            color: NotePalette.Butter.Bg
        );
        var ts = new DateTime(2026, 5, 19, 12, 0, 0, DateTimeKind.Utc);

        tag.Update("newContent", NotePalette.Peach.Bg, ts);

        tag.Name.Should().NotBe("old");
        tag.Color.Should().NotBe(NotePalette.Butter.Bg);

        tag.Name.Should().Be("newContent");
        tag.Color.Should().Be(NotePalette.Peach.Bg);
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAt()
    {
        var tag = Tag.Create("2026", NotePalette.Peach.Bg);

        tag.DeletedAt.Should().BeNull();

        tag.SoftDelete();

        tag.DeletedAt.Should().NotBeNull();
        tag.DeletedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

}