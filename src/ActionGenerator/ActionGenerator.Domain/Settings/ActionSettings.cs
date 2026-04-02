namespace ActionGenerator.Domain.Settings;

public sealed class ActionSettings
{
    public required DateTimeOffset MinDepartureTime { get; init; }
    public required bool SkipNightSendings { get; init; }
    public uint MaxNobleDistance { get; init; } = 49; //min 10, max 99
    public required OffSettings OffSettings { get; init; }
    public required CatasSettings CatasSettings { get; init; }
    public required FakeOffSettings FakeOffSettings { get; init; }
    public required FakeDeffSettings FakeDeffSettings { get; init; }
    public NobleSettings NobleSettings { get; init; } = new();

    /// <summary>
    /// Noble budget per allied player (PlayerId → max nobles to dispatch).
    /// Players absent from this dictionary are treated as having no budget limit.
    /// </summary>
    public IReadOnlyDictionary<int, uint> PlayerNobleBudgets { get; init; } = new Dictionary<int, uint>();
}

public class OffSettings
{
    public uint MinOffUnits { get; set; } = 18000; //min 1k, max 21k
    public uint MinDistanceFromFront { get; set; } = 5; //min 0, max 100
}

public class CatasSettings
{
    public uint MinCatasNumber { get; set; } = 50; //min 10, max 2500
    public uint MinDistanceFromFront { get; set; } = 5; //min 0, max 100
    public uint MaxOffUnits { get; set; } = 25_000; //min 0, max 25k
}

public class FakeOffSettings
{
    public uint MinOffUnits { get; set; } = 10_000; //min 1k, max 21k
    public uint MinDistanceFromFront { get; set; } = 5; //min 0, max 100
    public const uint MinMachineUnits = 8; //const, not show for user.
}

public class FakeDeffSettings
{
    public uint MaxOffUnits { get; set; } = 10_000; //min 0k, max 21k
    public uint MinDistanceFromFront { get; set; } = 5; //min 0, max 100
    public const uint MinTotalUnits  = 600; //const, not show for user.
    public const uint MinMachineUnits = 7; //const, not show for user.
}

public class NobleSettings
{
    public uint MinDistanceFromFront { get; set; } = 5; //min 0, max 30

    /// <summary>Minimum offensive potential for Full/Half/Quarter off noble sources.</summary>
    public uint MinOffUnitsForOffNoble { get; set; } = 10_000; //min 1k, max 21k

    /// <summary>Minimum offensive potential for NobleWith150Axes (fake-off noble) sources.</summary>
    public uint MinOffUnitsForFakeOffNoble { get; set; } = 7_000; //min 1k, max 21k

    /// <summary>Maximum offensive potential for NobleWith100HeavyCavalry (def-based noble) sources.</summary>
    public uint MaxOffUnitsForDefNoble { get; set; } = 10_000; //min 1k, max 21k

    /// <summary>Maximum offensive potential for NobleWith100HeavyCavalry (def-based noble) sources.</summary>
    public uint MinDeffUnitsForDefNoble { get; set; } = 10_000; //min 1k, max 21k
}