namespace ActionGenerator.Application.Common.Interfaces;

public interface IFrontDistanceCalculator
{
    void CalculateFrontDistances(
        IReadOnlyList<Domain.Entities.Village> allyVillages,
        IReadOnlyList<Domain.Entities.Village> enemyVillages);
}
