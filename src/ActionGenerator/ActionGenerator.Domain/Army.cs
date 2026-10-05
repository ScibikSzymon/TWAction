using ActionGenerator.Domain.Utilities;

namespace ActionGenerator.Domain.Entities;

public sealed class Army
{
    public uint Spear { get; init; }
    public uint Sword { get; init; }
    public uint Axe { get; init; }
    public uint Archer { get; init; }
    public uint Spy { get; init; }
    public uint Light { get; init; }
    public uint HorseArcher { get; init; }
    public uint Heavy { get; init; }
    public uint Ram { get; init; }
    public uint Catapult { get; init; }
    public uint Noble { get; init; }

    private readonly Lazy<uint> _offensivePotential;
    private readonly Lazy<uint> _defensivePotential;
    private readonly Lazy<uint> _totalPotential;

    public Army()
    {
        _offensivePotential = new Lazy<uint>(() => PopulationCalculator.CalculateOffensivePopulation(this));
        _defensivePotential = new Lazy<uint>(() => PopulationCalculator.CalculateDefensivePopulation(this));
        _totalPotential = new Lazy<uint>(() => PopulationCalculator.CalculatePopulation(this));
    }

    public uint OffensivePotential => _offensivePotential.Value;
    public uint DefensivePotential => _defensivePotential.Value;
    public uint TotalPotential => _totalPotential.Value;
}
