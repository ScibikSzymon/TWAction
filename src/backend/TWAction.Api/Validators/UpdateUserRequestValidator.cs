using FluentValidation;
using TWAction.Application.Users.DTOs;

namespace TWAction.Api.Validators;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .MaximumLength(320)
            .WithMessage("Email must not exceed 320 characters.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(200)
            .WithMessage("Display name must not exceed 200 characters.")
            .When(x => x.DisplayName is not null);

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Role must be a valid UserRole value.");

        RuleFor(x => x.SubscriptionTier).IsInEnum().When(x => x.SubscriptionTier.HasValue);
        RuleFor(x => x.ScheduleLimitOverride).GreaterThanOrEqualTo(0).When(x => x.ScheduleLimitOverride.HasValue);
        RuleFor(x => x.TemplateLimitOverride).GreaterThanOrEqualTo(0).When(x => x.TemplateLimitOverride.HasValue);
        RuleFor(x => x.TroopsUploadLimitOverride).GreaterThanOrEqualTo(0).When(x => x.TroopsUploadLimitOverride.HasValue);
    }
}
