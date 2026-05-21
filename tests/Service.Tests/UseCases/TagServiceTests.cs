using Domain.Entities;
using Domain.Exceptions;
using Domain.Ports;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Service.Dtos.Tags;
using Service.UseCases;

namespace Service.Tests.UseCases;

public class TagServiceTests
{
    private readonly Mock<ITagRepository> _repo = new();
    private readonly TagService _sut;

    public TagServiceTests()
    {
        _sut = new TagService(_repo.Object);
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_ValidRequest_CallsRepoAddOnce()
    {
        var request = new CreateTagRequest(
            Name: "2026",
            Color: NotePalette.Butter.Bg
        );

        var result = await _sut.CreateAsync(request, default);

        result.Should().BeSameAs(request);
        _repo.Verify(r => r.AddAsync(It.IsAny<Tag>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_PersistsNameAndColor()
    {
        Tag? captured = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Tag>(), default))
            .Callback<Tag, CancellationToken>((t, _) => captured = t)
            .Returns(Task.CompletedTask);

        var request = new CreateTagRequest("urgent", NotePalette.Peach.Bg);

        await _sut.CreateAsync(request, default);

        captured.Should().NotBeNull();
        captured.Name.Should().Be("urgent");
        captured.Color.Should().Be(NotePalette.Peach.Bg);
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_Missing_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Tag?)null);

        await FluentActions.Invoking(() => _sut.GetByIdAsync(Guid.NewGuid(), default))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_Exists_ReturnsMappedResponse()
    {
        var tag = Tag.Create("work", NotePalette.Sage.Bg);
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(tag);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), default);

        result.Should().NotBeNull();
        result.Name.Should().Be("work");
        result.Color.Should().Be(NotePalette.Sage.Bg);
    }

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResultFromRepo()
    {
        var items = new List<TagWithNoteCount>
        {
            new(Tag.Create("a", NotePalette.Sky.Bg), 3),
            new(Tag.Create("b", NotePalette.Blush.Bg), 0),
        };
        var paged = new PagedResult<TagWithNoteCount>(items, page: 1, pageSize: 10, totalCount: 2);

        _repo.Setup(r => r.GetAllWithTagCountAsync(1, 10, default)).ReturnsAsync(paged);

        var result = await _sut.GetAllAsync(1, 10, default);

        result.Should().BeSameAs(paged);
        result.Items.Should().HaveCount(2);
        result.Items[0].NoteCount.Should().Be(3);
        result.Items[1].Tag.Name.Should().Be("b");
    }

    // ---------- UpdateAsync ----------

    [Fact]
    public async Task UpdateAsync_Missing_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Tag?)null);

        var request = new UpdateTagRequest("x", NotePalette.Sand.Bg);

        await FluentActions.Invoking(() => _sut.UpdateAsync(Guid.NewGuid(), request, default))
            .Should().ThrowAsync<NotFoundException>();

        _repo.Verify(r => r.UpdateAsync(It.IsAny<Tag>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Existing_UpdatesAndPersists()
    {
        var existing = Tag.Create("old", NotePalette.Butter.Bg);
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(existing);

        var request = new UpdateTagRequest("new", NotePalette.Peach.Bg);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), request, default);

        result.Name.Should().Be("new");
        result.Color.Should().Be(NotePalette.Peach.Bg);
        existing.Name.Should().Be("new");
        existing.Color.Should().Be(NotePalette.Peach.Bg);
        existing.UpdatedAt.Should().NotBeNull();
        _repo.Verify(r => r.UpdateAsync(existing, default), Times.Once);
    }

    // ---------- DeleteAsync ----------

    [Fact]
    public async Task DeleteAsync_DelegatesToRepo()
    {
        var id = Guid.NewGuid();

        await _sut.DeleteAsync(id, default);

        _repo.Verify(r => r.DeleteAsync(id, default), Times.Once);
    }
}
