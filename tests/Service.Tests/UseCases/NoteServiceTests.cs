using Domain.Entities;
using Domain.Exceptions;
using Domain.Ports;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Service.Dtos.Notes;
using Service.UseCases;

namespace Service.Tests.UseCases;

public class NoteServiceTests
{
    private readonly Mock<INoteRepository> _repo = new();
    private readonly NoteService _sut;

    public NoteServiceTests()
    {
        _sut = new NoteService(_repo.Object);
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_ValidRequest_CallsRepoAddOnce()
    {
        var request = new CreateNoteRequest(
            Title: "Buy milk",
            Content: "2L whole milk",
            Color: "#ffffff",
            IsPinned: false,
            IsArchived: false,
            DueDate: null,
            FolderId: null
        );

        var result = await _sut.CreateAsync(request, default);

        result.Should().BeSameAs(request);
        _repo.Verify(r => r.AddAsync(It.IsAny<Note>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NullColor_DefaultsToButterPalette()
    {
        Note? captured = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Note>(), default))
            .Callback<Note, CancellationToken>((n, _) => captured = n)
            .Returns(Task.CompletedTask);

        var request = new CreateNoteRequest("t", "c", Color: null, false, false, null, null);

        await _sut.CreateAsync(request, default);

        captured.Should().NotBeNull();
        captured!.Color.Should().Be(NotePalette.Butter.Bg);
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_Missing_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Note?)null);

        await FluentActions.Invoking(() => _sut.GetByIdAsync(Guid.NewGuid(), default))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_Exists_ReturnsMappedResponse()
    {
        var note = Note.Create("hello", "world", "#abc123", true, false, null, null);
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(note);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), default);

        result.Should().NotBeNull();
        result!.Title.Should().Be("hello");
        result.Content.Should().Be("world");
        result.Color.Should().Be("#abc123");
        result.IsPinned.Should().BeTrue();
    }

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_ReturnsPagedMappedItems()
    {
        var notes = new List<Note>
        {
            Note.Create("a", "b", "#111", false, false, null, null),
            Note.Create("c", "d", "#222", true,  false, null, null),
        };
        var paged = new PagedResult<Note>(notes, page: 1, pageSize: 10, totalCount: 2);

        _repo.Setup(r => r.GetAllAsync(1, 10, default)).ReturnsAsync(paged);

        var result = await _sut.GetAllAsync(1, 10, default);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items[0].Title.Should().Be("a");
        result.Items[1].IsPinned.Should().BeTrue();
    }

    // ---------- UpdateAsync ----------

    [Fact]
    public async Task UpdateAsync_Missing_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Note?)null);

        var request = new UpdateNoteRequest("t", "c", "#fff", false, false, null, null);

        await FluentActions.Invoking(() => _sut.UpdateAsync(Guid.NewGuid(), request, default))
            .Should().ThrowAsync<NotFoundException>();

        _repo.Verify(r => r.UpdateAsync(It.IsAny<Note>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Existing_UpdatesAndPersists()
    {
        var existing = Note.Create("old", "old", "#000", false, false, null, null);
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(existing);

        var request = new UpdateNoteRequest("new", "new-content", "#fff", true, false, null, null);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), request, default);

        result.Title.Should().Be("new");
        result.Content.Should().Be("new-content");
        result.IsPinned.Should().BeTrue();
        existing.Title.Should().Be("new");
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
