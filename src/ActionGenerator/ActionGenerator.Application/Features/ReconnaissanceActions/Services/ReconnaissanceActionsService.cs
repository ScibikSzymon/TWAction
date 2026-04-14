using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Application.Common.Mappers;
using ActionGenerator.Application.Common.Services;
using ActionGenerator.Application.Features.ReconnaissanceActions.DTOs;
using ActionGenerator.Domain.Entities;
using Microsoft.Extensions.Logging;


namespace ActionGenerator.Application.Features.ReconnaissanceActions.Services;


public interface IReconnaissanceActionsService
{
    IReadOnlyList<AttackCommandDto> Generate(
        GenerateReconnaissanceActionsRequest request, 
        CancellationToken cancellationToken = default);
}

internal sealed partial class ReconnaissanceActionsService : IReconnaissanceActionsService
{
    private readonly INightTimeChecker _nightTimeChecker;
    private readonly IFrontDistanceCalculator _frontDistanceCalculator;
    private readonly ICommandFactory _commandGenerator;
    private readonly ILogger<ReconnaissanceActionsService> _logger;

    public ReconnaissanceActionsService(
        INightTimeChecker nightTimeChecker,
        IFrontDistanceCalculator frontDistanceCalculator,
        ICommandFactory commandFactory,
        ILogger<ReconnaissanceActionsService> logger)
    {
        _nightTimeChecker = nightTimeChecker;
        _frontDistanceCalculator = frontDistanceCalculator;
        _commandGenerator = commandFactory;
        _logger = logger;
    }

    public IReadOnlyList<AttackCommandDto> Generate(
        GenerateReconnaissanceActionsRequest request, 
        CancellationToken cancellationToken = default)
    {
        LogGenerationStarted(request.AllyVillages.Count, request.EnemyVillages.Count);

        var allyVillages = request.AllyVillages.ToEntities();
        var enemyVillages = request.EnemyVillages.ToEntities(
            request.MinArrivalTime, 
            request.MaxArrivalTime);

        _frontDistanceCalculator.CalculateFrontDistances(allyVillages, enemyVillages);

        // Filter eligible ally villages
        var eligibleAllyVillages = FilterAllyVillages(
            allyVillages,

            request.MinDistanceToFront,
            request.MinSpyCount,
            request.MaxPopulationInSourceVillage);

        LogEligibleAllyVillages(eligibleAllyVillages.Count, allyVillages.Count);

        var sortedEnemyVillages = enemyVillages
            .OrderBy(v => v.DistanceToFront)
            .ToList();

        var commands = GenerateCommands(
            eligibleAllyVillages,
            sortedEnemyVillages,
            request,
            cancellationToken);

        var commandDtos = commands.ToDtos();

        LogGenerationCompleted(commandDtos.Count, sortedEnemyVillages.Count);

        return commandDtos;
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
                && v.Army.TotalPotential < maxPopulation)
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting reconnaissance actions generation with {AllyCount} ally and {EnemyCount} enemy villages")]
    private partial void LogGenerationStarted(int allyCount, int enemyCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "{EligibleCount} of {TotalCount} ally villages eligible after filtering")]
    private partial void LogEligibleAllyVillages(int eligibleCount, int totalCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Generation completed: {CommandCount} commands generated for {TargetCount} enemy targets")]
    private partial void LogGenerationCompleted(int commandCount, int targetCount);
}

