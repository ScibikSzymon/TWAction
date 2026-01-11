using TWAction.Application.Common;
using TWAction.Application.DTOs;
using TWAction.Application.Interfaces;
using TWAction.Application.Mappers;
using TWAction.Domain.Entities;

namespace TWAction.Application.Handlers;

public sealed record SignInWithGoogleCommand(string Email, string? DisplayName, string Provider = "google");

/// <summary>
/// Handles sign-in operations using Google authentication.
/// Creates new users if they don't exist and manages session creation.
/// </summary>
public class SignInWithGoogleHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _sessionRepository;

    public SignInWithGoogleHandler(IUserRepository userRepository, IUserSessionRepository sessionRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
    }

    /// <summary>
    /// Processes a Google sign-in command and returns a result containing session information.
    /// </summary>
    /// <param name="command">The sign-in command containing user email and display name.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A Result containing SignInResult on success, or error information on failure.</returns>
    public async Task<Result<SignInResult>> Handle(SignInWithGoogleCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByEmailAsync(command.Email, command.Provider, cancellationToken);
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
            user = await _userRepository.AddAsync(user, cancellationToken);
        }

        var session = new UserSessionEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(8)
        };

        session = await _sessionRepository.CreateSessionAsync(session, cancellationToken);

        var result = new SignInResult
        {
            SessionId = session.Id,
            User = IUserMapper.ToDto(user)
        };

        return Result.Success(result);
    }
}
