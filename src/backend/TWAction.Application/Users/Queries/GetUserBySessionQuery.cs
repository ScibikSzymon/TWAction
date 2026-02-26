using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Queries;

public sealed record GetUserBySessionQuery(Guid SessionId);

public sealed class GetUserBySessionQueryValidator : AbstractValidator<GetUserBySessionQuery>
{
    public GetUserBySessionQueryValidator(
        IUserSessionRepository sessionRepository,
        IUserRepository userRepository)
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("Session ID must not be empty.");

        RuleFor(x => x.SessionId)
            .MustAsync(async (sessionId, cancellationToken) =>
            {
                var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken);
                return session is not null;
            })
            .WithMessage(query => $"Session with ID '{query.SessionId}' not found.");

        RuleFor(x => x.SessionId)
            .MustAsync(async (sessionId, cancellationToken) =>
            {
                var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken);
                if (session is null) return true;
                return session.ExpiresAt >= DateTimeOffset.UtcNow;
            })
            .WithMessage("Session has expired.");

        RuleFor(x => x.SessionId)
            .MustAsync(async (sessionId, cancellationToken) =>
            {
                var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken);
                if (session is null) return true;
                var user = await userRepository.GetByIdAsync(session.UserId, cancellationToken);
                return user is not null;
            })
            .WithMessage("User associated with the session was not found.");
    }
}

public class GetUserBySessionHandler(
    IUserSessionRepository sessionRepository,
    IUserRepository userRepository,
    IValidator<GetUserBySessionQuery> fluentValidator)
{
    public async Task<Result<UserDto>> Handle(GetUserBySessionQuery query, CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync<GetUserBySessionQuery, UserDto>(
            fluentValidator, query, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        var session = (await sessionRepository.GetByIdAsync(query.SessionId, cancellationToken))!;
        var user = (await userRepository.GetByIdAsync(session.UserId, cancellationToken))!;

        return Result.Success(IUserMapper.ToDto(user));
    }
}
