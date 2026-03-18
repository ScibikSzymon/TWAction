namespace ActionGenerator.Domain.Settings;

public sealed class ActionSettings
{
    public required DateTimeOffset MinDepartureTime { get; init; }
    public required bool SkipNightSendings { get; init; }
    public uint MaxNobleDistance { get; init; } = 49;
    public required OffSettings OffSettings { get; init; }
    public required CatasSettings CatasSettings { get; init; }
    public required FakeOffSettings FakeOffSettings { get; init; }
    public required FakeDeffSettings FakeDeffSettings { get; init; }
}

public class OffSettings
{
    public uint MinOffUnits { get; set; }
    public uint MinDistanceFromFront { get; set; } = 4;
}

public class CatasSettings
{
    public uint MinCatasNumber { get; set; } = 50;
    public uint MinDistanceFromFront { get; set; } = 4;
    public uint MaxOffUnits { get; set; } = 1_000_000;
}

public class FakeOffSettings
{
    public uint MinOffUnits { get; set; } = 10_000;
    public uint MinDistanceFromFront { get; set; } = 5;
    public uint MinMachineUnits { get; set; } = 8;
}

public class FakeDeffSettings
{
    public uint MaxOffUnits { get; set; } = 10_000;
    public uint MinDistanceFromFront { get; set; } = 3;
    public uint MinTotalUnits { get; set; } = 600;
    public uint MinMachineUnits { get; set; } = 7;
}