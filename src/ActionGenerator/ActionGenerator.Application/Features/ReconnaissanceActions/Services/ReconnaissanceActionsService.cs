using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Application.Common.Interfaces;
using ActionGenerator.Application.Common.Mappers;
using ActionGenerator.Application.Common.Services;
using ActionGenerator.Application.Features.ReconnaissanceActions.DTOs;
using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Enums;

namespace ActionGenerator.Application.Features.ReconnaissanceActions.Services;

public interface IReconnaissanceActionsService
{
    Task<IReadOnlyList<AttackCommandDto>> GenerateAsync(
        GenerateReconnaissanceActionsRequest request, 
        CancellationToken cancellationToken = default);
}

public sealed class ReconnaissanceActionsService : IReconnaissanceActionsService
{
    private readonly INightTimeChecker _nightTimeChecker;
    private readonly IFrontDistanceCalculator _frontDistanceCalculator;
    private readonly ICommandGenerator _commandGenerator;
    private readonly IPopulationCalculator _populationCalculator;

    public ReconnaissanceActionsService(
        INightTimeChecker nightTimeChecker,
        IFrontDistanceCalculator frontDistanceCalculator,
        IPopulationCalculator populationCalculator,
        ICommandGenerator commandGenerator)
    {
        _nightTimeChecker = nightTimeChecker;
        _frontDistanceCalculator = frontDistanceCalculator;
        _populationCalculator = populationCalculator;
        _commandGenerator = commandGenerator;
    }

    public async Task<IReadOnlyList<AttackCommandDto>> GenerateAsync(
        GenerateReconnaissanceActionsRequest request, 
        CancellationToken cancellationToken = default)
    {
        var allyVillages = SourceVillageMapper.ToEntities(request.AllyVillages);
        var enemyVillages = TargetMapper.ToEntities(
            request.EnemyVillages, 
            request.MinArrivalTime, 
            request.MaxArrivalTime,
            CommandType.Reconnaissance);

        _frontDistanceCalculator.CalculateFrontDistances(allyVillages, enemyVillages);

        var eligibleAllyVillages = FilterAllyVillages(
            allyVillages,
            request.MinDistanceToFront,
            request.MinSpyCount,
            request.MaxPopulationInSourceVillage);

        var sortedEnemyVillages = enemyVillages
            .OrderBy(v => v.DistanceToFront)
            .ToList();

        var commands = GenerateCommands(
            eligibleAllyVillages,
            sortedEnemyVillages,
            request,
            cancellationToken);

        var commandDtos = AttackCommandMapper.ToDtos(commands);

        return await Task.FromResult(commandDtos);
    }

    private List<SourceVillage> FilterAllyVillages(
        IReadOnlyList<SourceVillage> allyVillages,
        int minDistanceToFront,
        int minSpyCount,
        int maxPopulation)
    {
        return allyVillages
            .Where(v => v.DistanceToFront > minDistanceToFront
                && v.Army.Spy > minSpyCount
                && _populationCalculator.CalculatePopulation(v.Army) < maxPopulation)
            .OrderByDescending(v => v.Army.Spy)
            .ToList();
    }

    private List<AttackCommand> GenerateCommands(
        List<SourceVillage> allyVillages,
        List<Target> enemyVillages,
        GenerateReconnaissanceActionsRequest request,
        CancellationToken cancellationToken)
    {
        var commands = new List<AttackCommand>();
        var usedAllyVillages = new HashSet<int>();

        foreach (var enemyVillage in enemyVillages)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            foreach (var allyVillage in allyVillages)
            {
                if (usedAllyVillages.Contains(allyVillage.Id))
                    continue;

                var command = _commandGenerator.Generate(allyVillage, enemyVillage);

                if (request.SkipNightSendings && _nightTimeChecker.IsNightTime(command.MinimalDepartureTime))
                    continue;

                if (command.MinimalDepartureTime < request.MinDepartureTime)
                    continue;

                commands.Add(command);
                usedAllyVillages.Add(allyVillage.Id);
                break;
            }
        }

        return commands;
    }
}

