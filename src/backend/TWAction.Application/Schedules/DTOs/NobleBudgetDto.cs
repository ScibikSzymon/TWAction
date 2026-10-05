namespace TWAction.Application.Schedules.DTOs;

public sealed record NobleBudgetDto
{
    public required Guid Id { get; init; }

    public required Guid ScheduleId { get; init; }

    public required int PlayerId { get; init; }

    public required int Budget { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}

public sealed record SaveNobleBudgetsRequest
{
    public required List<PlayerBudgetItem> PlayerBudgets { get; init; }
}

public sealed record PlayerBudgetItem
{
    public required int PlayerId { get; init; }

    public required int Budget { get; init; }
}
