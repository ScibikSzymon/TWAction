using TWAction.Application.Common;
using TWAction.Application.Templates.DTOs;
using TWAction.Application.Templates.Interfaces;
using TWAction.Application.Templates.Mappers;

namespace TWAction.Application.Templates.Queries;

/// <summary>Returns a single target template by its identifier.</summary>
public sealed record GetTargetTemplateByIdQuery(Guid TemplateId, Guid UserId);

public class GetTargetTemplateByIdHandler(ITargetTemplateRepository repository)
{
    public async Task<Result<TargetTemplateDto>> Handle(
        GetTargetTemplateByIdQuery query,
        CancellationToken ct = default)
    {
        var template = await repository.GetByIdAsync(query.TemplateId, ct);

        if (template is null)
        {
            return Result.Failure<TargetTemplateDto>("Template not found.");
        }

        // Users may only access their own templates or shared default ones.
        if (!template.IsDefault && template.UserId != query.UserId)
        {
            return Result.Failure<TargetTemplateDto>("Template not found.");
        }

        return Result.Success(template.ToDto());
    }
}
