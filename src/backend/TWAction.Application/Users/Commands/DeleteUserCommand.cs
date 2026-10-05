using TWAction.Application.Common;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Commands;

public sealed record DeleteUserCommand(Guid UserId);

public class DeleteUserHandler(IUserRepository userRepository)
{
    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure($"User with ID '{command.UserId}' not found.");
        }

        await userRepository.DeleteByIdAsync(command.UserId, cancellationToken);

        return Result.Success();
    }
}
