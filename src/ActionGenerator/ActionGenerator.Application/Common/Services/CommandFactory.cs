using ActionGenerator.Domain.Configuration;
using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Application.Common.Services;

public interface ICommandFactory
{
    AttackCommand Generate(Village source, Target target);
}

internal sealed class CommandFactory : ICommandFactory
{
    public AttackCommand Generate(Village source, Target target)
    {
        var timePerField = UnitConfigurationProvider.GetTimePerFieldInMinutes(target.CommandType);
        var distance = source.Coordinates.CalculateDistance(target.Coordinates);

        var travelTimeMinutes = distance * timePerField;
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

