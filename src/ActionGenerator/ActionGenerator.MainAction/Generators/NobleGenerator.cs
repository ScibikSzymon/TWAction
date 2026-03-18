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
internal sealed class NobleGenerator : ICommandTypeGenerator
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
        var nobleSettings = settings.NobleSettings;

        // Noble consumption tracking — both initialized from any already-generated noble commands
        var noblesConsumed = BuildNoblesConsumed(alreadyGenerated);
        var noblesConsumedPerPlayer = BuildNoblesConsumedPerPlayer(alreadyGenerated);

        foreach (var commandType in NobleCommandTypes)
        {
            var typeTargets = targets.Where(t => t.CommandType == commandType).ToList();
            if (typeTargets.Count == 0)
                continue;

            var typeSources = FilterSources(allyVillages, commandType, nobleSettings, noblesConsumed);

            var commands = GenerateForType(
                typeSources, typeTargets, commandType, settings, noblesConsumed, noblesConsumedPerPlayer);

            result.AddRange(commands);
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
        Dictionary<int, uint> noblesConsumed,
        Dictionary<(int sourceId, int playerId), uint> noblesConsumedPerPlayer)
    {
        var result = new List<AttackCommand>();
        var maxPerVillageForType = MaxNoblesPerVillageForType(commandType);

        // Tracks how many nobles this village has been assigned within the current type pass
        var localNoblesForType = new Dictionary<int, uint>();

        var potentialPerTarget = BuildPotentialCommands(sources, targets, commandType, settings);
        var remaining = targets.ToHashSet();

        while (remaining.Count > 0)
        {
            // Most-constrained-first: pick the target with the fewest currently eligible sources
            var mostConstrained = remaining
                .OrderBy(t => CountEligible(
                    potentialPerTarget[t],
                    noblesConsumed,
                    localNoblesForType,
                    noblesConsumedPerPlayer,
                    settings.NobleSettings,
                    maxPerVillageForType))
                .First();

            var eligible = GetEligible(
                potentialPerTarget[mostConstrained],
                noblesConsumed,
                localNoblesForType,
                noblesConsumedPerPlayer,
                settings.NobleSettings,
                maxPerVillageForType);

            var selected = commandType == CommandType.RandomNoble
                ? eligible.Shuffle().Take((int)mostConstrained.CommandNumber).ToList()
                : eligible
                    .OrderByDescending(cmd => cmd.MinimalDepartureTime) // closest village = latest possible departure
                    .Take((int)mostConstrained.CommandNumber)
                    .ToList();

            foreach (var cmd in selected)
            {
                result.Add(cmd);
                noblesConsumed[cmd.Source.Id] = noblesConsumed.GetValueOrDefault(cmd.Source.Id) + 1;
                localNoblesForType[cmd.Source.Id] = localNoblesForType.GetValueOrDefault(cmd.Source.Id) + 1;

                var playerKey = (cmd.Source.Id, cmd.Target.PlayerId);
                noblesConsumedPerPlayer[playerKey] = noblesConsumedPerPlayer.GetValueOrDefault(playerKey) + 1;
            }

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
        Dictionary<int, uint> noblesConsumed) => commandType switch
    {
        CommandType.NobleWithFullOff or
        CommandType.NobleWithHalfOff or
        CommandType.NobleWithQuarterOffensive =>
            sources.Where(v => NoblesLeft(v, noblesConsumed) > 0
                            && v.Army.OffensivePotential >= settings.MinOffUnitsForOffNoble
                            && v.DistanceToFront >= settings.MinDistanceFromFront).ToList(),

        CommandType.NobleWith150Axes =>
            sources.Where(v => NoblesLeft(v, noblesConsumed) > 0
                            && v.Army.OffensivePotential >= settings.MinOffUnitsForFakeOffNoble
                            && v.DistanceToFront >= settings.MinDistanceFromFront).ToList(),

        CommandType.NobleWith100HeavyCavalry =>
            sources.Where(v => NoblesLeft(v, noblesConsumed) > 0
                            && v.Army.OffensivePotential < settings.MaxOffUnitsForDefNoble
                            && v.DistanceToFront >= settings.MinDistanceFromFront).ToList(),

        CommandType.RandomNoble =>
            sources.Where(v => NoblesLeft(v, noblesConsumed) > 0
                            && v.DistanceToFront >= settings.MinDistanceFromFront).ToList(),

        _ => throw new ArgumentOutOfRangeException(nameof(commandType))
    };

    // -------------------------------------------------------------------------
    // Eligibility checks
    // -------------------------------------------------------------------------

    private static int CountEligible(
        List<AttackCommand> commands,
        Dictionary<int, uint> noblesConsumed,
        Dictionary<int, uint> localNoblesForType,
        Dictionary<(int, int), uint> noblesConsumedPerPlayer,
        NobleSettings settings,
        uint maxPerVillageForType)
        => commands.Count(cmd => IsEligible(
            cmd, noblesConsumed, localNoblesForType, noblesConsumedPerPlayer, settings, maxPerVillageForType));

    private static List<AttackCommand> GetEligible(
        List<AttackCommand> commands,
        Dictionary<int, uint> noblesConsumed,
        Dictionary<int, uint> localNoblesForType,
        Dictionary<(int, int), uint> noblesConsumedPerPlayer,
        NobleSettings settings,
        uint maxPerVillageForType)
        => commands.Where(cmd => IsEligible(
            cmd, noblesConsumed, localNoblesForType, noblesConsumedPerPlayer, settings, maxPerVillageForType)).ToList();

    private static bool IsEligible(
        AttackCommand command,
        Dictionary<int, uint> noblesConsumed,
        Dictionary<int, uint> localNoblesForType,
        Dictionary<(int, int), uint> noblesConsumedPerPlayer,
        NobleSettings settings,
        uint maxPerVillageForType)
    {
        var sourceId = command.Source.Id;
        var source = (SourceVillage)command.Source;

        // Village must still have nobles in its garrison
        if (noblesConsumed.GetValueOrDefault(sourceId) >= source.Army.Noble)
            return false;

        // Within this command type, respect the type-specific per-village cap
        if (localNoblesForType.GetValueOrDefault(sourceId) >= maxPerVillageForType)
            return false;

        // One village must not flood a single player with noble attacks
        var playerKey = (sourceId, command.Target.PlayerId);
        if (noblesConsumedPerPlayer.GetValueOrDefault(playerKey) >= settings.MaxNoblesPerVillagePerPlayer)
            return false;

        return true;
    }

    // -------------------------------------------------------------------------
    // Tracking helpers
    // -------------------------------------------------------------------------

    private static Dictionary<int, uint> BuildNoblesConsumed(IReadOnlyList<AttackCommand> commands)
        => commands
            .Where(cmd => NobleCommandTypes.Contains(cmd.Target.CommandType))
            .GroupBy(cmd => cmd.Source.Id)
            .ToDictionary(g => g.Key, g => (uint)g.Count());

    private static Dictionary<(int sourceId, int playerId), uint> BuildNoblesConsumedPerPlayer(
        IReadOnlyList<AttackCommand> commands)
        => commands
            .Where(cmd => NobleCommandTypes.Contains(cmd.Target.CommandType))
            .GroupBy(cmd => (cmd.Source.Id, cmd.Target.PlayerId))
            .ToDictionary(g => g.Key, g => (uint)g.Count());

    private static uint NoblesLeft(SourceVillage source, Dictionary<int, uint> noblesConsumed)
        => source.Army.Noble - noblesConsumed.GetValueOrDefault(source.Id);

    // -------------------------------------------------------------------------
    // Game-rule constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// Maximum noble commands per village for a given type, derived from army composition logic:
    /// FullOff sends the entire army ? only 1 slot; QuarterOff sends ? ? up to 4 slots, etc.
    /// </summary>
    private static uint MaxNoblesPerVillageForType(CommandType commandType) => commandType switch
    {
        CommandType.NobleWithFullOff            => 1,
        CommandType.NobleWithHalfOff            => 2,
        CommandType.NobleWithQuarterOffensive   => 4,
        CommandType.NobleWith150Axes            => 4,
        CommandType.NobleWith100HeavyCavalry    => 4,
        CommandType.RandomNoble                 => uint.MaxValue, // capped only by Army.Noble
        _ => throw new ArgumentOutOfRangeException(nameof(commandType))
    };

    private static bool IsOffTypeNoble(CommandType commandType) => commandType is
        CommandType.NobleWithFullOff or
        CommandType.NobleWithHalfOff or
        CommandType.NobleWithQuarterOffensive;
}
