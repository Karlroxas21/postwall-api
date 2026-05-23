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

        result.Title.Should().Be(request.Title);
        result.Content.Should().Be(request.Content);
        result.Color.Should().Be(request.Color);
        _repo.Verify(r => r.AddAsync(It.IsAny<Note>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NullTagIds_DoesNotAttach()
    {
        var request = new CreateNoteRequest("t", "c", null, false, false, null, null, TagIds: null);

        await _sut.CreateAsync(request, default);

        _repo.Verify(
            r => r.AttachTagAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_EmptyTagIds_DoesNotAttach()
    {
        var request = new CreateNoteRequest("t", "c", null, false, false, null, null, TagIds: Array.Empty<Guid>());

        await _sut.CreateAsync(request, default);

        _repo.Verify(
            r => r.AttachTagAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithTagIds_AttachesEachAndReloads()
    {
        var tagId1 = Guid.NewGuid();
        var tagId2 = Guid.NewGuid();

        Note? added = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Note>(), default))
            .Callback<Note, CancellationToken>((n, _) => added = n)
            .Returns(Task.CompletedTask);

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(() => added);

        var request = new CreateNoteRequest(
            "t", "c", null, false, false, null, null,
            TagIds: new[] { tagId1, tagId2 });

        await _sut.CreateAsync(request, default);

        _repo.Verify(r => r.AttachTagAsync(It.IsAny<Guid>(), tagId1, default), Times.Once);
        _repo.Verify(r => r.AttachTagAsync(It.IsAny<Guid>(), tagId2, default), Times.Once);
        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), default), Times.Once);
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

        _repo.Setup(r => r.GetAllAsync(1, 10, null, default)).ReturnsAsync(paged);

        var result = await _sut.GetAllAsync(1, 10, null, default);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items[0].Title.Should().Be("a");
        result.Items[1].IsPinned.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_PinnedTrue_PassesFilterToRepoAndReturnsOnlyPinned()
    {
        var notes = new List<Note>
        {
            Note.Create("pinned-1", "x", "#111", true, false, null, null),
            Note.Create("pinned-2", "y", "#222", true, false, null, null),
        };
        var paged = new PagedResult<Note>(notes, page: 1, pageSize: 10, totalCount: 2);

        _repo.Setup(r => r.GetAllAsync(1, 10, new NoteQuery(Pinned: true), default)).ReturnsAsync(paged);

        var result = await _sut.GetAllAsync(1, 10, new NoteQuery(Pinned: true), default);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(i => i.IsPinned);
        _repo.Verify(r => r.GetAllAsync(1, 10, new NoteQuery(Pinned: true), default), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_PinnedFalse_PassesFilterToRepoAndReturnsOnlyUnpinned()
    {
        var notes = new List<Note>
        {
            Note.Create("unpinned-1", "x", "#111", false, false, null, null),
        };
        var paged = new PagedResult<Note>(notes, page: 1, pageSize: 10, totalCount: 1);

        _repo.Setup(r => r.GetAllAsync(1, 10, new NoteQuery(Pinned: false), default)).ReturnsAsync(paged);

        var result = await _sut.GetAllAsync(1, 10, new NoteQuery(Pinned: false), default);

        result.Items.Should().HaveCount(1);
        result.Items.Should().OnlyContain(i => !i.IsPinned);
        _repo.Verify(r => r.GetAllAsync(1, 10, new NoteQuery(Pinned: false), default), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ArchivedTrue_PassesFilterToRepoAndReturnsOnlyArchived()
    {
        var notes = new List<Note>
        {
            Note.Create("archived-1", "x", "#111", true, isArchived: true, null, null),
            Note.Create("archived-2", "y", "#222", true, isArchived: true, null, null),
        };
        var paged = new PagedResult<Note>(notes, page: 1, pageSize: 10, totalCount: 2);

        _repo.Setup(r => r.GetAllAsync(1, 10, new NoteQuery(Pinned: false, Archived: true), default)).ReturnsAsync(paged);

        var result = await _sut.GetAllAsync(1, 10, new NoteQuery(Pinned: false, Archived: true), default);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(i => i.IsArchived);
        _repo.Verify(r => r.GetAllAsync(1, 10, new NoteQuery(Pinned: false, Archived: true), default), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ArchivedFalse_PassesFilterToRepoAndReturnsOnlyUnarchived()
    {
        var notes = new List<Note>
        {
            Note.Create("unarchived-1", "x", "#111", false, false, null, null),
        };
        var paged = new PagedResult<Note>(notes, page: 1, pageSize: 10, totalCount: 1);

        _repo.Setup(r => r.GetAllAsync(1, 10, new NoteQuery(Pinned: false, Archived: false), default)).ReturnsAsync(paged);

        var result = await _sut.GetAllAsync(1, 10, new NoteQuery(Pinned: false, Archived: false), default);

        result.Items.Should().HaveCount(1);
        result.Items.Should().OnlyContain(i => !i.IsArchived);
        _repo.Verify(r => r.GetAllAsync(1, 10, new NoteQuery(Pinned: false, Archived: false), default), Times.Once);
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

    // ---------- AttachTagAsync / DetachTagAsync ----------

    [Fact]
    public async Task AttachTagAsync_DelegatesToRepo()
    {
        var noteId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        await _sut.AttachTagAsync(noteId, tagId, default);

        _repo.Verify(r => r.AttachTagAsync(noteId, tagId, default), Times.Once);
    }

    [Fact]
    public async Task DetachTagAsync_DelegatesToRepo()
    {
        var noteId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        await _sut.DetachTagAsync(noteId, tagId, default);

        _repo.Verify(r => r.DetachTagAsync(noteId, tagId, default), Times.Once);
    }

    // ---------- PinNote / UnpinNote ----------

    [Fact]
    public async Task PinNote_DelegatesToRepo()
    {
        var noteId = Guid.NewGuid();

        await _sut.PinNote(noteId, default);

        _repo.Verify(r => r.PinNoteAsync(noteId, default), Times.Once);
    }

    [Fact]
    public async Task UnpinNote_DelegatesToRepo()
    {
        var noteId = Guid.NewGuid();

        await _sut.UnpinNote(noteId, default);

        _repo.Verify(r => r.UnpinNoteAsync(noteId, default), Times.Once);
    }

    // ---------- Archive Note / Unarchive Note ----------

    [Fact]
    public async Task ArchiveNote_DelegatesToRepo()
    {
        var noteId = Guid.NewGuid();

        await _sut.ArchiveNote(noteId, default);

        _repo.Verify(r => r.ArchiveNoteAsync(noteId, default), Times.Once);
    }

    [Fact]
    public async Task UnarchiveNote_DelegatesToRepo()
    {
        var noteId = Guid.NewGuid();

        await _sut.UnarchiveNote(noteId, default);

        _repo.Verify(r => r.UnarchiveNoteAsync(noteId, default), Times.Once);
    }
}
