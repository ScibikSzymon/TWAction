using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Interfaces;
using TWAction.Domain.Users;

namespace TWAction.Application.Handlers;

public sealed record SignInWithGoogleCommand(string Email, string? DisplayName, string Provider = "google");

public sealed class SignInWithGoogleCommandValidator : AbstractValidator<SignInWithGoogleCommand>
{
    public SignInWithGoogleCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email must not be empty.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("Provider must not be empty.");
    }
}

public class SignInWithGoogleHandler(
    IUserRepository userRepository,
    IUserSessionRepository sessionRepository,
    IValidator<SignInWithGoogleCommand> fluentValidator)
{
    public async Task<Result<SignInResultDto>> Handle(SignInWithGoogleCommand command, CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync<SignInWithGoogleCommand, SignInResultDto>(
            fluentValidator, command, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        var user = await userRepository.FindByEmailAsync(command.Email, command.Provider, cancellationToken);
        if (user is null)
        {
            user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                DisplayName = command.DisplayName,
                Provider = command.Provider,
                Role = UserRole.User,
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
