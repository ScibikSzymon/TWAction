using ActionGenerator.Application.Common.Validators;
using ActionGenerator.Application.Features.MainActions.Dtos;
using FluentValidation;

namespace ActionGenerator.Application.Features.MainActions.Validators;

public sealed class GenerateMainActionRequestValidator : AbstractValidator<GenerateMainActionRequest>
{
    public GenerateMainActionRequestValidator()
    {
        RuleFor(x => x.AllyVillages)
            .NotEmpty()
            .WithMessage("AllyVillages cannot be empty");

        RuleFor(x => x.Targets)
            .NotEmpty()
            .WithMessage("Targets cannot be empty");

        RuleFor(x => x.MaxNobleDistance)
            .GreaterThan(0u)
            .WithMessage("MaxNobleDistance must be greater than 0");

        RuleForEach(x => x.AllyVillages)
            .SetValidator(new VillageDtoValidator());

        RuleForEach(x => x.Targets)
            .SetValidator(new TargetDtoValidator());
    }
}
