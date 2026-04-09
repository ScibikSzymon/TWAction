namespace TWAction.Application.Settings.DTOs;

public sealed record MainActionSettingsDto
{
    public required Guid Id { get; init; }
    public required Guid ScheduleId { get; init; }
    public required DateTimeOffset MinDepartureTime { get; init; }
    public required bool SkipNightSendings { get; init; }
    public required uint MaxNobleDistance { get; init; }
    public required DateOnly ActionDate { get; init; }
    public required MainActionOffSettingsDto OffSettings { get; init; }
    public required MainActionCatasSettingsDto CatasSettings { get; init; }
    public required MainActionFakeOffSettingsDto FakeOffSettings { get; init; }
    public required MainActionFakeDeffSettingsDto FakeDeffSettings { get; init; }
    public required MainActionNobleSettingsDto NobleSettings { get; init; }
    public required IReadOnlyDictionary<int, uint> PlayerNobleBudgets { get; init; }
}

public sealed record MainActionOffSettingsDto
{
    public required uint MinOffUnits { get; init; }
    public required uint MinDistanceFromFront { get; init; }
}

public sealed record MainActionCatasSettingsDto
{
    public required uint MinCatasNumber { get; init; }
    public required uint MinDistanceFromFront { get; init; }
    public required uint MaxOffUnits { get; init; }
}

public sealed record MainActionFakeOffSettingsDto
{
    public required uint MinOffUnits { get; init; }
    public required uint MinDistanceFromFront { get; init; }
}

public sealed record MainActionFakeDeffSettingsDto
{
    public required uint MaxOffUnits { get; init; }
    public required uint MinDistanceFromFront { get; init; }
}

public sealed record MainActionNobleSettingsDto
{
    public required uint MinDistanceFromFront { get; init; }
    public required uint MinOffUnitsForOffNoble { get; init; }
    public required uint MinOffUnitsForFakeOffNoble { get; init; }
    public required uint MaxOffUnitsForDefNoble { get; init; }
    public required uint MinDeffUnitsForDefNoble { get; init; }
}

public sealed record SaveMainActionSettingsRequest
{
    public required DateTimeOffset MinDepartureTime { get; init; }
    public required bool SkipNightSendings { get; init; }
    public required uint MaxNobleDistance { get; init; }
    public required DateOnly ActionDate { get; init; }
    public required MainActionOffSettingsDto OffSettings { get; init; }
    public required MainActionCatasSettingsDto CatasSettings { get; init; }
    public required MainActionFakeOffSettingsDto FakeOffSettings { get; init; }
    public required MainActionFakeDeffSettingsDto FakeDeffSettings { get; init; }
    public required MainActionNobleSettingsDto NobleSettings { get; init; }
    public IReadOnlyDictionary<int, uint> PlayerNobleBudgets { get; init; } = new Dictionary<int, uint>();
}
