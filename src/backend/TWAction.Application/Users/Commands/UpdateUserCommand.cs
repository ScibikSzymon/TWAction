using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Commands;

public sealed record UpdateUserCommand(Guid UserId, UpdateUserRequest Request);

public sealed class UpdateUserHandler(IUserRepository userRepository)
{
    public async Task<Result<UserDto>> Handle(UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>($"User with ID '{command.UserId}' not found.");
        }

        var email = command.Request.Email.Trim();
        var existingUser = await userRepository.FindByEmailAsync(
            email,
            user.Provider,
            cancellationToken);

        if (existingUser is not null && existingUser.Id != user.Id)
        {
            return Result.Failure<UserDto>("Another user already uses this email address.");
        }

        user.Email = email;
        user.DisplayName = string.IsNullOrWhiteSpace(command.Request.DisplayName)
            ? null
            : command.Request.DisplayName.Trim();
        user.Role = command.Request.Role;

        var updatedUser = await userRepository.UpdateAsync(user, cancellationToken);
        return Result.Success(IUserMapper.ToDto(updatedUser));
    }
}
