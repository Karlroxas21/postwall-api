using Microsoft.AspNetCore.Mvc;
using Service.Dtos.Notes;
using Service.Ports;

namespace Entrypoint.Controllers;

[ApiController]
[Route("v1/api/notes")]
public class NoteController : ControllerBase
{
    private readonly INoteService _noteService;

    public NoteController(INoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default
    )
    {
        var result = await _noteService.GetAllAsync(page, pageSize, ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = "GetNoteById")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var note = await _noteService.GetByIdAsync(id, ct);

        return Ok(note);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoteRequest request, CancellationToken ct)
    {
        var created = await _noteService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNoteRequest request, CancellationToken ct)
    {
        var updated = await _noteService.UpdateAsync(id, request, ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _noteService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{noteId:guid}/tags/{tagId:guid}")]
    public async Task<IActionResult> AttachTag(Guid noteId, Guid tagId, CancellationToken ct)
    {
        await _noteService.AttachTagAsync(noteId, tagId, ct);

        return Created($"/v1/api/notes/{noteId}/tags/{tagId}", null);
    }

    [HttpDelete("{noteId:guid}/tags/{tagId:guid}")]
    public async Task<IActionResult> DetachTag(Guid noteId, Guid tagId, CancellationToken ct)
    {
        await _noteService.DetachTagAsync(noteId, tagId, ct);

        return NoContent();
    }
}
