using TWAction.Domain.Schedules;

namespace TWAction.Domain.Users;

public sealed class UserEntity
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string Provider { get; set; } = "google";

    public UserRole Role { get; set; } = UserRole.User;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<UserSessionEntity> Sessions { get; set; } = [];
    
    public ICollection<ScheduleEntity> Schedules { get; set; } = [];
}
