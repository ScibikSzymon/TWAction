using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Settings.DTOs;
using TWAction.Application.Settings.Interfaces;

namespace TWAction.Application.Settings.Queries;

public sealed record GetMainActionSettingsQuery(Guid ScheduleId);

public class GetMainActionSettingsHandler(IMainActionSettingsRepository repository)
{
    public async Task<Result<MainActionSettingsDto>> Handle(
        GetMainActionSettingsQuery query, 
        CancellationToken cancellationToken = default)
    {
        var settings = await repository.GetByScheduleIdAsync(query.ScheduleId, cancellationToken);

        if (settings is null)
        {
            return Result.Failure<MainActionSettingsDto>(
                $"Main action settings for schedule '{query.ScheduleId}' not found.");
        }

        return Result.Success(IMainActionSettingsMapper.ToDto(settings));
    }
}
