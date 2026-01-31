using FluentValidation;
using TWAction.Application.Schedules.Commands;

namespace TWAction.Application.Schedules.Validators;

public sealed class CreateScheduleCommandValidator : AbstractValidator<CreateScheduleCommand>
{
    public CreateScheduleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID cannot be empty.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Schedule name is required.")
            .MaximumLength(100)
            .WithMessage("Schedule name cannot exceed 100 characters.");

        RuleFor(x => x.EnemyTribalWarsIds)
            .NotNull()
            .WithMessage("Enemy list cannot be null.");

        RuleFor(x => x.World)
            .IsInEnum()
            .WithMessage("Invalid world type.");

        RuleFor(x => x.ScheduleType)
            .IsInEnum()
            .WithMessage("Invalid schedule type.");
    }
}
