using TWAction.Application.Common;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Commands;

public sealed record DeleteSessionCommand(Guid SessionId);

public class DeleteSessionHandler(IUserSessionRepository sessionRepository)
{
    public async Task<Result> Handle(DeleteSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = await sessionRepository.GetByIdAsync(command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure($"Session with ID '{command.SessionId}' not found.");
        }

        await sessionRepository.DeleteByIdAsync(command.SessionId, cancellationToken);

        return Result.Success();
    }
}
