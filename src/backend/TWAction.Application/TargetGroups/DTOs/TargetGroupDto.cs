using TWAction.Application.Templates.DTOs;

namespace TWAction.Application.TargetGroups.DTOs;

/// <summary>Read-only projection of a target group returned by the API.</summary>
public sealed record TargetGroupDto(
    Guid Id,
    Guid ScheduleId,
    string Name,
    IReadOnlyList<string> VillageCoordinates,
    IReadOnlyList<TemplateWaveDto> Waves,
    Guid? BaseTemplateId,
    string? BaseTemplateName);

/// <summary>Payload for creating a new target group.</summary>
public sealed record CreateTargetGroupRequest(
    string Name,
    IReadOnlyList<string> VillageCoordinates,
    IReadOnlyList<TemplateWaveDto> Waves,
    Guid? BaseTemplateId,
    string? BaseTemplateName);

/// <summary>Payload for updating an existing target group.</summary>
public sealed record UpdateTargetGroupRequest(
    string Name,
    IReadOnlyList<string> VillageCoordinates,
    IReadOnlyList<TemplateWaveDto> Waves,
    Guid? BaseTemplateId,
    string? BaseTemplateName);
