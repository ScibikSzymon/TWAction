using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Enums;
using ActionGenerator.Domain.Settings;

namespace ActionGenerator.MainAction.Generators;

/// <summary>
/// Generates noble commands using a most-constrained-first algorithm.
///
/// Off-type nobles (Full/Half/Quarter/150Axes/100HeavyCavalry):
///   The closest eligible source is always chosen, which means the command
///   departs as late as possible (shortest travel time = latest departure window).
///
/// RandomNoble:
///   Most-constrained-first target selection, but the source village is picked at random
///   from all eligible villages.
///
/// Noble limits are tracked per village (Army.Noble) and per source-to-player pair.
/// Per-type caps (e.g. max 1 per village for FullOff, max 2 for HalfOff) are game rules
/// encoded as constants and are not configurable.
/// </summary>
internal sealed partial class NobleGenerator : ICommandTypeGenerator
{
    private static readonly IReadOnlyList<CommandType> NobleCommandTypes =
    [
        CommandType.NobleWithFullOff,
        CommandType.NobleWithHalfOff,
        CommandType.NobleWithQuarterOffensive,
        CommandType.NobleWith150Axes,
        CommandType.NobleWith100HeavyCavalry,
        CommandType.RandomNoble,
    ];

    public IReadOnlyList<AttackCommand> Generate(
        IReadOnlyList<SourceVillage> allyVillages,
        IReadOnlyList<Target> targets,
        ActionSettings settings,
        IReadOnlyList<AttackCommand> alreadyGenerated)
    {
        var result = new List<AttackCommand>();

        // Shared state — persists across all command types so later types see earlier assignments
        var totalNoblesUsed = BuildTotalNoblesUsed(alreadyGenerated);
        var noblesUsedPerPlayer = BuildNoblesUsedPerPlayer(alreadyGenerated);
        var playerBudgetUsed = BuildPlayerBudgetUsed(alreadyGenerated);

        foreach (var commandType in NobleCommandTypes)
        {
            var typeTargets = targets.Where(t => t.CommandType == commandType).ToList();
            if (typeTargets.Count == 0)
                continue;

            var typeSources = FilterSources(allyVillages, commandType, settings.NobleSettings, totalNoblesUsed);

            // Tracker is created fresh per type so the type-local cap resets each iteration,
            // while the shared dictionaries keep accumulating across types.
            var tracker = new NobleTracker(
                totalNoblesUsed,
                noblesUsedPerPlayer,
                playerBudgetUsed,
                settings.PlayerNobleBudgets,
                MaxNoblesPerVillageForType(commandType),
                MaxNoblesPerVillagePerPlayerForType(commandType));

            result.AddRange(GenerateForType(typeSources, typeTargets, commandType, settings, tracker));
        }

        return result;
    }

    // -------------------------------------------------------------------------
    // Core algorithm
    // -------------------------------------------------------------------------

