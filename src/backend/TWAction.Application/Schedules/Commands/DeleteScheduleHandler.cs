using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Interfaces;
using TWAction.Application.Schedules.Interfaces;

namespace TWAction.Application.Schedules.Commands;

public sealed record DeleteScheduleCommand(Guid ScheduleId);

public sealed class DeleteScheduleCommandValidator : AbstractValidator<DeleteScheduleCommand>
{
    public DeleteScheduleCommandValidator(
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
            .WithMessage(command => $"Schedule with ID '{command.ScheduleId}' not found.")
            .When(command => currentUser.TryGetUserId(out _));

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
            .When(command => currentUser.TryGetUserId(out _));
    }
}

public class DeleteScheduleHandler(
    IScheduleRepository scheduleRepository,
    IValidator<DeleteScheduleCommand> fluentValidator)
{
    public async Task<Result> Handle(DeleteScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync(
            fluentValidator, command, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        await scheduleRepository.DeleteByIdAsync(command.ScheduleId, cancellationToken);

        return Result.Success();
    }
}
