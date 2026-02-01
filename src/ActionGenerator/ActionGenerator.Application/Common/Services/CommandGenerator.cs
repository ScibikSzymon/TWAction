using ActionGenerator.Domain.Common.ValueObjects;
using ActionGenerator.Domain.Configuration;
using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Enums;

namespace ActionGenerator.Application.Common.Services;

public interface ICommandGenerator
{
    AttackCommand Generate(Village source, Target target);
}

public class CommandGenerator : ICommandGenerator
{
    public AttackCommand Generate(Village source, Target target)
    {
        var speedMinutesPerField = UnitConfigurationProvider.GetSpeedMinutesPerField(target.CommandType);
        var distance = source.Coordinates.CalculateDistance(target.Coordinates);

        var travelTimeMinutes = distance * speedMinutesPerField;
        var travelTimeSpan = TimeSpan.FromMinutes(travelTimeMinutes);
        var minTime = target.MinArrivalTime.Subtract(travelTimeSpan);
        var maxTime = target.MaxArrivalTime.Subtract(travelTimeSpan);

        return new AttackCommand()
        {
            Source = source,
            Destination = target,
            CommandType = target.CommandType,
            TimeWindow = new TimeWindow()
            {
                MinDepartureTime = minTime,
                MaxDepartureTime = maxTime,
                MinArrivalTime = target.MinArrivalTime,
                MaxArrivalTime = target.MaxArrivalTime
            }
        };
    }
}
