namespace TWAction.Application.MainActions.Services;

public sealed class TroopsStateStatsExtractor
{
    /// <summary>
    /// Extracts village count and unique player count from parsed troops data
    /// </summary>
    public TroopsStateStats Extract(ParsedTroopsData parsedData)
    {
        var villageCount = parsedData.DataRows
            .Select(row => row[1].Trim()) // Village coordinates are at column 1
            .Distinct()
            .Count();

        var uniquePlayers = parsedData.DataRows
            .Select(row => row[0].Trim()) // Player name is at column 0
            .Distinct()
            .Count();

        return new TroopsStateStats
        {
            VillageCount = villageCount,
            PlayerCount = uniquePlayers
        };
    }
}

public sealed class TroopsStateStats
{
    public required int VillageCount { get; init; }

    public required int PlayerCount { get; init; }
}

