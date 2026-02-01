using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Application.Common.Interfaces;

public interface IPopulationCalculator
{
    int CalculatePopulation(Army army);
}
