using FluentValidation;
using TWAction.Application.Schedules.Commands;

namespace TWAction.Application.Schedules.Validators;

public sealed class UpdateScheduleCommandValidator : AbstractValidator<UpdateScheduleCommand>
{
    public UpdateScheduleCommandValidator()
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID cannot be empty.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Schedule name is required.")
            .MaximumLength(100)
            .WithMessage("Schedule name cannot exceed 100 characters.");

        RuleFor(x => x.World)
            .IsInEnum()
            .WithMessage("Invalid world type.");

        RuleFor(x => x.ScheduleType)
            .IsInEnum()
            .WithMessage("Invalid schedule type.");
    }
}
