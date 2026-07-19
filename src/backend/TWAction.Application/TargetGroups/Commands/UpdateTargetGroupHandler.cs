using System.Text.RegularExpressions;
using TWAction.Application.Common;
using TWAction.Application.TargetGroups.DTOs;
using TWAction.Application.TargetGroups.Interfaces;
using TWAction.Application.TargetGroups.Mappers;
using TWAction.Application.Templates.DTOs;
using TWAction.Domain.Templates;

namespace TWAction.Application.TargetGroups.Commands;

public sealed record UpdateTargetGroupCommand(
    Guid GroupId,
    Guid ScheduleId,
    string Name,
    IReadOnlyList<string> VillageCoordinates,
    IReadOnlyList<TemplateWaveDto> Waves,
    Guid? BaseTemplateId,
    string? BaseTemplateName);

public sealed class UpdateTargetGroupHandler(ITargetGroupRepository repository)
{
    private static readonly Regex CoordinatePattern = new(@"^\d+\|\d+$", RegexOptions.Compiled);

    public async Task<Result<TargetGroupDto>> Handle(UpdateTargetGroupCommand command, CancellationToken ct = default)
    {
        var group = await repository.GetByIdAsync(command.GroupId, ct);

        if (group is null || group.ScheduleId != command.ScheduleId)
        {
            return Result.Failure<TargetGroupDto>("Target group not found.");
        }

        var validationError = Validate(command.Name, command.VillageCoordinates, command.Waves);
        if (validationError is not null)
        {
            return Result.Failure<TargetGroupDto>(validationError);
        }

        group.Name = command.Name.Trim();
        group.VillageCoordinates = [.. command.VillageCoordinates.Select(c => c.Trim())];
        group.Waves = [.. command.Waves.Select(MapWave).OrderBy(w => w.MaxTime)];
        group.BaseTemplateId = command.BaseTemplateId;
        group.BaseTemplateName = command.BaseTemplateName?.Trim();

        await repository.UpdateAsync(group, ct);
        return Result.Success(group.ToDto());
    }

    private static string? Validate(string name, IReadOnlyList<string> coords, IReadOnlyList<TemplateWaveDto> waves)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Group name is required.";
        }

        if (name.Length > 200)
        {
            return "Group name cannot exceed 200 characters.";
        }

        if (coords is null || coords.Count == 0)
        {
            return "At least one village coordinate is required.";
        }

        if (coords.Count > 500)
        {
            return "Cannot exceed 500 village coordinates per group.";
        }

        foreach (var coord in coords)
        {
            if (!CoordinatePattern.IsMatch(coord?.Trim() ?? string.Empty))
            {
                return $"Invalid coordinate format: '{coord}'. Expected X|Y (e.g., 473|490).";
            }
        }

        if (waves is null || waves.Count == 0)
        {
            return "At least one attack wave is required.";
        }

        return ValidateWaves(waves);
    }

    private static string? ValidateWaves(IReadOnlyList<TemplateWaveDto> waves)
    {
        foreach (var wave in waves)
        {
            if (wave.CommandNumber <= 0)
            {
                return "Command number must be positive.";
            }

            if (!CommandTypeConstants.All.Contains(wave.CommandType))
            {
                return $"Unknown command type: '{wave.CommandType}'.";
            }

            if (wave.MinTime >= wave.MaxTime)
            {
                return "MinTime must be less than MaxTime for each wave.";
            }
        }

        return null;
    }

    private static TemplateWave MapWave(TemplateWaveDto dto) =>
        new() { MinTime = dto.MinTime, MaxTime = dto.MaxTime, CommandNumber = dto.CommandNumber, CommandType = dto.CommandType };
}
