using TWAction.Application.AttackCommands.DTOs;
using TWAction.Application.AttackCommands.Interfaces;
using TWAction.Application.AttackCommands.Queries;
using TWAction.Application.Common;
using TWAction.Application.ReconnaissanceActions.DTOs;
using TWAction.Application.Schedules.Interfaces;

namespace TWAction.Application.AttackCommands.Handlers;

/// <summary>
/// Handler for getting attack commands for a schedule.
/// </summary>
public sealed class GetAttackCommandsHandler(
    IScheduleRepository scheduleRepository,
    IAttackCommandRepository attackCommandRepository)
{
    public async Task<Result<IReadOnlyList<AttackCommandResponseDto>>> Handle(
        GetAttackCommandsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Validate schedule exists
        var schedule = await scheduleRepository.GetByIdAsync(query.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<IReadOnlyList<AttackCommandResponseDto>>("Schedule not found.");
        }

        // Get attack commands
        var commands = await attackCommandRepository.GetByScheduleIdAsync(query.ScheduleId, cancellationToken);

        // Map to DTOs
        var dtos = commands.Select(c => new AttackCommandResponseDto
        {
            Id = c.Id,
            TimeWindow = new TimeWindowDto
            {
                MinDepartureTime = c.MinDepartureTime,
                MaxDepartureTime = c.MaxDepartureTime,
                MinArrivalTime = c.MinArrivalTime,
                MaxArrivalTime = c.MaxArrivalTime
            },
            Source = new VillageSmallDto
            {
                Id = c.SourceVillageId,
                X = c.SourceX,
                Y = c.SourceY,
                PlayerId = c.SourcePlayerId
            },
            Destination = new VillageSmallDto
            {
                Id = c.DestinationVillageId,
                X = c.DestinationX,
                Y = c.DestinationY,
                PlayerId = c.DestinationPlayerId
            },
            CommandType = c.CommandType,
            CreatedAt = c.CreatedAt
        }).ToList();

        return Result.Success<IReadOnlyList<AttackCommandResponseDto>>(dtos);
    }
}
