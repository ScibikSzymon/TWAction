using TWAction.Application.Common;
using TWAction.Application.ReconnaissanceActions.Commands;
using TWAction.Application.ReconnaissanceActions.DTOs;
using TWAction.Application.ReconnaissanceActions.Interfaces;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Schedules.Services;
using TWAction.Application.Settings.Interfaces;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.ReconnaissanceActions;
using TWAction.Domain.Tribes;

namespace TWAction.Application.ReconnaissanceActions.Handlers;

/// <summary>
/// Handler for generating reconnaissance actions.
/// Validates prerequisites, calls Generator.Api, and saves results.
/// </summary>
public sealed class GenerateReconnaissanceActionsHandler(
    IScheduleRepository scheduleRepository,
    IReconnaissanceSettingsRepository settingsRepository,
    ITroopsStateRepository troopsStateRepository,
    IAttackCommandRepository attackCommandRepository,
    ITribesService tribesService,
    IGeneratorApiClient generatorApiClient,
    TroopsStateCompressionService compressionService)
{
    public async Task<Result<GenerateReconnaissanceActionsResponse>> Handle(
        GenerateReconnaissanceActionsCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate schedule exists
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<GenerateReconnaissanceActionsResponse>("Schedule not found.");
        }

        // 2. Validate reconnaissance settings exist
        var settings = await settingsRepository.GetByScheduleIdAsync(command.ScheduleId, cancellationToken);
        if (settings is null)
        {
            return Result.Failure<GenerateReconnaissanceActionsResponse>(
                "Reconnaissance settings not found. Please configure settings first.");
        }

        // 3. Validate troops state exists
        var troopsState = await troopsStateRepository.GetByScheduleIdAsync(command.ScheduleId, cancellationToken);
        if (troopsState is null)
        {
            return Result.Failure<GenerateReconnaissanceActionsResponse>(
                "Troops state not found. Please upload troops data first.");
        }

        // 4. Decompress troops data
        var decompressResult = compressionService.Decompress(troopsState.CompressedData);
        if (decompressResult.IsFailure)
        {
            return Result.Failure<GenerateReconnaissanceActionsResponse>(
                $"Failed to decompress troops data: {decompressResult.Error}");
        }

        var rawTroopsData = decompressResult.Value;

        // 5. Parse troops data to get ally villages with armies
        var allyVillages = ParseTroopsDataToVillages(rawTroopsData);

        // 6. Fetch enemy villages from Tribes API
        var (villagesById, _) = await tribesService.GetVillagesAsync(schedule.World, cancellationToken);

        // Get enemy tribe IDs from schedule.Enemies
        var enemyTribeIds = schedule.Enemies.Select(e => e.TribalWarsId).ToList();

        var enemyVillages = GetEnemyVillages(villagesById, enemyTribeIds);

        if (enemyVillages.Count == 0)
        {
            return Result.Failure<GenerateReconnaissanceActionsResponse>(
                "No enemy villages found for the selected tribes.");
        }

        // 7. Prepare request for Generator.Api
        var generatorRequest = new GenerateReconnaissanceActionsRequest
        {
            MinDepartureTime = settings.MinDepartureTime,
            MinArrivalTime = settings.MinArrivalTime,
            MaxArrivalTime = settings.MaxArrivalTime,
            MinDistanceToFront = settings.MinDistanceToFront,
            MinSpyCount = settings.MinSpyCount,
            MaxPopulationInSourceVillage = settings.MaxPopulationInSourceVillage,
            SkipNightSendings = settings.SkipNightSendings,
            AllyVillages = allyVillages,
            EnemyVillages = enemyVillages
        };

        // 8. Call Generator.Api
        IReadOnlyList<AttackCommandDto> generatedCommands;
        try
        {
            generatedCommands = await generatorApiClient.GenerateReconnaissanceActionsAsync(
                generatorRequest,
                cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<GenerateReconnaissanceActionsResponse>(
                $"Failed to call Generator.Api: {ex.Message}");
        }

        // 9. Convert DTOs to entities
        var commandEntities = generatedCommands.Select(dto => new AttackCommandEntity
        {
            Id = Guid.NewGuid(),
            ScheduleId = command.ScheduleId,
            MinDepartureTime = dto.TimeWindow.MinDepartureTime,
            MaxDepartureTime = dto.TimeWindow.MaxDepartureTime,
            MinArrivalTime = dto.TimeWindow.MinArrivalTime,
            MaxArrivalTime = dto.TimeWindow.MaxArrivalTime,
            SourceVillageId = dto.Source.Id,
            SourceX = dto.Source.X,
            SourceY = dto.Source.Y,
            SourcePlayerId = dto.Source.PlayerId,
            DestinationVillageId = dto.Destination.Id,
            DestinationX = dto.Destination.X,
            DestinationY = dto.Destination.Y,
            DestinationPlayerId = dto.Destination.PlayerId,
            CommandType = dto.CommandType,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        // 10. Save to database (replaces old commands)
        await attackCommandRepository.SaveCommandsAsync(
            command.ScheduleId,
            commandEntities,
            cancellationToken);

        return Result.Success(new GenerateReconnaissanceActionsResponse
        {
            GeneratedCommandsCount = commandEntities.Count,
            ScheduleId = command.ScheduleId
        });
    }

    private List<VillageDto> ParseTroopsDataToVillages(string rawData)
    {
        var villages = new List<VillageDto>();
        var lines = rawData.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = line.Split('\t');
            if (parts.Length < 14) continue;

            // Format: VillageId PlayerId X Y Spear Sword Axe Archer Spy Light HorseArcher Heavy Ram Catapult (no Noble in reconnaissance)
            if (!int.TryParse(parts[0], out var villageId)) continue;
            if (!int.TryParse(parts[1], out var playerId)) continue;
            if (!int.TryParse(parts[2], out var x)) continue;
            if (!int.TryParse(parts[3], out var y)) continue;

            var army = new ArmyDto
            {
                Spear = uint.TryParse(parts[4], out var spear) ? spear : 0,
                Sword = uint.TryParse(parts[5], out var sword) ? sword : 0,
                Axe = uint.TryParse(parts[6], out var axe) ? axe : 0,
                Archer = uint.TryParse(parts[7], out var archer) ? archer : 0,
                Spy = uint.TryParse(parts[8], out var spy) ? spy : 0,
                Light = uint.TryParse(parts[9], out var light) ? light : 0,
                HorseArcher = uint.TryParse(parts[10], out var horseArcher) ? horseArcher : 0,
                Heavy = uint.TryParse(parts[11], out var heavy) ? heavy : 0,
                Ram = uint.TryParse(parts[12], out var ram) ? ram : 0,
                Catapult = uint.TryParse(parts[13], out var catapult) ? catapult : 0,
                Noble = 0 // Reconnaissance doesn't use nobles
            };

            villages.Add(new VillageDto
            {
                Id = villageId,
                PlayerId = playerId,
                X = x,
                Y = y,
                Army = army
            });
        }

        return villages;
    }

    private List<VillageSmallDto> GetEnemyVillages(
        Dictionary<int, VillageInfo> allVillages,
        IReadOnlyList<int> enemyTribeIds)
    {
        return allVillages.Values
            .Where(v => v.Player != null && enemyTribeIds.Contains(v.Player.TribeId))
            .Select(v => new VillageSmallDto
            {
                Id = v.Id,
                PlayerId = v.PlayerId,
                X = v.X,
                Y = v.Y
            })
            .ToList();
    }
}
