using ActionGenerator.Application.Features.ReconnaissanceActions.DTOs;
using FluentValidation;

namespace ActionGenerator.Application.Features.ReconnaissanceActions.Validators;

public sealed class GenerateReconnaissanceActionsRequestValidator 
    : AbstractValidator<GenerateReconnaissanceActionsRequest>
{
    public GenerateReconnaissanceActionsRequestValidator()
    {
        RuleFor(x => x.MinDepartureTime)
            .LessThan(x => x.MinArrivalTime)
            .WithMessage("MinDepartureTime must be before MinArrivalTime");

        RuleFor(x => x.MinArrivalTime)
            .LessThanOrEqualTo(x => x.MaxArrivalTime)
            .WithMessage("MinArrivalTime must be before or equal to MaxArrivalTime");

        RuleFor(x => x.MinDistanceToFront)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MinDistanceToFront must be greater than or equal to 0");

        RuleFor(x => x.MinSpyCount)
            .GreaterThan(0)
            .WithMessage("MinSpyCount must be greater than 0");

        RuleFor(x => x.MaxPopulationInSourceVillage)
            .GreaterThan(0)
            .WithMessage("MaxPopulationInSourceVillage must be greater than 0");

        RuleFor(x => x.AllyVillages)
            .NotEmpty()
            .WithMessage("AllyVillages cannot be empty");

        RuleFor(x => x.EnemyVillages)
            .NotEmpty()
            .WithMessage("EnemyVillages cannot be empty");

        RuleForEach(x => x.AllyVillages)
            .SetValidator(new VillageDtoValidator());

        RuleForEach(x => x.EnemyVillages)
            .SetValidator(new VillageSmallDtoValidator());
    }
}

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

public sealed class VillageSmallDtoValidator : AbstractValidator<VillageSmallDto>
{
    public VillageSmallDtoValidator()
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
    }
}
