namespace TWAction.Api.Validators;

using FluentValidation;
using TWAction.Api.Endpoints;

public sealed class UpdateScheduleRequestValidator : AbstractValidator<UpdateScheduleRequest>
{
    public UpdateScheduleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Schedule name is required.")
            .MaximumLength(100)
            .WithMessage("Schedule name must not exceed 100 characters.");

        RuleFor(x => x.World)
            .IsInEnum()
            .WithMessage("World must be a valid WorldType value.");

        RuleFor(x => x.ScheduleType)
            .IsInEnum()
            .WithMessage("Schedule type must be a valid ScheduleType value.");

        RuleFor(x => x.EnemyTribalWarsIds)
            .Must(ids => ids is null || ids.All(id => id > 0))
            .WithMessage("All enemy Tribal Wars IDs must be positive integers.");
    }
}
