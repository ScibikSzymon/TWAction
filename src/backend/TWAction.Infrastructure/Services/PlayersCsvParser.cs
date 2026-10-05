using Microsoft.Extensions.Logging;
using TWAction.Domain.Tribes;

namespace TWAction.Infrastructure.Services;

/// <summary>
/// Parses TribalWars player.txt CSV format (NO HEADER).
/// Format: ID,Name,TribeID,VillagesCount,Points,Rank
/// </summary>
public sealed class PlayersCsvParser(ILogger<PlayersCsvParser> logger)
{
    /// <summary>
    /// Parses player.txt data from TribalWars.
    /// </summary>
    /// <param name="csvData">Raw CSV content from player.txt</param>
    /// <returns>Dictionary mapping player ID to PlayerInfo</returns>
    /// <exception cref="InvalidOperationException">Thrown when CSV format is invalid</exception>
    public Dictionary<int, PlayerInfo> Parse(string csvData)
    {
        if (string.IsNullOrWhiteSpace(csvData))
        {
            throw new InvalidOperationException("CSV data cannot be empty.");
        }

        var lines = csvData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count == 0)
        {
            throw new InvalidOperationException("No data rows found in player CSV.");
        }

        var players = new Dictionary<int, PlayerInfo>(lines.Count);

        for (int i = 0; i < lines.Count; i++)
        {
            var columns = lines[i].Split(',');

            if (columns.Length < 6)
            {
                logger.LogWarning($"Warning: Player row {i + 1} has {columns.Length} columns, expected 6. Line: {lines[i]}");
                continue;
            }

            // Parse player ID (column 0)
            if (!int.TryParse(columns[0].Trim(), out var id))
            {
                logger.LogWarning($"Warning: Player row {i + 1} has invalid ID. Line: {lines[i]}");
                continue;
            }

            // Parse tribe ID (column 2)
            if (!int.TryParse(columns[2].Trim(), out var tribeId))
            {
                logger.LogWarning($"Warning: Player row {i + 1} has invalid TribeID. Line: {lines[i]}");
                continue;
            }

            // Parse villages count (column 3)
            if (!int.TryParse(columns[3].Trim(), out var villagesCount))
            {
                villagesCount = 0;
            }

            // Parse points (column 4)
            if (!int.TryParse(columns[4].Trim(), out var points))
            {
                points = 0;
            }

            // Parse rank (column 5)
            if (!int.TryParse(columns[5].Trim(), out var rank))
            {
                rank = 0;
            }

            // Name (column 1) - URL encoded
            var nick = System.Net.WebUtility.UrlDecode(columns[1].Trim());

            players[id] = new PlayerInfo
            {
                Id = id,
                Nick = nick,
                TribeId = tribeId,
                VillagesCount = villagesCount,
                Points = points,
                Rank = rank
            };
        }

        return players;
    }
}
