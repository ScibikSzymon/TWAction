using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Application.Common.Mappers;

internal static class AttackCommandMapper
{
    public static AttackCommandDto ToDto(AttackCommand command)
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
            Source = VillageMapper.ToSmallDto(command.Source),
            Destination = VillageMapper.ToSmallDto(command.Target)
        };
    }

    public static IReadOnlyList<AttackCommandDto> ToDtos(IEnumerable<AttackCommand> commands)
    {
        return commands.Select(ToDto).ToList();
    }
}

