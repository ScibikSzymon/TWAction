using ActionGenerator.Application.Common.Interfaces;
using ActionGenerator.Domain.Configuration;
using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Infrastructure.Services;

public sealed class CommandGenerator : ICommandGenerator
{
    public AttackCommand Generate(Village source, Target target)
    {
        var speedMinutesPerField = UnitConfigurationProvider.GetSpeedMinutesPerField(target.CommandType);
        var distance = source.Coordinates.CalculateDistance(target.Coordinates);

        var travelTimeMinutes = distance * speedMinutesPerField;
        var travelTimeSpan = TimeSpan.FromMinutes(travelTimeMinutes);
        var minTime = target.MinArrivalTime.Subtract(travelTimeSpan);
        var maxTime = target.MaxArrivalTime.Subtract(travelTimeSpan);

        return new AttackCommand
        {
            Source = source,
            Target = target,
            MinimalDepartureTime = minTime,
            MaximalDepartureTime = maxTime
        };
    }
}

