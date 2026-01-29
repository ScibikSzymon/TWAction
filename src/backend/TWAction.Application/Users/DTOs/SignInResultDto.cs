namespace TWAction.Application.Users.DTOs;

public sealed record SignInResultDto
{
    public required Guid SessionId { get; init; }

    public required UserDto User { get; init; }
}
