using TWAction.Application.DTOs;
using TWAction.Application.Interfaces;
using TWAction.Application.Mappers;
using TWAction.Domain.Entities;

namespace TWAction.Application.Handlers;

public sealed record SignInWithGoogleCommand(string Email, string? DisplayName, string Provider = "google");
public class SignInWithGoogleHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _sessionRepository;

    public SignInWithGoogleHandler(IUserRepository userRepository, IUserSessionRepository sessionRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<SignInResult> Handle(SignInWithGoogleCommand command, CancellationToken cancellationToken = default)
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

        return new SignInResult
        {
            SessionId = session.Id,
            User = IUserMapper.ToDto(user)
        };
    }
}
