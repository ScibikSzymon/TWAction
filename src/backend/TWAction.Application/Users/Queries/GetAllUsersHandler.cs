using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Interfaces;
using TWAction.Application.Mappers;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Queries;

public sealed class GetAllUsersQuery { }

public sealed class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
{
    public GetAllUsersQueryValidator()
    {
        
    }
}

public class GetAllUsersHandler(
    IUserRepository userRepository,
    IValidator<GetAllUsersQuery> fluentValidator)
{
    public async Task<Result<IEnumerable<UserDto>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync<GetAllUsersQuery, IEnumerable<UserDto>>(
            fluentValidator, query, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        var users = await userRepository.ListAllAsync(cancellationToken);
        var userDtos = users.Select(IUserMapper.ToDto).ToList();
        return Result.Success<IEnumerable<UserDto>>(userDtos);
    }
}
