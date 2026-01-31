using FluentValidation;
using TWAction.Application.Schedules.Commands;

namespace TWAction.Application.Schedules.Validators;

public sealed class DeleteScheduleCommandValidator : AbstractValidator<DeleteScheduleCommand>
{
    public DeleteScheduleCommandValidator()
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID cannot be empty.");
    }
}
