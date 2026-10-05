using TWAction.Application.Common;

namespace TWAction.Application.MainActions.Services;

public sealed class ParsedTroopsData
{
    public required string[] Header { get; init; }
    public required List<string[]> DataRows { get; init; }
}

public sealed class TroopsStateValidator
{
    private const int ExpectedColumnCount = 11;
    
    private static readonly string[] ValidHeaders = new[]
    {
        "PlayerName,Village,Spear,Sword,Archer,Marcher,Catapult,Axe,Polearm,Ram,Trebuchet",
        "Nazwa gracza,Wioska,Piki,Miecze,Zwiad,CK,Katasy,Topory,LK,Tarany,Grube"
    };

    public Result<ParsedTroopsData> ValidateAndParse(string rawData)
    {
        if (string.IsNullOrWhiteSpace(rawData))
        {
            return Result.Failure<ParsedTroopsData>("Troops data cannot be empty.");
        }

        var lines = rawData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count < 2)
        {
            return Result.Failure<ParsedTroopsData>("Troops data must contain header and at least one data row.");
        }

        // Validate header
        if (!ValidHeaders.Contains(lines[0]))
        {
            return Result.Failure<ParsedTroopsData>(
                $"Invalid header. Expected one of: {string.Join(" or ", ValidHeaders)}");
        }

        var header = lines[0].Split(',');
        var dataRows = new List<string[]>();

        // Validate data rows
        for (int i = 1; i < lines.Count; i++)
        {
            var columns = lines[i].Split(',');

            if (columns.Length != ExpectedColumnCount)
            {
                return Result.Failure<ParsedTroopsData>(
                    $"Row {i + 1} has {columns.Length} columns, expected {ExpectedColumnCount}.");
            }

            // Validate player name (column 0)
            if (string.IsNullOrWhiteSpace(columns[0]))
            {
                return Result.Failure<ParsedTroopsData>($"Row {i + 1}: Player name cannot be empty.");
            }

            // Validate village coordinates (column 1)
            var villageValidation = ValidateVillageCoordinates(columns[1], i + 1);
            if (villageValidation.IsFailure)
            {
                return Result.Failure<ParsedTroopsData>(villageValidation.Error);
            }

            // Validate unit counts (columns 2-10)
            bool canAdd = true; //IDK why but some villages have negative unit counts, TW has nug..
            for (int j = 2; j < ExpectedColumnCount; j++)
            {
                if (!int.TryParse(columns[j], out var unitCount) || unitCount < 0)
                {
                    canAdd = false; 
                    break;
                }
            }
            if(canAdd)
                dataRows.Add(columns);
        }

        return Result.Success(new ParsedTroopsData
        {
            Header = header,
            DataRows = dataRows
        });
    }

    private Result ValidateVillageCoordinates(string coordinates, int rowNumber)
    {
        if (string.IsNullOrWhiteSpace(coordinates))
        {
            return Result.Failure($"Row {rowNumber}: Village coordinates cannot be empty.");
        }

        var parts = coordinates.Split('|');
        if (parts.Length != 2)
        {
            return Result.Failure(
                $"Row {rowNumber}: Village coordinates must be in format 'X|Y', got '{coordinates}'.");
        }

        if (!int.TryParse(parts[0], out _) || !int.TryParse(parts[1], out _))
        {
            return Result.Failure(
                $"Row {rowNumber}: Village coordinates must contain valid integers in format 'X|Y'.");
        }

        return Result.Success();
    }
}
