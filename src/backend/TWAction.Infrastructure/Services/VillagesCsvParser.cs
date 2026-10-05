using TWAction.Domain.Tribes;

namespace TWAction.Infrastructure.Services;

/// <summary>
/// Parses TribalWars village.txt CSV format (NO HEADER).
/// Format: ID,Name,X,Y,PlayerID,Points,Rank
/// </summary>
public sealed class VillagesCsvParser
{
    /// <summary>
    /// Parses village.txt data from TribalWars and links with player data.
    /// </summary>
    /// <param name="csvData">Raw CSV content from village.txt</param>
    /// <param name="players">Dictionary of players for linking (optional)</param>
    /// <returns>Tuple with ID-based dictionary and coordinate-based dictionary</returns>
    /// <exception cref="InvalidOperationException">Thrown when CSV format is invalid</exception>
    public (Dictionary<int, VillageInfo> ById, Dictionary<int, VillageInfo> ByCoordinates) Parse(
        string csvData, 
        Dictionary<int, PlayerInfo>? players = null)
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
            throw new InvalidOperationException("No data rows found in village CSV.");
        }

        var villagesById = new Dictionary<int, VillageInfo>(lines.Count);
        var villagesByCoords = new Dictionary<int, VillageInfo>(lines.Count);

        for (int i = 0; i < lines.Count; i++)
        {
            var columns = lines[i].Split(',');

            if (columns.Length < 7)
            {
                Console.WriteLine($"Warning: Village row {i + 1} has {columns.Length} columns, expected 7. Line: {lines[i]}");
                continue;
            }

            // Parse village ID (column 0)
            if (!int.TryParse(columns[0].Trim(), out var id))
            {
                Console.WriteLine($"Warning: Village row {i + 1} has invalid ID. Line: {lines[i]}");
                continue;
            }

            // Parse X coordinate (column 2)
            if (!int.TryParse(columns[2].Trim(), out var x))
            {
                Console.WriteLine($"Warning: Village row {i + 1} has invalid X coordinate. Line: {lines[i]}");
                continue;
            }

            // Parse Y coordinate (column 3)
            if (!int.TryParse(columns[3].Trim(), out var y))
            {
                Console.WriteLine($"Warning: Village row {i + 1} has invalid Y coordinate. Line: {lines[i]}");
                continue;
            }

            // Parse player ID (column 4)
            if (!int.TryParse(columns[4].Trim(), out var playerId))
            {
                Console.WriteLine($"Warning: Village row {i + 1} has invalid PlayerID. Line: {lines[i]}");
                continue;
            }

            // Parse points (column 5)
            if (!int.TryParse(columns[5].Trim(), out var points))
            {
                points = 0;
            }

            // Name (column 1) - URL encoded
            var name = System.Net.WebUtility.UrlDecode(columns[1].Trim());

            // Try to link with player data
            PlayerInfo? player = null;
            if (players != null)
            {
                players.TryGetValue(playerId, out player);
            }

            var village = new VillageInfo
            {
                Id = id,
                Name = name,
                X = x,
                Y = y,
                Points = points,
                PlayerId = playerId,
                Player = player
            };

            villagesById[id] = village;
            villagesByCoords[village.CoordinateKey] = village;
        }

        return (villagesById, villagesByCoords);
    }
}
