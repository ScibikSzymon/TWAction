using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Interfaces;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;

namespace TWAction.Application.Schedules.Queries;

public sealed record GetAllSchedulesQuery(Guid UserId);

public sealed class GetAllSchedulesQueryValidator : AbstractValidator<GetAllSchedulesQuery>
{
    public GetAllSchedulesQueryValidator(ICurrentUserAccessor currentUser)
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID must not be empty.");
    }
}

public class GetAllSchedulesHandler(
    IScheduleRepository scheduleRepository,
    IValidator<GetAllSchedulesQuery> fluentValidator)
{
    public async Task<Result<IEnumerable<ScheduleDto>>> Handle(GetAllSchedulesQuery query, CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync<GetAllSchedulesQuery, IEnumerable<ScheduleDto>>(
            fluentValidator, query, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        var schedules = await scheduleRepository.ListByUserIdAsync(query.UserId, cancellationToken);
        var scheduleDtos = schedules.Select(IScheduleMapper.ToDto).ToList();
        return Result.Success<IEnumerable<ScheduleDto>>(scheduleDtos);
    }
}
