namespace TWAction.Application.MainActions.DTOs;

public sealed record PlayerNobleStatsDto
{
    public required int PlayerId { get; init; }

    public required string PlayerName { get; init; }

    public required int TotalNobles { get; init; }
}
