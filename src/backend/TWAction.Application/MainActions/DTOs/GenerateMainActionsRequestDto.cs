using TWAction.Application.ReconnaissanceActions.DTOs;
using TWAction.Domain.Settings;

namespace TWAction.Application.MainActions.DTOs;

/// <summary>
/// Request DTO matching Generator.Api's GenerateMainActionRequest payload.
/// The property names and types must stay in sync with ActionGenerator.Application.Features.MainActions.Dtos.GenerateMainActionRequest.
/// </summary>
public sealed record GenerateMainActionsRequest
{
    public required DateTimeOffset MinDepartureTime { get; init; }
    public required bool SkipNightSendings { get; init; }
    public required IReadOnlyList<VillageDto> AllyVillages { get; init; }
    public required IReadOnlyList<TargetDto> Targets { get; init; }
    public IReadOnlyDictionary<int, uint> PlayerNobleBudgets { get; init; } = new Dictionary<int, uint>();
    public uint MaxNobleDistance { get; init; } = 49;
    public required OffSettingsDto OffSettings { get; init; }
    public required CatasSettingsDto CatasSettings { get; init; }
    public required FakeOffSettingsDto FakeOffSettings { get; init; }
    public required FakeDeffSettingsDto FakeDeffSettings { get; init; }
    public required NobleSettingsDto NobleSettings { get; init; }
}

/// <summary>A single attack target: one village + one wave of the assigned template.</summary>
public sealed record TargetDto
{
    public required DateTimeOffset MinArrivalTime { get; init; }
    public required DateTimeOffset MaxArrivalTime { get; init; }
    public required string CommandType { get; init; }
    public required uint CommandNumber { get; init; }
    public required VillageSmallDto Village { get; init; }
}

/// <summary>Mirror of ActionGenerator.Domain.Settings.OffSettings.</summary>
public sealed record OffSettingsDto
{
    public required uint MinOffUnits { get; init; }
    public required uint MinDistanceFromFront { get; init; }
}

/// <summary>Mirror of ActionGenerator.Domain.Settings.CatasSettings.</summary>
public sealed record CatasSettingsDto
{
    public required uint MinCatasNumber { get; init; }
    public required uint MinDistanceFromFront { get; init; }
    public required uint MaxOffUnits { get; init; }
}

/// <summary>Mirror of ActionGenerator.Domain.Settings.FakeOffSettings.</summary>
public sealed record FakeOffSettingsDto
{
    public required uint MinOffUnits { get; init; }
    public required uint MinDistanceFromFront { get; init; }
}

/// <summary>Mirror of ActionGenerator.Domain.Settings.FakeDeffSettings.</summary>
public sealed record FakeDeffSettingsDto
{
    public required uint MaxOffUnits { get; init; }
    public required uint MinDistanceFromFront { get; init; }
}

/// <summary>Mirror of ActionGenerator.Domain.Settings.NobleSettings.</summary>
public sealed record NobleSettingsDto
{
    public required uint MinDistanceFromFront { get; init; }
    public required uint MinOffUnitsForOffNoble { get; init; }
    public required uint MinOffUnitsForFakeOffNoble { get; init; }
    public required uint MaxOffUnitsForDefNoble { get; init; }
    public required uint MinDeffUnitsForDefNoble { get; init; }
}
