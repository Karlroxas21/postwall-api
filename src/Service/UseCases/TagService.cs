using Domain.Entities;
using Domain.Exceptions;
using Domain.Ports;
using Domain.ValueObjects;
using Service.Dtos.Tags;
using Service.Ports;

namespace Service.UseCases;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository) => _tagRepository = tagRepository;

    public async Task<CreateTagRequest> CreateAsync(CreateTagRequest request, CancellationToken ct)
    {
        var tag = Tag.Create(
            request.Name,
            request.Color ?? NotePalette.Blush.Bg
        );

        await _tagRepository.AddAsync(tag, ct);

        return request;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await _tagRepository.DeleteAsync(id, ct);
    }

    public async Task<PagedResult<TagWithNoteCount>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        return await _tagRepository.GetAllWithTagCountAsync(page, pageSize, ct);
    }

    public async Task<TagResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var tag = await _tagRepository.GetByIdAsync(id, ct);

        if (tag is null)
        {
            throw new NotFoundException($"Tag {id} not found");
        }

        return ToTagResponse(tag);
    }

    public async Task<TagResponse> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken ct)
    {
        var existingTag = await _tagRepository.GetByIdAsync(id, ct);

        if (existingTag is null)
        {
            throw new NotFoundException($"Tag {id} not found");
        }

        existingTag.Update(
            request.Name,
            request.Color,
            DateTime.UtcNow
        );

        await _tagRepository.UpdateAsync(existingTag, ct);

        return ToTagResponse(existingTag);
    }

    private static TagResponse ToTagResponse(Tag tag)
    {
        return new TagResponse(
            tag.Id,
            tag.Name,
            tag.Color,
            tag.NoteTags,
            tag.CreatedAt,
            tag.UpdatedAt
        );
    }
}
