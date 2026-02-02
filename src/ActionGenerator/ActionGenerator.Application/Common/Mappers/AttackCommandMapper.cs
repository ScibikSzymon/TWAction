using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Application.Common.Mappers;

internal static class AttackCommandMapper
{
    public static AttackCommandDto ToDto(this AttackCommand command)
    {
        return new AttackCommandDto
        {
            TimeWindow = new TimeWindowDto
            {
                MinDepartureTime = command.MinimalDepartureTime,
                MaxDepartureTime = command.MaximalDepartureTime,
                MinArrivalTime = command.Target.MinArrivalTime,
                MaxArrivalTime = command.Target.MaxArrivalTime
            },
            Source = command.Source.ToSmallDto(),
            Destination = command.Target.ToSmallDto()
        };
    }

    public static IReadOnlyList<AttackCommandDto> ToDtos(this IEnumerable<AttackCommand> commands)
    {
        return commands.Select(ToDto).ToList();
    }
}

