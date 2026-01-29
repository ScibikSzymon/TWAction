namespace TWAction.Api.Validators;

using FluentValidation;
using TWAction.Api.Endpoints;
using TWAction.Domain.Schedules;

/// <summary>
/// Validator for <see cref="UpdateScheduleRequest"/>.
/// </summary>
public sealed class UpdateScheduleRequestValidator : AbstractValidator<UpdateScheduleRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateScheduleRequestValidator"/> class.
    /// </summary>
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
