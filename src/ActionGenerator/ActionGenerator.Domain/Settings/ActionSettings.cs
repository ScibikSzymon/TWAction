namespace ActionGenerator.Domain.Settings;

public sealed class ActionSettings
{
    public required DateTimeOffset MinDepartureTime { get; init; }
    public required bool SkipNightSendings { get; init; }
    public uint MaxNobleDistance { get; init; } = 49;
    public required OFfSettings OFfSettings { get; init; }
    public required CatasSettings CatasSettings { get; init; }
    public required FakeOffSettings FakeOffSettings { get; init; }
    public required FakeDeffSettings FakeDeffSettings { get; init; } 
}


public class OFfSettings
{
    public uint MinOffUnits { get; set; }
    public uint MinDistanceFromFront { get; set; } = 4;
}

public class CatasSettings
{
    public uint MinCatasNumber { get; set; } = 50;
    public uint MinDistanceFromFront { get; set; } = 4;
    public uint MaxOffUnits { get; set; } = 1400000; //katasy sa rozpisywane z fulli
}

public class FakeOffSettings
{
    public uint MinOffUnits { get; set; } = 10000;
    public uint MinDistanceFromFront { get; set; } = 5;
}

public class FakeDeffSettings
{
    public uint MaxOffUnits { get; set; } = 10000;
    public uint MinDistanceFromFront { get; set; } = 3;
}