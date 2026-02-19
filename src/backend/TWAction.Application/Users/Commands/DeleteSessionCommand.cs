using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Commands;

public sealed record DeleteSessionCommand(Guid SessionId);

public sealed class DeleteSessionCommandValidator : AbstractValidator<DeleteSessionCommand>
{
    public DeleteSessionCommandValidator(IUserSessionRepository sessionRepository)
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
            .WithMessage(command => $"Session with ID '{command.SessionId}' not found.");
    }
}

public class DeleteSessionHandler(
    IUserSessionRepository sessionRepository,
    IValidator<DeleteSessionCommand> fluentValidator)
{
    public async Task<Result> Handle(DeleteSessionCommand command, CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync(
            fluentValidator, command, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        await sessionRepository.DeleteByIdAsync(command.SessionId, cancellationToken);

        return Result.Success();
    }
}
