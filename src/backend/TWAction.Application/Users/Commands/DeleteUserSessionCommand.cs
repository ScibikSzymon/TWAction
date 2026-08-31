using TWAction.Application.Common;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Commands;

public sealed record DeleteUserSessionCommand(Guid UserId, Guid SessionId);

public sealed class DeleteUserSessionHandler(IUserSessionRepository sessionRepository)
{
    public async Task<Result> Handle(DeleteUserSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = await sessionRepository.GetByIdAsync(command.SessionId, cancellationToken);

        if (session is null || session.UserId != command.UserId)
        {
            return Result.Failure($"Session with ID '{command.SessionId}' not found for this user.");
        }

        await sessionRepository.DeleteByIdAsync(command.SessionId, cancellationToken);
        return Result.Success();
    }
}
