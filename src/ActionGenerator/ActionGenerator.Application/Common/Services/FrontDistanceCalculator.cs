using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Application.Common.Services;

public interface IFrontDistanceCalculator
{
    void CalculateFrontDistances(
        IReadOnlyList<Village> allyVillages,
        IReadOnlyList<Village> enemyVillages);
}

internal sealed class FrontDistanceCalculator : IFrontDistanceCalculator
{
    public void CalculateFrontDistances(
        IReadOnlyList<Village> allyVillages,
        IReadOnlyList<Village> enemyVillages)
    {
        if (enemyVillages.Count == 0)
            return;

        if (allyVillages.Count == 0)
            return;

        Parallel.ForEach(enemyVillages, enemyVillage =>
        {
            var minDistance = allyVillages.Min(allyVillage => allyVillage.Coordinates.CalculateDistance(enemyVillage.Coordinates));

            enemyVillage.DistanceToFront = (int)Math.Round(minDistance);
        });

        Parallel.ForEach(allyVillages, allyVillage =>
        {
            var minDistance = enemyVillages.Min(enemyVillage =>
                enemyVillage.Coordinates.CalculateDistance(allyVillage.Coordinates));

            allyVillage.DistanceToFront = (int)Math.Round(minDistance);
        });
    }
}
