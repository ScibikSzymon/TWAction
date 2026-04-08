using TWAction.Application.Common;
using TWAction.Application.Templates.DTOs;
using TWAction.Application.Templates.Interfaces;
using TWAction.Application.Templates.Mappers;

namespace TWAction.Application.Templates.Queries;

/// <summary>Returns all default templates together with the authenticated user's own templates.</summary>
public sealed record GetTargetTemplatesQuery(Guid UserId);

public class GetTargetTemplatesHandler(ITargetTemplateRepository repository)
{
    public async Task<Result<IEnumerable<TargetTemplateDto>>> Handle(
        GetTargetTemplatesQuery query,
        CancellationToken ct = default)
    {
        var templates = await repository.GetAllAsync(query.UserId, ct);

        var dtos = templates
            .OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.Name)
            .Select(t => t.ToDto());

        return Result.Success<IEnumerable<TargetTemplateDto>>(dtos);
    }
}
