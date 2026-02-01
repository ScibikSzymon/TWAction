using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Application.Common.Interfaces;

public interface IPopulationCalculator
{
    int CalculatePopulation(Army army);
}

public sealed class PopulationCalculator : IPopulationCalculator
{
    private const int SpearPopulation = 1;
    private const int SwordPopulation = 1;
    private const int AxePopulation = 1;
    private const int ArcherPopulation = 1;
    private const int SpyPopulation = 2;
    private const int LightPopulation = 4;
    private const int HorseArcherPopulation = 5;
    private const int HeavyPopulation = 6;
    private const int RamPopulation = 5;
    private const int CatapultPopulation = 8;
    private const int NoblePopulation = 100;

    public int CalculatePopulation(Army army)
    {
        return army.Spear * SpearPopulation
            + army.Sword * SwordPopulation
            + army.Axe * AxePopulation
            + army.Archer * ArcherPopulation
            + army.Spy * SpyPopulation
            + army.Light * LightPopulation
            + army.HorseArcher * HorseArcherPopulation
            + army.Heavy * HeavyPopulation
            + army.Ram * RamPopulation
            + army.Catapult * CatapultPopulation
            + army.Noble * NoblePopulation;
    }
}
