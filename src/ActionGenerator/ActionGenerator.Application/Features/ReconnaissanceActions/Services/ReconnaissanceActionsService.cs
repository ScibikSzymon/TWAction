using ActionGenerator.Application.Common.Interfaces;
using ActionGenerator.Application.Common.Services;
using ActionGenerator.Application.Features.ReconnaissanceActions.DTOs;
using ActionGenerator.Domain.Common.ValueObjects;
using ActionGenerator.Domain.Configuration;
using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Enums;

namespace ActionGenerator.Application.Features.ReconnaissanceActions.Services;

public interface IReconnaissanceActionsService
{
    Task<GenerateReconnaissanceActionsResponse> GenerateAsync(
        GenerateReconnaissanceActionsRequest request, 
        CancellationToken cancellationToken = default);
}

public sealed class ReconnaissanceActionsService : IReconnaissanceActionsService
{
    private readonly IDistanceCalculator _distanceCalculator;
    private readonly INightTimeChecker _nightTimeChecker;
    private readonly IFrontDistanceCalculator _frontDistanceCalculator;
    private readonly ICommandGenerator _commandGenerator;
    private readonly IPopulationCalculator _populationCalculator;

    public ReconnaissanceActionsService(
        IDistanceCalculator distanceCalculator,
        INightTimeChecker nightTimeChecker,
        IFrontDistanceCalculator frontDistanceCalculator,
        IPopulationCalculator populationCalculator,
        ICommandGenerator commandGenerator)
    {
        _distanceCalculator = distanceCalculator;
        _nightTimeChecker = nightTimeChecker;
        _frontDistanceCalculator = frontDistanceCalculator;
        _populationCalculator = populationCalculator;
        _commandGenerator = commandGenerator;
    }

    public Task<GenerateReconnaissanceActionsResponse> GenerateAsync(
        GenerateReconnaissanceActionsRequest request, 
        CancellationToken cancellationToken = default)
    {
        var allyVillages = MapToSourceVillages(request.AllyVillages);
        var enemyVillages = MapToTargets(request.EnemyVillages, request.MinArrivalTime, request.MaxArrivalTime);

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

        var commandDtos = commands.Select(MapToDto).ToList();

        return Task.FromResult(new GenerateReconnaissanceActionsResponse
        {
            Commands = commandDtos
        });
    }

    private static List<SourceVillage> MapToSourceVillages(IReadOnlyList<VillageDto> dtos)
    {
        return dtos.Select(dto => new SourceVillage
        {
            Id = dto.Id,
            PlayerId = dto.PlayerId,
            Coordinates = new Coordinates { X = dto.X, Y = dto.Y },
            Army = new Army
            {
                Spy = dto.Army.Spy,
                Spear = dto.Army.Spear,
                //dopisz chacie
            }
        }).ToList();
    }
    private static List<Target> MapToTargets(IReadOnlyList<VillageSmallDto> dtos, DateTimeOffset minArrivalTime, DateTimeOffset maxArrivalTime)
    {
        return dtos.Select(dto => new Target
        {
            Id = dto.Id,
            PlayerId = dto.PlayerId,
            Coordinates = new Coordinates { X = dto.X, Y = dto.Y },
            MinArrivalTime = minArrivalTime,
            MaxArrivalTime = maxArrivalTime,
            CommandType = CommandType.Reconnaissance
        }).ToList();
    }
    private List<SourceVillage> FilterAllyVillages(
        List<SourceVillage> allyVillages,
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

                var command = _commandGenerator.Generate(
                    allyVillage,
                    enemyVillage);

                if (request.SkipNightSendings && _nightTimeChecker.IsNightTime(command.TimeWindow.MinDepartureTime))
                    continue;

                if (command.TimeWindow.MinDepartureTime < request.MinDepartureTime)
                    continue;

                commands.Add(command);
                usedAllyVillages.Add(allyVillage.Id);
                break;
            }
        }

        return commands;
    }

    private static AttackCommandDto MapToDto(AttackCommand command)
    {
        return new AttackCommandDto
        {
            TimeWindow = command.TimeWindow,
            Source = MapToSmallVillageDto(command.Source),
            Destination = MapToSmallVillageDto(command.Destination),
        };
    }

    private static VillageSmallDto MapToSmallVillageDto(Village village)
    {
        return new VillageSmallDto
        {
            Id = village.Id,
            PlayerId = village.PlayerId,
            X = village.Coordinates.X,
            Y = village.Coordinates.Y
        };
    }
}
