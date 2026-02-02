using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Domain.Utilities;

public static class PopulationCalculator
{
    private const uint SpearPopulation = 1;
    private const uint SwordPopulation = 1;
    private const uint AxePopulation = 1;
    private const uint ArcherPopulation = 1;
    private const uint SpyPopulation = 2;
    private const uint LightPopulation = 4;
    private const uint HorseArcherPopulation = 5;
    private const uint HeavyPopulation = 6;
    private const uint RamPopulation = 5;
    private const uint CatapultPopulation = 8;
    private const uint NoblePopulation = 100;

    public static uint CalculatePopulation(Army army)
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
