using ActionGenerator.Application.Common.Interfaces;
using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Infrastructure.Services;

public sealed class FrontDistanceCalculator(IDistanceCalculator distanceCalculator) : IFrontDistanceCalculator
{
    private readonly IDistanceCalculator _distanceCalculator = distanceCalculator;

    public void CalculateFrontDistances(
        IReadOnlyList<Village> allyVillages,
        IReadOnlyList<Village> enemyVillages)
    {
        if (!enemyVillages.Any())
            return;

        if (!allyVillages.Any())
            return;

        Parallel.ForEach(enemyVillages, enemyVillage =>
        {
            var minDistance = allyVillages.Min(allyVillage =>
                _distanceCalculator.CalculateDistance(
                    enemyVillage.Coordinates.X,
                    enemyVillage.Coordinates.Y,
                    allyVillage.Coordinates.X,
                    allyVillage.Coordinates.Y));

            enemyVillage.DistanceToFront = (int)Math.Round(minDistance);
        });

        Parallel.ForEach(allyVillages, allyVillage =>
        {
            var minDistance = enemyVillages.Min(enemyVillage =>
                _distanceCalculator.CalculateDistance(
                    allyVillage.Coordinates.X,
                    allyVillage.Coordinates.Y,
                    enemyVillage.Coordinates.X,
                    enemyVillage.Coordinates.Y));

            allyVillage.DistanceToFront = (int)Math.Round(minDistance);
        });
    }
}
