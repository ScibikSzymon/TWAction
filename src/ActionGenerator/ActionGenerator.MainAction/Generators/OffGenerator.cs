using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Enums;
using ActionGenerator.Domain.Settings;

namespace ActionGenerator.MainAction.Generators;

/// <summary>
/// Generates Off and Catapults commands using a greedy "most-constrained-first" algorithm.
/// For each round it picks the target with the fewest available source candidates and
/// assigns the closest eligible sources up to the required command count.
/// </summary>
internal sealed class OffGenerator : ICommandTypeGenerator
{
    public IReadOnlyList<AttackCommand> Generate(
        IReadOnlyList<SourceVillage> allyVillages,
        IReadOnlyList<Target> targets,
        ActionSettings settings,
        IReadOnlyList<AttackCommand> alreadyGenerated)
    {
        var alreadyUsedSourceIds = alreadyGenerated
            .Select(c => c.Source.Id)
            .ToHashSet();

        var result = new List<AttackCommand>();

        var offTargets = targets.Where(t => t.CommandType == CommandType.Off).ToList();
        if (offTargets.Count > 0)
        {
            var offSources = FilterOffSources(allyVillages, settings.OffSettings, alreadyUsedSourceIds);
            result.AddRange(GenerateOptimal(offSources, offTargets, settings));
        }

        var catasTargets = targets.Where(t => t.CommandType == CommandType.Catapults).ToList();
        if (catasTargets.Count > 0)
        {
            var catasSources = FilterCatasSources(allyVillages, settings.CatasSettings, []);
            result.AddRange(GenerateOptimal(catasSources, catasTargets, settings));
        }

        return result;
    }

    private static List<SourceVillage> FilterOffSources(
        IReadOnlyList<SourceVillage> sources,
        OffSettings settings,
        HashSet<int> excludedIds)
    {
        return sources
            .Where(v => v.Army.OffensivePotential >= settings.MinOffUnits
                     && v.DistanceToFront >= settings.MinDistanceFromFront
                     && !excludedIds.Contains(v.Id))
            .ToList()
            .Shuffle();
    }

    private static List<SourceVillage> FilterCatasSources(
        IReadOnlyList<SourceVillage> sources,
        CatasSettings settings,
        HashSet<int> excludedIds)
    {
        return sources
            .Where(v => v.Army.Catapult >= settings.MinCatasNumber
                     && v.DistanceToFront >= settings.MinDistanceFromFront
                     && v.Army.OffensivePotential < settings.MaxOffUnits
                     && !excludedIds.Contains(v.Id))
            .ToList()
            .Shuffle();
    }

    private static List<AttackCommand> GenerateOptimal(
        List<SourceVillage> sources,
        List<Target> targets,
        ActionSettings settings)
    {
        var potentialCommandsPerTarget = BuildPotentialCommands(sources, targets, settings);
        var usedSourceIds = new HashSet<int>();
        var result = new List<AttackCommand>();
        var remaining = targets.ToHashSet();

        while (remaining.Count > 0)
        {
            var mostConstrained = remaining
                .OrderBy(t => potentialCommandsPerTarget[t].Count(cmd => !usedSourceIds.Contains(cmd.Source.Id)))
                .First();

            var available = potentialCommandsPerTarget[mostConstrained]
                .Where(cmd => !usedSourceIds.Contains(cmd.Source.Id))
                .ToList();

            var selected = SelectClosestFirst(available, (int)mostConstrained.CommandNumber);

            if (selected.Count == 0)
            {
                remaining.Remove(mostConstrained);
                continue;
            }

            foreach (var cmd in selected)
            {
                result.Add(cmd);
                usedSourceIds.Add(cmd.Source.Id);
            }

            remaining.Remove(mostConstrained);
        }

        return result;
    }

    private static Dictionary<Target, List<AttackCommand>> BuildPotentialCommands(
        IReadOnlyList<SourceVillage> sources,
        List<Target> targets,
        ActionSettings settings)
    {
        var result = new Dictionary<Target, List<AttackCommand>>();

        Parallel.ForEach(targets, target =>
        {
            var commands = sources
                .Select(s => (source: s, command: CommandFactory.Create(s, target)))
                .Where(x => IsCommandAvailable(x.source, x.command, settings))
                .Select(x => x.command)
                .ToList();

            lock (result)
            {
                result[target] = commands;
            }
        });

        return result;
    }

    private static bool IsCommandAvailable(SourceVillage source, AttackCommand command, ActionSettings settings)
    {
        if (command.MinimalDepartureTime < settings.MinDepartureTime)
            return false;

        if (settings.SkipNightSendings && NightTimeHelper.IsNightTime(command.MinimalDepartureTime))
            return false;

        if (command.Target.CommandType == CommandType.Catapults
            && source.CatasComeBackTime.HasValue
            && command.MinimalDepartureTime < source.CatasComeBackTime.Value)
            return false;

        if (command.Target.CommandType != CommandType.Catapults
            && source.OffComeBackTime.HasValue
            && command.MinimalDepartureTime < source.OffComeBackTime.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Prefers sources within the preferred distance threshold; falls back to any available source.
    /// </summary>
    private static List<AttackCommand> SelectClosestFirst(List<AttackCommand> available, int count)
    {
        if (available.Count == 0)
            return [];

        const double preferredMaxDistanceFields = 118.0; // ~59 fields * 2

        var closeCommands = available
            .Where(cmd => cmd.Source.Coordinates.CalculateDistance(cmd.Target.Coordinates) < preferredMaxDistanceFields)
            .Take(count)
            .ToList();

        if (closeCommands.Count >= count)
            return closeCommands;

        return available.Take(count).ToList();
    }
}
