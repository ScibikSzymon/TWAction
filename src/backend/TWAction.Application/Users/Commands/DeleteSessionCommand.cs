using TWAction.Application.Common;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Commands;

public sealed record DeleteSessionCommand(Guid SessionId);

public class DeleteSessionHandler(IUserSessionRepository sessionRepository)
{
    /// <summary>
    /// Handles the deletion of a user session.
    /// </summary>
    /// <returns>A result indicating success or failure with an error message.</returns>
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