    private static List<AttackCommand> GenerateForType(
        List<SourceVillage> sources,
        List<Target> targets,
        CommandType commandType,
        ActionSettings settings,
        NobleTracker tracker)
    {
        var result = new List<AttackCommand>();
        var potentialPerTarget = BuildPotentialCommands(sources, targets, commandType, settings);
        var remaining = targets.ToHashSet();

        while (remaining.Count > 0)
        {
            var mostConstrained = remaining
                .OrderBy(t => tracker.CountEligible(potentialPerTarget[t]))
                .First();

            var eligible = tracker.GetEligible(potentialPerTarget[mostConstrained]);

            var selected = commandType == CommandType.RandomNoble
                ? eligible.Shuffle().Take((int)mostConstrained.CommandNumber).ToList()
                : eligible
                    .OrderByDescending(cmd => cmd.MinimalDepartureTime) // closest village = latest possible departure
                    .Take((int)mostConstrained.CommandNumber)
                    .ToList();

            foreach (var cmd in selected)
                tracker.Record(cmd);

            result.AddRange(selected);
            remaining.Remove(mostConstrained);
        }

        return result;
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

        if (settings.SkipNightSendings && NightTimeHelper.IsNightTime(command.MinimalDepartureTime))
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

    private static List<SourceVillage> FilterSources(
        IReadOnlyList<SourceVillage> sources,
        CommandType commandType,
        NobleSettings settings,
        Dictionary<int, uint> totalNoblesUsed) => commandType switch
    {
        CommandType.NobleWithFullOff or
        CommandType.NobleWithHalfOff or
        CommandType.NobleWithQuarterOffensive =>
            sources.Where(v => HasNoblesLeft(v, totalNoblesUsed)
                            && v.Army.OffensivePotential >= settings.MinOffUnitsForOffNoble
                            && v.DistanceToFront >= settings.MinDistanceFromFront).ToList(),

        CommandType.NobleWith150Axes =>
            sources.Where(v => HasNoblesLeft(v, totalNoblesUsed)
                            && v.Army.OffensivePotential >= settings.MinOffUnitsForFakeOffNoble
                            && v.DistanceToFront >= settings.MinDistanceFromFront).ToList(),

        CommandType.NobleWith100HeavyCavalry =>
            sources.Where(v => HasNoblesLeft(v, totalNoblesUsed)
                            && v.Army.OffensivePotential < settings.MaxOffUnitsForDefNoble
                            && v.DistanceToFront >= settings.MinDistanceFromFront).ToList(),

        CommandType.RandomNoble =>
            sources.Where(v => HasNoblesLeft(v, totalNoblesUsed)
                            && v.DistanceToFront >= settings.MinDistanceFromFront).ToList(),

        _ => throw new ArgumentOutOfRangeException(nameof(commandType))
    };

    // -------------------------------------------------------------------------
    // Tracking helpers
    // -------------------------------------------------------------------------

    private static Dictionary<int, uint> BuildTotalNoblesUsed(IReadOnlyList<AttackCommand> commands)
        => commands
            .Where(cmd => NobleCommandTypes.Contains(cmd.Target.CommandType))
            .GroupBy(cmd => cmd.Source.Id)
            .ToDictionary(g => g.Key, g => (uint)g.Count());

    private static Dictionary<SourcePlayerKey, uint> BuildNoblesUsedPerPlayer(
        IReadOnlyList<AttackCommand> commands)
        => commands
            .Where(cmd => NobleCommandTypes.Contains(cmd.Target.CommandType))
            .GroupBy(cmd => new SourcePlayerKey(cmd.Source.Id, cmd.Target.PlayerId))
            .ToDictionary(g => g.Key, g => (uint)g.Count());

    private static Dictionary<int, uint> BuildPlayerBudgetUsed(IReadOnlyList<AttackCommand> commands)
        => commands
            .Where(cmd => NobleCommandTypes.Contains(cmd.Target.CommandType))
            .GroupBy(cmd => cmd.Source.PlayerId)
            .ToDictionary(g => g.Key, g => (uint)g.Count());

    private static bool HasNoblesLeft(SourceVillage source, Dictionary<int, uint> totalNoblesUsed)
        => totalNoblesUsed.GetValueOrDefault(source.Id) < source.Army.Noble;

    // -------------------------------------------------------------------------
    // Game-rule constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// Maximum noble commands per village for a given type, derived from army composition logic:
    /// FullOff sends the entire army → only 1 slot; QuarterOff sends ¼ → up to 4 slots, etc.
    /// </summary>
    private static uint MaxNoblesPerVillageForType(CommandType commandType) => commandType switch
    {
        CommandType.NobleWithFullOff            => 1,
        CommandType.NobleWithHalfOff            => 2,
        CommandType.NobleWithQuarterOffensive   => 4,
        CommandType.NobleWith150Axes            => 6,
        CommandType.NobleWith100HeavyCavalry    => 6,
        CommandType.RandomNoble                 => uint.MaxValue, // capped only by Army.Noble
        _ => throw new ArgumentOutOfRangeException(nameof(commandType))
    };

    /// <summary>
    /// Maximum noble commands one village may send toward the same destination player,
    /// per command type. Mirrors the maxAttackOnPlayerFromVillage parameter from the old generator.
    /// </summary>
    private static uint MaxNoblesPerVillagePerPlayerForType(CommandType commandType) => commandType switch
    {
        CommandType.NobleWithFullOff            => 1,
        CommandType.NobleWithHalfOff            => 2,
        CommandType.NobleWithQuarterOffensive   => 4,
        CommandType.NobleWith150Axes            => 2,
        CommandType.NobleWith100HeavyCavalry    => 2,
        CommandType.RandomNoble                 => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(commandType))
    };

    private static bool IsOffTypeNoble(CommandType commandType) => commandType is
        CommandType.NobleWithFullOff or
        CommandType.NobleWithHalfOff or
        CommandType.NobleWithQuarterOffensive;
}