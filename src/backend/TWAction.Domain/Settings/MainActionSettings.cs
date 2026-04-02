namespace TWAction.Domain.Settings;

/// <summary>
/// Represents main action settings for a schedule.
/// </summary>
public sealed class MainActionSettings
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public DateTimeOffset MinDepartureTime { get; set; }
    public bool SkipNightSendings { get; set; }
    public uint MaxNobleDistance { get; set; }
    public MainActionOffSettings OffSettings { get; set; } = null!;
    public MainActionCatasSettings CatasSettings { get; set; } = null!;
    public MainActionFakeOffSettings FakeOffSettings { get; set; } = null!;
    public MainActionFakeDeffSettings FakeDeffSettings { get; set; } = null!;
    public MainActionNobleSettings NobleSettings { get; set; } = null!;
    public Dictionary<int, uint> PlayerNobleBudgets { get; set; } = new();
}

/// <summary>
/// Settings for off attacks.
/// </summary>
public sealed class MainActionOffSettings
{
    public uint MinOffUnits { get; set; }
    public uint MinDistanceFromFront { get; set; }
}

/// <summary>
/// Settings for catapult attacks.
/// </summary>
public sealed class MainActionCatasSettings
{
    public uint MinCatasNumber { get; set; }
    public uint MinDistanceFromFront { get; set; }
    public uint MaxOffUnits { get; set; }
}

/// <summary>
/// Settings for fake off attacks.
/// </summary>
public sealed class MainActionFakeOffSettings
{
    public uint MinOffUnits { get; set; }
    public uint MinDistanceFromFront { get; set; }
}

/// <summary>
/// Settings for fake deff attacks.
/// </summary>
public sealed class MainActionFakeDeffSettings
{
    public uint MaxOffUnits { get; set; }
    public uint MinDistanceFromFront { get; set; }
}

/// <summary>
/// Settings for noble attacks.
/// </summary>
public sealed class MainActionNobleSettings
{
    public uint MinDistanceFromFront { get; set; }
    public uint MinOffUnitsForOffNoble { get; set; }
    public uint MinOffUnitsForFakeOffNoble { get; set; }
    public uint MaxOffUnitsForDefNoble { get; set; }
    public uint MinDeffUnitsForDefNoble { get; set; }
}
