using TWAction.Application.Common;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Commands;

public sealed record DeleteUserSessionsCommand(Guid UserId);

public sealed class DeleteUserSessionsHandler(
    IUserRepository userRepository,
    IUserSessionRepository sessionRepository)
{
    public async Task<Result> Handle(DeleteUserSessionsCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure($"User with ID '{command.UserId}' not found.");
        }

        await sessionRepository.DeleteByUserIdAsync(command.UserId, cancellationToken);
        return Result.Success();
    }
}
