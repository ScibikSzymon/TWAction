using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Interfaces;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Settings.DTOs;
using TWAction.Application.Settings.Interfaces;

namespace TWAction.Application.Settings.Queries;

public sealed record GetReconnaissanceSettingsQuery(Guid ScheduleId);

public sealed class GetReconnaissanceSettingsQueryValidator : AbstractValidator<GetReconnaissanceSettingsQuery>
{
    public GetReconnaissanceSettingsQueryValidator(
        ICurrentUserAccessor currentUser,
        IScheduleRepository scheduleRepository,
        IReconnaissanceSettingsRepository settingsRepository)
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

        RuleFor(x => x.ScheduleId)
            .MustAsync(async (scheduleId, cancellationToken) =>
            {
                var settings = await settingsRepository.GetByScheduleIdAsync(scheduleId, cancellationToken);
                return settings is not null;
            })
            .WithMessage(query => $"Reconnaissance settings for schedule '{query.ScheduleId}' not found.")
            .When(query => currentUser.TryGetUserId(out _));
    }
}

public class GetReconnaissanceSettingsHandler(
    IReconnaissanceSettingsRepository repository,
    IValidator<GetReconnaissanceSettingsQuery> fluentValidator)
{
    public async Task<Result<ReconnaissanceSettingsDto>> Handle(
        GetReconnaissanceSettingsQuery query, 
        CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync<GetReconnaissanceSettingsQuery, ReconnaissanceSettingsDto>(
            fluentValidator, query, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        var settings = (await repository.GetByScheduleIdAsync(query.ScheduleId, cancellationToken))!;

        return Result.Success(IReconnaissanceSettingsMapper.ToDto(settings));
    }
}
