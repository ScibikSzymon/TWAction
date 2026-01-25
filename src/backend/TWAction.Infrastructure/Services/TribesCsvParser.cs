using TWAction.Domain.Tribes;

namespace TWAction.Infrastructure.Services;

public sealed class TribesCsvParser
{
    private const int ExpectedColumnCount = 8;

    /// <summary>
    /// Parses TribalWars ally.txt CSV format (NO HEADER)
    /// Expected format: TribalWarsID,Name,Short,PlayersCount,VillagesCount,Top40Points,TotalPoints,Ranking
    /// Throws exceptions on validation failure
    /// </summary>
    public List<TribeInfo> Parse(string csvData)
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
            throw new InvalidOperationException("No data rows found in CSV.");
        }

        var tribes = new List<TribeInfo>();

        // Parse all rows (no header in ally.txt)
        for (int i = 0; i < lines.Count; i++)
        {
            var columns = lines[i].Split(',');

            if (columns.Length < 5)
            {
                throw new InvalidOperationException(
                    $"Row {i + 1} has {columns.Length} columns, expected at least 5.");
            }

            // Parse TribalWarsID (column 0)
            if (!int.TryParse(columns[0].Trim(), out var tribalWarsId))
            {
                throw new InvalidOperationException(
                    $"Row {i + 1}: Invalid TribalWarsID. Must be integer.");
            }

            // Name (column 1) - URL encoded
            var name = System.Net.WebUtility.UrlDecode(columns[1].Trim());
            
            // Short (column 2) - URL encoded
            var shortName = System.Net.WebUtility.UrlDecode(columns[2].Trim());

            // VillagesCount (column 4)
            if (!int.TryParse(columns[4].Trim(), out var villagesCount))
            {
                throw new InvalidOperationException(
                    $"Row {i + 1}: Invalid VillagesCount. Must be integer.");
            }

            tribes.Add(new TribeInfo
            {
                TribalWarsId = tribalWarsId,
                Name = name,
                Short = shortName,
                VillagesCount = villagesCount
            });
        }

        return tribes;
    }
}


