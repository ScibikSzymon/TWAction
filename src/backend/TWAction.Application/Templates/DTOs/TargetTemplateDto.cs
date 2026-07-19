namespace TWAction.Application.Templates.DTOs;

/// <summary>Represents a single wave of attacks within a template.</summary>
public sealed record TemplateWaveDto(
    TimeOnly MinTime,
    TimeOnly MaxTime,
    int CommandNumber,
    string CommandType);

/// <summary>Full representation of a target template returned to the client.</summary>
public sealed record TargetTemplateDto(
    Guid Id,
    Guid? UserId,
    string Name,
    bool IsDefault,
    IReadOnlyList<TemplateWaveDto> Waves);

/// <summary>Request body for creating a new user-owned target template.</summary>
public sealed record CreateTargetTemplateRequest(
    string Name,
    IReadOnlyList<TemplateWaveDto> Waves);

/// <summary>Request body for updating an existing user-owned target template.</summary>
public sealed record UpdateTargetTemplateRequest(
    string Name,
    IReadOnlyList<TemplateWaveDto> Waves);
