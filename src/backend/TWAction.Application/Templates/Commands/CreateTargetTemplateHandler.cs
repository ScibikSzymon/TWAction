using TWAction.Application.Common;
using TWAction.Application.Templates.DTOs;
using TWAction.Application.Templates.Interfaces;
using TWAction.Application.Templates.Mappers;
using TWAction.Domain.Templates;

namespace TWAction.Application.Templates.Commands;

/// <summary>Creates a new target template owned by the authenticated user.</summary>
public sealed record CreateTargetTemplateCommand(
    Guid UserId,
    string Name,
    IReadOnlyList<TemplateWaveDto> Waves);

public class CreateTargetTemplateHandler(ITargetTemplateRepository repository)
{
    public async Task<Result<TargetTemplateDto>> Handle(
        CreateTargetTemplateCommand command,
        CancellationToken ct = default)
    {
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

        var template = new TargetTemplate
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Name = command.Name.Trim(),
            IsDefault = false,
            Waves = command.Waves.Select(MapWave).ToList()
        };

        var created = await repository.CreateAsync(template, ct);
        return Result.Success(created.ToDto());
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
