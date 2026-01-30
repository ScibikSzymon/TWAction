using ActionGenerator.Application.Reconnaissance.Commands;
using FluentValidation;

namespace ActionGenerator.Application.Reconnaissance.Validators;

public class GenerateReconnaissanceActionsCommandValidator : AbstractValidator<GenerateReconnaissanceActionsCommand>
{
    public GenerateReconnaissanceActionsCommandValidator()
    {
        RuleFor(x => x.MinArrivalTime)
            .GreaterThan(x => x.MinDepartureTime)
            .WithMessage("MinArrivalTime must be after MinDepartureTime.");

        RuleFor(x => x.MaxArrivalTime)
            .GreaterThan(x => x.MinArrivalTime)
            .WithMessage("MaxArrivalTime must be after MinArrivalTime.");

        RuleFor(x => x.SourceVillages)
            .NotEmpty()
            .WithMessage("At least one source village is required.");

        RuleFor(x => x.TargetVillages)
            .NotEmpty()
            .WithMessage("At least one target village is required.");

        RuleFor(x => x.MinDistanceToFront)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MinDistanceToFront must be non-negative.");

        RuleFor(x => x.MinSpyCount)
            .GreaterThan(0)
            .WithMessage("MinSpyCount must be greater than zero.");

        RuleFor(x => x.MaxPopulationInSourceVillage)
            .GreaterThan(0)
            .WithMessage("MaxPopulationInSourceVillage must be greater than zero.");
    }
}
