namespace TWAction.Application.Users.DTOs;

public sealed record UserDto
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public string? DisplayName { get; init; }

    public required string Provider { get; init; }

    public required string Role { get; init; }
    public required string SubscriptionTier { get; init; }
    public int? ScheduleLimitOverride { get; init; }
    public int? TemplateLimitOverride { get; init; }
    public int? TroopsUploadLimitOverride { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
