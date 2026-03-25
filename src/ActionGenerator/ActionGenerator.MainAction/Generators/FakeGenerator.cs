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
///
/// Commands are added to storage immediately as they are generated so that subsequent
/// player groups and FindSource re-orderings always see the current total load per village.
/// </summary>
internal sealed class FakeGenerator(ICommandsStorage storage) : ICommandTypeGenerator
{
    private const double MinFakeDistanceFields = 20.0;

    public void Generate(
        IReadOnlyList<SourceVillage> allyVillages,
        IReadOnlyList<Target> targets,
        ActionSettings settings)
    {
        var fakeOffTargets = targets.Where(t => t.CommandType == CommandType.FakeOffensive).ToList();
        if (fakeOffTargets.Count > 0)
        {
            var sources = FilterFakeOffSources(allyVillages, settings.FakeOffSettings).Shuffle();
            GenerateFakes(sources, fakeOffTargets, settings, uniquePerPlayer: true);
        }

        var fakeDeffTargets = targets.Where(t => t.CommandType == CommandType.FakeDefensive).ToList();
        if (fakeDeffTargets.Count > 0)
        {
            var sources = FilterFakeDeffSources(allyVillages, settings.FakeDeffSettings).Shuffle();
            GenerateFakes(sources, fakeDeffTargets, settings, uniquePerPlayer: false);
        }
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

    private void GenerateFakes(
        List<SourceVillage> sources,
        List<Target> targets,
        ActionSettings settings,
        bool uniquePerPlayer)
    {
        var groupedByPlayer = targets
            .GroupBy(t => t.PlayerId)
            .OrderBy(g => g.Sum(t => (int)t.CommandNumber));

        foreach (var playerGroup in groupedByPlayer)
        {
            var sourcesForPlayer = BuildSourcesForPlayer(sources, playerGroup.Key, uniquePerPlayer);

            foreach (var target in playerGroup)
            {
                for (int i = 0; i < (int)target.CommandNumber; i++)
                {
                    var command = FindSource(sourcesForPlayer, target, settings);
                    if (command is null)
                        break;

                    storage.Add([command]);

                    if (uniquePerPlayer)
                        sourcesForPlayer.RemoveAll(v => v.Id == command.Source.Id);
                    else
                        BubbleBackByLoad(sourcesForPlayer, command.Source.Id);
                }
            }
        }
    }

    private List<SourceVillage> BuildSourcesForPlayer(
        List<SourceVillage> allSources,
        int playerId,
        bool uniquePerPlayer)
    {
        HashSet<int>? excludedSourceIds = null;
        if (uniquePerPlayer)
        {
            excludedSourceIds = storage.Commands
                .Where(cmd => cmd.Target.PlayerId == playerId)
                .Select(cmd => cmd.Source.Id)
                .ToHashSet();
        }

        var result = allSources
            .Where(v => excludedSourceIds == null || !excludedSourceIds.Contains(v.Id))
            .ToList();

        SortByLoad(result);
        return result;
    }

    private void SortByLoad(List<SourceVillage> sources) =>
        sources.Sort((a, b) =>
            storage.GetCommandsFromSource(a.Id).Count
                .CompareTo(storage.GetCommandsFromSource(b.Id).Count));

    /// <summary>
    /// After one command is added, only the source that was just used has a higher count.
    /// The list is otherwise sorted — so we only need to bubble that one element
    /// rightward until it reaches its correct position. O(k) swaps where k is typically 0–1.
    /// </summary>
    private void BubbleBackByLoad(List<SourceVillage> sources, int updatedSourceId)
    {
        var index = sources.FindIndex(v => v.Id == updatedSourceId);
        if (index < 0)
            return;

        var updatedCount = storage.GetCommandsFromSource(updatedSourceId).Count;
        while (index + 1 < sources.Count &&
               storage.GetCommandsFromSource(sources[index + 1].Id).Count < updatedCount)
        {
            (sources[index], sources[index + 1]) = (sources[index + 1], sources[index]);
            index++;
        }
    }

    private AttackCommand? FindSource(
        List<SourceVillage> sources,
        Target target,
        ActionSettings settings)
    {
        foreach (var source in sources)
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

