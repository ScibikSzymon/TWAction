using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Tribes.DTOs;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.Schedules;

namespace TWAction.Application.Tribes.Queries;

public sealed record GetTribesQuery(WorldType World);

public class GetTribesHandler(ITribesService tribesService)
{
    public async Task<Result<List<TribeDto>>> Handle(GetTribesQuery query, CancellationToken cancellationToken = default)
    {
        var result = await tribesService.GetTribesAsync(query.World, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<List<TribeDto>>(result.Error);
        }

        return Result.Success(ITribeMapper.ToDtos(result.Value));
    }
}



