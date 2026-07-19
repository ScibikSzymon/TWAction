using TWAction.Application.AttackCommands.Interfaces;
using TWAction.Application.Common;
using TWAction.Application.MainActions.Commands;
using TWAction.Application.MainActions.DTOs;
using TWAction.Application.ReconnaissanceActions.DTOs;
using TWAction.Application.ReconnaissanceActions.Interfaces;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Schedules.Services;
using TWAction.Application.Settings.Interfaces;
using TWAction.Application.TargetGroups.Interfaces;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.AttackCommands;
using TWAction.Domain.Schedules;
using TWAction.Domain.TargetGroups;
using TWAction.Domain.Templates;
using TWAction.Domain.Tribes;

namespace TWAction.Application.MainActions.Handlers;

/// <summary>
/// Orchestrates main action generation.
/// Validates all prerequisites, builds the GenerateMainActionRequest from stored data,
/// calls Generator.Api, and persists the resulting commands.
/// </summary>
public sealed class GenerateMainActionsHandler(
    IScheduleRepository scheduleRepository,
    IMainActionSettingsRepository settingsRepository,
    ITroopsStateRepository troopsStateRepository,
    ITargetGroupRepository targetGroupRepository,
    IAttackCommandRepository attackCommandRepository,
    ITribesService tribesService,
    IGeneratorApiClient generatorApiClient,
    TroopsStateCompressionService compressionService)
{
    public async Task<Result<GenerateMainActionsResponse>> Handle(
        GenerateMainActionsCommand command,
        CancellationToken cancellationToken = default)
    {
        // ── 1. Load and validate schedule ────────────────────────────────
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<GenerateMainActionsResponse>("Schedule not found.");
        }

        if (schedule.ScheduleType != ScheduleType.Main)
        {
            return Result.Failure<GenerateMainActionsResponse>(
                "Main action generation is only available for Main-type schedules.");
        }

        // ── 2. Load main action settings ─────────────────────────────────
        var settings = await settingsRepository.GetByScheduleIdAsync(command.ScheduleId, cancellationToken);
        if (settings is null)
        {
            return Result.Failure<GenerateMainActionsResponse>(
                "Main action settings not found. Please configure and save settings first.");
        }

        // ── 3. Load and decompress troops state ───────────────────────────
        var troopsState = await troopsStateRepository.GetByScheduleIdAsync(command.ScheduleId, cancellationToken);
        if (troopsState is null)
        {
            return Result.Failure<GenerateMainActionsResponse>(
                "Troops state not found. Please upload troops data first.");
        }

        var decompressResult = compressionService.Decompress(troopsState.CompressedData);
        if (decompressResult.IsFailure)
        {
            return Result.Failure<GenerateMainActionsResponse>(
                $"Failed to decompress troops data: {decompressResult.Error}");
        }

        // ── 4. Load target groups ─────────────────────────────────────────
        var targetGroups = (await targetGroupRepository.GetAllAsync(command.ScheduleId, cancellationToken)).ToList();
        if (targetGroups.Count == 0)
        {
            return Result.Failure<GenerateMainActionsResponse>(
                "No target groups defined. Create at least one target group before generating.");
        }

        // ── 5. Fetch villages from Tribes API ─────────────────────────────
        var (villagesById, villagesByCoordinates) = await tribesService.GetVillagesAsync(
            schedule.World, cancellationToken);

        // ── 6. Parse ally villages from troops data ───────────────────────
        var allyVillages = ParseAllyVillages(decompressResult.Value, villagesByCoordinates);

        // ── 7. Build target list from target groups ───────────────────────
        var buildResult = BuildTargets(targetGroups, settings.ActionDate, villagesByCoordinates);
        if (buildResult.IsFailure)
        {
            return Result.Failure<GenerateMainActionsResponse>(buildResult.Error);
        }

        var targets = buildResult.Value;
        var targetVillageCount = targetGroups.Sum(g => g.VillageCoordinates.Count);

        // ── 8. Build generator request ────────────────────────────────────
        var generatorRequest = new GenerateMainActionsRequest
        {
            MinDepartureTime = settings.MinDepartureTime,
            SkipNightSendings = settings.SkipNightSendings,
            AllyVillages = allyVillages,
            Targets = targets,
            PlayerNobleBudgets = settings.PlayerNobleBudgets
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            MaxNobleDistance = settings.MaxNobleDistance,
            OffSettings = new OffSettingsDto
            {
                MinOffUnits = settings.OffSettings.MinOffUnits,
                MinDistanceFromFront = settings.OffSettings.MinDistanceFromFront
            },
            CatasSettings = new CatasSettingsDto
            {
                MinCatasNumber = settings.CatasSettings.MinCatasNumber,
                MinDistanceFromFront = settings.CatasSettings.MinDistanceFromFront,
                MaxOffUnits = settings.CatasSettings.MaxOffUnits
            },
            FakeOffSettings = new FakeOffSettingsDto
            {
                MinOffUnits = settings.FakeOffSettings.MinOffUnits,
                MinDistanceFromFront = settings.FakeOffSettings.MinDistanceFromFront
            },
            FakeDeffSettings = new FakeDeffSettingsDto
            {
                MaxOffUnits = settings.FakeDeffSettings.MaxOffUnits,
                MinDistanceFromFront = settings.FakeDeffSettings.MinDistanceFromFront
            },
            NobleSettings = new NobleSettingsDto
            {
                MinDistanceFromFront = settings.NobleSettings.MinDistanceFromFront,
                MinOffUnitsForOffNoble = settings.NobleSettings.MinOffUnitsForOffNoble,
                MinOffUnitsForFakeOffNoble = settings.NobleSettings.MinOffUnitsForFakeOffNoble,
                MaxOffUnitsForDefNoble = settings.NobleSettings.MaxOffUnitsForDefNoble,
                MinDeffUnitsForDefNoble = settings.NobleSettings.MinDeffUnitsForDefNoble
            }
        };

        // ── 9. Call Generator.Api ─────────────────────────────────────────
        IReadOnlyList<AttackCommandDto> generatedCommands;
        try
        {
            generatedCommands = await generatorApiClient.GenerateMainActionsAsync(
                generatorRequest, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<GenerateMainActionsResponse>(
                $"Failed to call Generator.Api: {ex.Message}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            return Result.Failure<GenerateMainActionsResponse>(
                $"Generator.Api request timed out: {ex.Message}");
        }

        // ── 10. Persist results ───────────────────────────────────────────
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
            CommandType = dto.CommandType.ToString(),
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        await attackCommandRepository.SaveCommandsAsync(
            command.ScheduleId, commandEntities, cancellationToken);

        return Result.Success(new GenerateMainActionsResponse
        {
            ScheduleId = command.ScheduleId,
            GeneratedCommandsCount = commandEntities.Count,
            TargetGroupCount = targetGroups.Count,
            TargetVillageCount = targetVillageCount
        });
    }

    /// <summary>
    /// Parses CSV troops data and resolves each row to a VillageDto using the coordinate index.
    /// </summary>
    private static List<VillageDto> ParseAllyVillages(
        string rawData,
        Dictionary<int, VillageInfo> villagesByCoordinates)
    {
        var villages = new List<VillageDto>();
        var lines = rawData.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            // Format: PlayerName,X|Y,Spear,Sword,Spy,Heavy,Catapult,Axe,Light,Ram,Noble
            var parts = line.Split(',');
            if (parts.Length < 11)
            {
                continue;
            }

            var coordParts = parts[1].Trim().Split('|');
            if (coordParts.Length != 2)
            {
                continue;
            }

            if (!int.TryParse(coordParts[0], out var x) || !int.TryParse(coordParts[1], out var y))
            {
                continue;
            }

            var coordinateKey = x * 1000 + y;
            if (!villagesByCoordinates.TryGetValue(coordinateKey, out var villageInfo))
            {
                continue;
            }

            var army = new ArmyDto
            {
                Spear = uint.TryParse(parts[2].Trim(), out var spear) ? spear : 0,
                Sword = uint.TryParse(parts[3].Trim(), out var sword) ? sword : 0,
                Axe = uint.TryParse(parts[7].Trim(), out var axe) ? axe : 0,
                Archer = 0,
                Spy = uint.TryParse(parts[4].Trim(), out var spy) ? spy : 0,
                Light = uint.TryParse(parts[8].Trim(), out var light) ? light : 0,
                HorseArcher = 0,
                Heavy = uint.TryParse(parts[5].Trim(), out var heavy) ? heavy : 0,
                Ram = uint.TryParse(parts[9].Trim(), out var ram) ? ram : 0,
                Catapult = uint.TryParse(parts[6].Trim(), out var catapult) ? catapult : 0,
                Noble = uint.TryParse(parts[10].Trim(), out var noble) ? noble : 0
            };

            villages.Add(new VillageDto
            {
                Id = villageInfo.Id,
                PlayerId = villageInfo.PlayerId,
                X = x,
                Y = y,
                Army = army
            });
        }

        return villages;
    }

    /// <summary>
    /// Converts all target groups into a flat list of TargetDto entries.
    /// For each group: for each village coordinate: for each wave → one TargetDto.
    /// Wave arrival times are derived by combining ActionDate with the wave's MinTime/MaxTime.
    /// </summary>
    private static Result<List<TargetDto>> BuildTargets(
        List<TargetGroup> groups,
        DateOnly actionDate,
        Dictionary<int, VillageInfo> villagesByCoordinates)
    {
        var targets = new List<TargetDto>();
        var unresolvedCoords = new List<string>();

        foreach (var group in groups)
        {
            foreach (var coordStr in group.VillageCoordinates)
            {
                var parts = coordStr.Split('|');
                if (parts.Length != 2
                    || !int.TryParse(parts[0], out var x)
                    || !int.TryParse(parts[1], out var y))
                {
                    unresolvedCoords.Add(coordStr);
                    continue;
                }

                var coordinateKey = x * 1000 + y;
                if (!villagesByCoordinates.TryGetValue(coordinateKey, out var villageInfo))
                {
                    // Village not found in the tribes API — skip but collect for diagnostics
                    unresolvedCoords.Add(coordStr);
                    continue;
                }

                var villageSmall = new VillageSmallDto
                {
                    Id = villageInfo.Id,
                    PlayerId = villageInfo.PlayerId,
                    X = x,
                    Y = y
                };

                foreach (var wave in group.Waves)
                {
                    var minArrival = CombineDateTime(actionDate, wave.MinTime);
                    var maxArrival = CombineDateTime(actionDate, wave.MaxTime);

                    targets.Add(new TargetDto
                    {
                        MinArrivalTime = minArrival,
                        MaxArrivalTime = maxArrival,
                        CommandType = wave.CommandType,
                        CommandNumber = (uint)wave.CommandNumber,
                        Village = villageSmall
                    });
                }
            }
        }

        if (targets.Count == 0)
        {
            var hint = unresolvedCoords.Count > 0
                ? $" Unresolved coordinates: {string.Join(", ", unresolvedCoords.Take(5))}."
                : string.Empty;
            return Result.Failure<List<TargetDto>>(
                $"No valid target villages could be resolved from the target groups.{hint}" +
                " Ensure village coordinates are correct and the tribes data is up to date.");
        }

        return Result.Success(targets);
    }

    /// <summary>
    /// Combines a DateOnly with a TimeOnly to produce a UTC DateTimeOffset.
    /// </summary>
    private static DateTimeOffset CombineDateTime(DateOnly date, TimeOnly time) =>
        new DateTimeOffset(date.ToDateTime(time), TimeSpan.Zero);
}
