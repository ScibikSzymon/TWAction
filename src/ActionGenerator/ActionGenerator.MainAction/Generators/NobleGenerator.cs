using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Enums;
using ActionGenerator.Domain.Settings;

namespace ActionGenerator.MainAction.Generators;

/// <summary>
/// Generates noble commands using a most-constrained-first algorithm.
///
/// Off-type nobles (Full/Half/Quarter/150Axes/100HeavyCavalry/NobleWithDeff):
///   The closest eligible source is always chosen, which means the command
///   departs as late as possible (shortest travel time = latest departure window).
///
/// RandomNoble:
///   Most-constrained-first target selection, but the source village is picked at random.
///
/// Commands are added to storage immediately so that <see cref="NobleLimitsChecker"/>
/// always sees the current garrison and budget state when evaluating subsequent targets.
/// </summary>
internal sealed class NobleGenerator(ICommandsStorage storage, NobleLimitsChecker limitsChecker) : ICommandTypeGenerator
{
    public void Generate(
        IReadOnlyList<SourceVillage> allyVillages,
        IReadOnlyList<Target> targets,
        ActionSettings settings)
    {
        foreach (var commandType in NobleLimitsChecker.NobleCommandTypes)
        {
            var typeTargets = targets.Where(t => t.CommandType == commandType).ToList();
            if (typeTargets.Count == 0)
                continue;

            var typeSources = FilterSources(allyVillages, commandType, settings.NobleSettings);
            GenerateForType(typeSources, typeTargets, commandType, settings);
        }
    }

    // -------------------------------------------------------------------------
    // Core algorithm
    // -------------------------------------------------------------------------

    private void GenerateForType(
        List<SourceVillage> sources,
        List<Target> targets,
        CommandType commandType,
        ActionSettings settings)
    {
        var potentialPerTarget = BuildPotentialCommands(sources, targets, commandType, settings);
        var remaining = targets.ToHashSet();

        while (remaining.Count > 0)
        {
            var mostConstrained = remaining
                .OrderBy(t => limitsChecker.CountAllowed(potentialPerTarget[t], settings.PlayerNobleBudgets))
                .First();

            var potencialCommands = limitsChecker.GetAllowed(
                potentialPerTarget[mostConstrained], settings.PlayerNobleBudgets);

            var selected = commandType == CommandType.RandomNoble
                ? potencialCommands.Shuffle().Take((int)mostConstrained.CommandNumber).ToList()
                : potencialCommands
                    .OrderByDescending(cmd => cmd.MinimalDepartureTime) // closest village = latest possible departure
                    .Take((int)mostConstrained.CommandNumber)
                    .ToList();

            // Add immediately so the next iteration's eligibility checks reflect the current state
            storage.Add(selected);
            remaining.Remove(mostConstrained);
        }
    }

    // -------------------------------------------------------------------------
    // Potential command building
    // -------------------------------------------------------------------------

    private static Dictionary<Target, List<AttackCommand>> BuildPotentialCommands(
        IReadOnlyList<SourceVillage> sources,
        List<Target> targets,
        CommandType commandType,
        ActionSettings settings)
    {
        var result = new Dictionary<Target, List<AttackCommand>>();

        Parallel.ForEach(targets, target =>
        {
            var commands = sources
                .Select(s => (source: s, command: CommandFactory.Create(s, target)))
                .Where(x => IsCommandAvailable(x.source, x.command, commandType, settings))
                .Select(x => x.command)
                .ToList();

            lock (result)
            {
                result[target] = commands;
            }
        });

        return result;
    }

    private static bool IsCommandAvailable(
        SourceVillage source,
        AttackCommand command,
        CommandType commandType,
        ActionSettings settings)
    {
        if (command.MinimalDepartureTime < settings.MinDepartureTime)
            return false;

        if (settings.SkipNightSendings && NightTimeHelper.IsNightTime(command.MinimalDepartureTime) 
                && command.Target.MinArrivalTime - command.MinimalDepartureTime > new TimeSpan(24, 0, 0)) //in last night sending noble is allow.
            return false;

        var distance = source.Coordinates.CalculateDistance(command.Target.Coordinates);
        if (distance > settings.MaxNobleDistance)
            return false;

        if (IsOffTypeNoble(commandType)
            && source.OffComeBackTime.HasValue
            && command.MinimalDepartureTime < source.OffComeBackTime.Value)
            return false;

        return true;
    }

    // -------------------------------------------------------------------------
    // Source filtering
    // -------------------------------------------------------------------------

    private List<SourceVillage> FilterSources(
        IReadOnlyList<SourceVillage> sources,
        CommandType commandType,
        NobleSettings settings) => commandType switch
    {
        CommandType.NobleWithFullOff or
        CommandType.NobleWithHalfOff or
        CommandType.NobleWithQuarterOffensive =>
            sources.Where(v => v.Army.OffensivePotential >= settings.MinOffUnitsForOffNoble
                            && v.DistanceToFront >= settings.MinDistanceFromFront).ToList(),

        CommandType.NobleWith150Axes =>
            sources.Where(v => v.Army.OffensivePotential >= settings.MinOffUnitsForFakeOffNoble
                            && v.DistanceToFront >= settings.MinDistanceFromFront).ToList(),

        CommandType.NobleWith100HeavyCavalry or
        CommandType.NobleWithDeff =>
            sources.Where(v => v.Army.OffensivePotential < settings.MaxOffUnitsForDefNoble
                            && v.Army.DefensivePotential >= settings.MinDeffUnitsForDefNoble
                            && v.DistanceToFront >= settings.MinDistanceFromFront).ToList(),

        CommandType.RandomNoble =>
            sources.Where(v => v.DistanceToFront >= settings.MinDistanceFromFront).ToList(),

        _ => throw new ArgumentOutOfRangeException(nameof(commandType))
    };

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static bool IsOffTypeNoble(CommandType commandType) => commandType is
        CommandType.NobleWithFullOff or
        CommandType.NobleWithHalfOff or
        CommandType.NobleWithQuarterOffensive;
}
