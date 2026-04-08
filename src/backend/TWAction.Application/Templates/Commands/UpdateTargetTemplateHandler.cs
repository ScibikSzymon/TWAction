using TWAction.Application.Common;
using TWAction.Application.Templates.DTOs;
using TWAction.Application.Templates.Interfaces;
using TWAction.Application.Templates.Mappers;
using TWAction.Domain.Templates;

namespace TWAction.Application.Templates.Commands;

/// <summary>Updates the name and waves of an existing user-owned template.</summary>
public sealed record UpdateTargetTemplateCommand(
    Guid TemplateId,
    Guid UserId,
    string Name,
    IReadOnlyList<TemplateWaveDto> Waves);

public class UpdateTargetTemplateHandler(ITargetTemplateRepository repository)
{
    public async Task<Result<TargetTemplateDto>> Handle(
        UpdateTargetTemplateCommand command,
        CancellationToken ct = default)
    {
        var template = await repository.GetByIdAsync(command.TemplateId, ct);

        if (template is null || template.UserId != command.UserId)
        {
            return Result.Failure<TargetTemplateDto>("Template not found.");
        }

        if (template.IsDefault)
        {
            return Result.Failure<TargetTemplateDto>("Default templates cannot be modified.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Failure<TargetTemplateDto>("Template name is required.");
        }

        if (command.Name.Length > 200)
        {
            return Result.Failure<TargetTemplateDto>("Template name cannot exceed 200 characters.");
        }

        if (command.Waves is null || command.Waves.Count == 0)
        {
            return Result.Failure<TargetTemplateDto>("Template must contain at least one wave.");
        }

        var waveError = ValidateWaves(command.Waves);
        if (waveError is not null)
        {
            return Result.Failure<TargetTemplateDto>(waveError);
        }

        template.Name = command.Name.Trim();
        template.Waves = command.Waves.Select(MapWave).ToList();

        await repository.UpdateAsync(template, ct);
        return Result.Success(template.ToDto());
    }

    private static string? ValidateWaves(IReadOnlyList<TemplateWaveDto> waves)
    {
        foreach (var wave in waves)
        {
            if (!CommandTypeConstants.All.Contains(wave.CommandType))
            {
                return $"Invalid command type: '{wave.CommandType}'.";
            }

            if (wave.CommandNumber <= 0)
            {
                return "Each wave must have at least one command.";
            }

            if (wave.MinTime >= wave.MaxTime)
            {
                return "Wave MinTime must be earlier than MaxTime.";
            }
        }

        return null;
    }

    private static TemplateWave MapWave(TemplateWaveDto dto) =>
        new()
        {
            MinTime = dto.MinTime,
            MaxTime = dto.MaxTime,
            CommandNumber = dto.CommandNumber,
            CommandType = dto.CommandType
        };
}
