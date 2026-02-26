using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Interfaces;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;

namespace TWAction.Application.Schedules.Queries;

public sealed record GetScheduleByIdQuery(Guid ScheduleId);

public sealed class GetScheduleByIdQueryValidator : AbstractValidator<GetScheduleByIdQuery>
{
    public GetScheduleByIdQueryValidator(
        ICurrentUserAccessor currentUser,
        IScheduleRepository scheduleRepository)
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID must not be empty.");

        RuleFor(x => x.ScheduleId)
            .MustAsync(async (scheduleId, cancellationToken) =>
            {
                var schedule = await scheduleRepository.GetByIdAsync(scheduleId, cancellationToken);
                return schedule is not null;
            })
            .WithMessage(query => $"Schedule with ID '{query.ScheduleId}' not found.")
            .When(query => currentUser.TryGetUserId(out _));

        RuleFor(x => x.ScheduleId)
            .MustAsync(async (scheduleId, cancellationToken) =>
            {
                var schedule = await scheduleRepository.GetByIdAsync(scheduleId, cancellationToken);
                if (schedule is null) return true;
                if (currentUser.IsAdmin) return true;
                currentUser.TryGetUserId(out var userId);
                return schedule.UserGuid == userId;
            })
            .WithMessage("Schedule not found for specified user.")
            .When(query => currentUser.TryGetUserId(out _));
    }
}

public class GetScheduleByIdHandler(
    IScheduleRepository scheduleRepository,
    IValidator<GetScheduleByIdQuery> fluentValidator)
{
    /// <summary>
    /// Handles fetching a schedule by ID — runs FluentValidation first, then returns data.
    /// </summary>
    public async Task<Result<ScheduleDto>> Handle(GetScheduleByIdQuery query, CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync<GetScheduleByIdQuery, ScheduleDto>(
            fluentValidator, query, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        var schedule = (await scheduleRepository.GetByIdAsync(query.ScheduleId, cancellationToken))!;

        return Result.Success(IScheduleMapper.ToDto(schedule));
    }
}
