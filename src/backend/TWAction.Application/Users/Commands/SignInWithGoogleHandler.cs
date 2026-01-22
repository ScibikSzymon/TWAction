using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Interfaces;
using TWAction.Domain.Users;

namespace TWAction.Application.Handlers;

public sealed record SignInWithGoogleCommand(string Email, string? DisplayName, string Provider = "google");

/// <summary>
/// Handles sign-in operations using Google authentication.
/// Creates new users if they don't exist and manages session creation.
/// </summary>
public class SignInWithGoogleHandler(IUserRepository userRepository, IUserSessionRepository sessionRepository)
{
    /// <summary>
    /// Processes a Google sign-in command and returns a result containing session information.
    /// </summary>
    /// <param name="command">The sign-in command containing user email and display name.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A Result containing SignInResult on success, or error information on failure.</returns>
    public async Task<Result<SignInResultDto>> Handle(SignInWithGoogleCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByEmailAsync(command.Email, command.Provider, cancellationToken);
        if (user is null)
        {
            user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                DisplayName = command.DisplayName,
                Provider = command.Provider,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user = await userRepository.AddAsync(user, cancellationToken);
        }

        var session = new UserSessionEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(8)
        };

        session = await sessionRepository.CreateSessionAsync(session, cancellationToken);

        var result = new SignInResultDto
        {
            SessionId = session.Id,
            User = IUserMapper.ToDto(user)
        };

        return Result.Success(result);
    }
}
