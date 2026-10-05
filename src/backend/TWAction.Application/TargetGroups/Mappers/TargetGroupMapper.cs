using TWAction.Application.TargetGroups.DTOs;
using TWAction.Application.Templates.DTOs;
using TWAction.Domain.TargetGroups;

namespace TWAction.Application.TargetGroups.Mappers;

internal static class TargetGroupMapper
{
    internal static TargetGroupDto ToDto(this TargetGroup group) =>
        new(
            group.Id,
            group.ScheduleId,
            group.Name,
            group.VillageCoordinates.AsReadOnly(),
            group.Waves
                .OrderBy(w => w.MaxTime)
                .Select(w => new TemplateWaveDto(w.MinTime, w.MaxTime, w.CommandNumber, w.CommandType))
                .ToArray(),
            group.BaseTemplateId,
            group.BaseTemplateName);
}
