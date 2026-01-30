using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Settings.DTOs;
using TWAction.Application.Settings.Interfaces;

namespace TWAction.Application.Settings.Queries;

public sealed record GetReconnaissanceSettingsQuery(Guid ScheduleId);

public class GetReconnaissanceSettingsHandler(IReconnaissanceSettingsRepository repository)
{
    public async Task<Result<ReconnaissanceSettingsDto>> Handle(
        GetReconnaissanceSettingsQuery query, 
        CancellationToken cancellationToken = default)
    {
        var settings = await repository.GetByScheduleIdAsync(query.ScheduleId, cancellationToken);

        if (settings is null)
        {
            return Result.Failure<ReconnaissanceSettingsDto>(
                $"Reconnaissance settings for schedule '{query.ScheduleId}' not found.");
        }

        return Result.Success(IReconnaissanceSettingsMapper.ToDto(settings));
    }
}
