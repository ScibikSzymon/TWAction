using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Enums;
using ActionGenerator.Domain.Settings;

namespace ActionGenerator.MainAction.Generators;

/// <summary>
/// Generates FakeOffensive and FakeDefensive commands.
/// Targets are grouped by destination player and filled from the least-loaded sources first.
///
/// FakeOffensive (uniquePerPlayer = true):
///   A source that already has any command targeting the same player is excluded,
///   making the fake harder to distinguish from the real attack.
///
/// FakeDefensive (uniquePerPlayer = false):
///   Sources can be reused across different players and are never globally consumed.
/// </summary>
internal sealed class FakeGenerator : ICommandTypeGenerator
{
    private const double MinFakeDistanceFields = 20.0;

    public IReadOnlyList<AttackCommand> Generate(
        IReadOnlyList<SourceVillage> allyVillages,
        IReadOnlyList<Target> targets,
        ActionSettings settings,
        IReadOnlyList<AttackCommand> alreadyGenerated)
    {
        var result = new List<AttackCommand>();

        var fakeOffTargets = targets.Where(t => t.CommandType == CommandType.FakeOffensive).ToList();
        if (fakeOffTargets.Count > 0)
        {
            var sources = FilterFakeOffSources(allyVillages, settings.FakeOffSettings)
                .Shuffle();
            var commands = GenerateFakes(sources, fakeOffTargets, settings, alreadyGenerated, uniquePerPlayer: true);
            result.AddRange(commands);
        }

        var fakeDeffTargets = targets.Where(t => t.CommandType == CommandType.FakeDefensive).ToList();
        if (fakeDeffTargets.Count > 0)
        {
            var sources = FilterFakeDeffSources(allyVillages, settings.FakeDeffSettings)
                .Shuffle();
            var commands = GenerateFakes(sources, fakeDeffTargets, settings, alreadyGenerated, uniquePerPlayer: false);
            result.AddRange(commands);
        }

        return result;
    }

    private static List<SourceVillage> FilterFakeOffSources(
        IReadOnlyList<SourceVillage> sources,
        FakeOffSettings settings)
    {
        return sources
            .Where(v => v.Army.OffensivePotential >= settings.MinOffUnits
                     && v.DistanceToFront >= settings.MinDistanceFromFront
                     && (v.Army.Ram > settings.MinMachineUnits || v.Army.Catapult > settings.MinMachineUnits))
            .ToList();
    }

    private static List<SourceVillage> FilterFakeDeffSources(
        IReadOnlyList<SourceVillage> sources,
        FakeDeffSettings settings)
    {
        return sources
            .Where(v => v.Army.OffensivePotential < settings.MaxOffUnits
                     && v.Army.TotalPotential > settings.MinTotalUnits
                     && v.DistanceToFront > settings.MinDistanceFromFront
                     && (v.Army.Ram > settings.MinMachineUnits || v.Army.Catapult > settings.MinMachineUnits))
            .ToList();
    }

    private static List<AttackCommand> GenerateFakes(
        List<SourceVillage> sources,
        List<Target> targets,
        ActionSettings settings,
        IReadOnlyList<AttackCommand> alreadyGenerated,
        bool uniquePerPlayer)
    {
        var result = new List<AttackCommand>();
        var commandCountPerSource = sources.ToDictionary(v => v.Id, _ => 0);

        var groupedByPlayer = targets
            .GroupBy(t => t.PlayerId)
            .OrderBy(g => g.Sum(t => t.CommandNumber));

        foreach (var playerGroup in groupedByPlayer)
        {
            var sourcesForPlayer = BuildSourcesForPlayer(
                sources,
                playerGroup.Key,
                alreadyGenerated,
                commandCountPerSource,
                uniquePerPlayer);

            foreach (var target in playerGroup)
            {
                for (int i = 0; i < (int)target.CommandNumber; i++)
                {
                    var command = FindSource(sourcesForPlayer, target, settings, commandCountPerSource);
                    if (command is null)
                        break;

                    result.Add(command);
                    commandCountPerSource[command.Source.Id]++;

                    if (uniquePerPlayer)
                        sourcesForPlayer.RemoveAll(v => v.Id == command.Source.Id);
                }
            }
        }

        return result;
    }

    private static List<SourceVillage> BuildSourcesForPlayer(
        List<SourceVillage> allSources,
        int playerId,
        IReadOnlyList<AttackCommand> alreadyGenerated,
        Dictionary<int, int> commandCountPerSource,
        bool uniquePerPlayer)
    {
        HashSet<int>? excludedSourceIds = null;
        if (uniquePerPlayer)
        {
            excludedSourceIds = alreadyGenerated
                .Where(cmd => cmd.Target.PlayerId == playerId)
                .Select(cmd => cmd.Source.Id)
                .ToHashSet();
        }

        return allSources
            .Where(v => excludedSourceIds == null || !excludedSourceIds.Contains(v.Id))
            .OrderBy(v => commandCountPerSource[v.Id])
            .ToList();
    }

    private static AttackCommand? FindSource(
        List<SourceVillage> sortedSources,
        Target target,
        ActionSettings settings,
        Dictionary<int, int> commandCountPerSource)
    {
        foreach (var source in sortedSources.OrderBy(v => commandCountPerSource[v.Id]))
        {
            var command = CommandFactory.Create(source, target);

            if (command.MinimalDepartureTime < settings.MinDepartureTime)
                continue;

            if (source.Coordinates.CalculateDistance(target.Coordinates) < MinFakeDistanceFields)
                continue;

            if (settings.SkipNightSendings && NightTimeHelper.IsNightTime(command.MinimalDepartureTime))
                continue;

            return command;
        }

        return null;
    }
}
