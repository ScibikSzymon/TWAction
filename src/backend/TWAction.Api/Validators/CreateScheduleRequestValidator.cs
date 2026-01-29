namespace TWAction.Api.Validators;

using FluentValidation;
using TWAction.Api.Endpoints;
using TWAction.Domain.Schedules;

/// <summary>
/// Validator for <see cref="CreateScheduleRequest"/>.
/// </summary>
public sealed class CreateScheduleRequestValidator : AbstractValidator<CreateScheduleRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateScheduleRequestValidator"/> class.
    /// </summary>
    public CreateScheduleRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

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
