using TWAction.Application.Common;
using TWAction.Domain.Tribes;

namespace TWAction.Infrastructure.Services;

public sealed class TribesCsvParser
{
    /// <summary>
    /// Parses TribalWars ally.txt CSV format
    /// Expected format: TribalWarsID,Name,Short,PlayersCount,VillagesCount,Top40Points,TotalPoints,Ranking
    /// </summary>
    public Result<List<TribeInfo>> Parse(string csvData)
    {
        if (string.IsNullOrWhiteSpace(csvData))
        {
            return Result.Failure<List<TribeInfo>>("CSV data cannot be empty.");
        }

        var lines = csvData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count == 0)
        {
            return Result.Failure<List<TribeInfo>>("No data rows found in CSV.");
        }

        var tribes = new List<TribeInfo>();

        foreach (var (line, index) in lines.Select((line, i) => (line, i)))
        {
            var columns = line.Split(',');

            if (columns.Length < 5)
            {
                return Result.Failure<List<TribeInfo>>(
                    $"Row {index + 1}: Expected at least 5 columns, got {columns.Length}.");
            }

            // Parse TribalWarsID
            if (!int.TryParse(columns[0].Trim(), out var tribalWarsId))
            {
                return Result.Failure<List<TribeInfo>>(
                    $"Row {index + 1}: Invalid TribalWarsID. Must be integer.");
            }

            var name = System.Net.WebUtility.UrlDecode(columns[1].Trim());
            var shortName = System.Net.WebUtility.UrlDecode(columns[2].Trim());

            // Parse VillagesCount
            if (!int.TryParse(columns[4].Trim(), out var villagesCount))
            {
                return Result.Failure<List<TribeInfo>>(
                    $"Row {index + 1}: Invalid VillagesCount. Must be integer.");
            }

            tribes.Add(new TribeInfo
            {
                TribalWarsId = tribalWarsId,
                Name = name,
                Short = shortName,
                VillagesCount = villagesCount
            });
        }

        return Result.Success(tribes);
    }
}
