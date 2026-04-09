using TWAction.Application.Templates.DTOs;
using TWAction.Domain.Templates;

namespace TWAction.Application.Templates.Mappers;

/// <summary>Maps between <see cref="TargetTemplate"/> domain objects and their DTOs.</summary>
internal static class TargetTemplateMapper
{
    internal static TargetTemplateDto ToDto(this TargetTemplate template) =>
        new(
            template.Id,
            template.UserId,
            template.Name,
            template.IsDefault,
            template.Waves
                .OrderBy(w => w.MaxTime)
                .Select(w => new TemplateWaveDto(w.MinTime, w.MaxTime, w.CommandNumber, w.CommandType))
                .ToList()
        );
}
