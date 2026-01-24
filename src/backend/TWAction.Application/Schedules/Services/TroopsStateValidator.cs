using TWAction.Application.Common;

namespace TWAction.Application.Schedules.Services;

public sealed class TroopsStateValidator
{
    private const int ExpectedColumnCount = 11;
    
    private static readonly string[] ValidHeaders = new[]
    {
        "PlayerName,Village,Spear,Sword,Archer,Marcher,Catapult,Axe,Polearm,Ram,Trebuchet",
        "Nazwa gracza,Wioska,Piki,Miecze,Zwiad,CK,Katasy,Topory,LK,Tarany,Grube"
    };

    public Result<List<string[]>> ValidateAndParse(string rawData)
    {
        if (string.IsNullOrWhiteSpace(rawData))
        {
            return Result.Failure<List<string[]>>("Troops data cannot be empty.");
        }

        var lines = rawData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count < 2)
        {
            return Result.Failure<List<string[]>>("Troops data must contain header and at least one data row.");
        }

        // Validate header
        if (!ValidHeaders.Contains(lines[0]))
        {
            return Result.Failure<List<string[]>>(
                $"Invalid header. Expected one of: {string.Join(" or ", ValidHeaders)}");
        }

        var dataRows = new List<string[]>();

        // Validate data rows
        for (int i = 1; i < lines.Count; i++)
        {
            var columns = lines[i].Split(',');

            if (columns.Length != ExpectedColumnCount)
            {
                return Result.Failure<List<string[]>>(
                    $"Row {i + 1} has {columns.Length} columns, expected {ExpectedColumnCount}.");
            }

            // Validate player name (column 0)
            if (string.IsNullOrWhiteSpace(columns[0]))
            {
                return Result.Failure<List<string[]>>($"Row {i + 1}: Player name cannot be empty.");
            }

            // Validate village coordinates (column 1)
            var villageValidation = ValidateVillageCoordinates(columns[1], i + 1);
            if (villageValidation.IsFailure)
            {
                return villageValidation;
            }

            // Validate unit counts (columns 2-10)
            for (int j = 2; j < ExpectedColumnCount; j++)
            {
                if (!int.TryParse(columns[j], out var unitCount) || unitCount < 0)
                {
                    var unitNames = new[] { "Spear", "Sword", "Archer", "Marcher", "Catapult", "Axe", "Polearm", "Ram", "Trebuchet" };
                    return Result.Failure<List<string[]>>(
                        $"Row {i + 1}: Invalid unit count for '{unitNames[j - 2]}'. Must be non-negative integer.");
                }
            }

            dataRows.Add(columns);
        }

        return Result.Success(dataRows);
    }

    private Result<List<string[]>> ValidateVillageCoordinates(string coordinates, int rowNumber)
    {
        if (string.IsNullOrWhiteSpace(coordinates))
        {
            return Result.Failure<List<string[]>>($"Row {rowNumber}: Village coordinates cannot be empty.");
        }

        var parts = coordinates.Split('|');
        if (parts.Length != 2)
        {
            return Result.Failure<List<string[]>>(
                $"Row {rowNumber}: Village coordinates must be in format 'X|Y', got '{coordinates}'.");
        }

        if (!int.TryParse(parts[0], out _) || !int.TryParse(parts[1], out _))
        {
            return Result.Failure<List<string[]>>(
                $"Row {rowNumber}: Village coordinates must contain valid integers in format 'X|Y'.");
        }

        return Result.Success(new List<string[]>());
    }
}
