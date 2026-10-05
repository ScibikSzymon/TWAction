using ActionGenerator.Application.Common.DTOs;
using FluentValidation;

namespace ActionGenerator.Application.Common.Validators;

public sealed class VillageDtoValidator : AbstractValidator<VillageDto>
{
    public VillageDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Village Id must be greater than 0");

        RuleFor(x => x.PlayerId)
            .GreaterThan(0)
            .WithMessage("PlayerId must be greater than 0");

        RuleFor(x => x.X)
            .GreaterThanOrEqualTo(0)
            .WithMessage("X coordinate must be greater than or equal to 0");

        RuleFor(x => x.Y)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Y coordinate must be greater than or equal to 0");

        RuleFor(x => x.Army)
            .NotNull()
            .WithMessage("Army cannot be null");
    } 
}
