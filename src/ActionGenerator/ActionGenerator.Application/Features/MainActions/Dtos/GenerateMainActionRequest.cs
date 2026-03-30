using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Domain.Settings;

namespace ActionGenerator.Application.Features.MainActions.Dtos;

public sealed record GenerateMainActionRequest
{
    public required DateTimeOffset MinDepartureTime { get; init; }
    public required bool SkipNightSendings { get; init; }
    public required IReadOnlyList<VillageDto> AllyVillages { get; init; }
    public required IReadOnlyList<TargetDto> Targets { get; init; }

    /// <summary>Noble budget per allied player (PlayerId ? max nobles to dispatch).</summary>
    public IReadOnlyDictionary<int, uint> PlayerNobleBudgets { get; init; } = new Dictionary<int, uint>();

    public uint MaxNobleDistance { get; init; } = 49;

    // Action algorithm settings — sent by the client or populated with backend defaults.
    public OffSettings OffSettings { get; init; } = new();
    public CatasSettings CatasSettings { get; init; } = new();
    public FakeOffSettings FakeOffSettings { get; init; } = new();
    public FakeDeffSettings FakeDeffSettings { get; init; } = new();
    public NobleSettings NobleSettings { get; init; } = new();
}
