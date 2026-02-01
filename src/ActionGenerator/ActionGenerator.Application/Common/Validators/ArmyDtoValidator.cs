using ActionGenerator.Application.Common.DTOs;
using FluentValidation;

namespace ActionGenerator.Application.Common.Validators;

public sealed class ArmyDtoValidator : AbstractValidator<ArmyDto>
{
    public ArmyDtoValidator()
    {
        RuleFor(x => x.Spear)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Spear must be greater than or equal to 0");

        RuleFor(x => x.Sword)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Sword must be greater than or equal to 0");

        RuleFor(x => x.Axe)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Axe must be greater than or equal to 0");

        RuleFor(x => x.Archer)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Archer must be greater than or equal to 0");

        RuleFor(x => x.Spy)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Spy must be greater than or equal to 0");

        RuleFor(x => x.Light)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Light must be greater than or equal to 0");

        RuleFor(x => x.HorseArcher)
            .GreaterThanOrEqualTo(0)
            .WithMessage("HorseArcher must be greater than or equal to 0");

        RuleFor(x => x.Heavy)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Heavy must be greater than or equal to 0");

        RuleFor(x => x.Ram)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Ram must be greater than or equal to 0");

        RuleFor(x => x.Catapult)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Catapult must be greater than or equal to 0");

        RuleFor(x => x.Noble)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Noble must be greater than or equal to 0");
    }
}
