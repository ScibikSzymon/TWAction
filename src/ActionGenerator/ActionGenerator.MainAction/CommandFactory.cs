using ActionGenerator.Domain.Configuration;
using ActionGenerator.Domain.Entities;

namespace ActionGenerator.MainAction;

public static class CommandFactory
{
    public static AttackCommand Create(Village source, Target target)
    {
        var timePerField = UnitConfigurationProvider.GetTimePerFieldInMinutes(target.CommandType);
        var distance = source.Coordinates.CalculateDistance(target.Coordinates);
        var travelTime = TimeSpan.FromMinutes(distance * timePerField);

        return new AttackCommand
        {
            Source = source,
            Target = target,
            MinimalDepartureTime = target.MinArrivalTime - travelTime,
            MaximalDepartureTime = target.MaxArrivalTime - travelTime
        };
    }
}
