using Microsoft.AspNetCore.Mvc;
using Service.Dtos.Tags;
using Service.Ports;

namespace Entrypoint.Controllers;

[ApiController]
[Route("v1/api/tags")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;

    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default
    )
    {
        var result = await _tagService.GetAllAsync(page, pageSize, ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tag = await _tagService.GetByIdAsync(id, ct);

        return Ok(tag);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request, CancellationToken ct)
    {
        var created = await _tagService.CreateAsync(request, ct);

        return StatusCode(201, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTagRequest request, CancellationToken ct)
    {
        var updated = await _tagService.UpdateAsync(id, request, ct);

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _tagService.DeleteAsync(id, ct);
        
        return NoContent();
    }

}