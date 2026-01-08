namespace TWAction.Application.DTOs;

public sealed class SignInResult
{
    public Guid SessionId { get; set; }

    public UserDto User { get; set; } = null!;
}
