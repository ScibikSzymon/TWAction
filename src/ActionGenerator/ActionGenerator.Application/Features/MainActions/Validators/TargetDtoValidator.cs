using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Application.Common.Validators;
using ActionGenerator.Domain.Enums;
using FluentValidation;

namespace ActionGenerator.Application.Features.MainActions.Validators;

public sealed class TargetDtoValidator : AbstractValidator<TargetDto>
{
    private static readonly IReadOnlyList<CommandType> ValidCommandTypes =
    [
        CommandType.Off,
        CommandType.Catapults,
        CommandType.FakeOffensive,
        CommandType.FakeDefensive,
        CommandType.NobleWithFullOff,
        CommandType.NobleWithHalfOff,
        CommandType.NobleWithQuarterOffensive,
        CommandType.NobleWith150Axes,
        CommandType.NobleWith100HeavyCavalry,
        CommandType.RandomNoble,
        CommandType.NobleWithDeff
    ];

    public TargetDtoValidator()
    {
        RuleFor(x => x.MinArrivalTime)
            .LessThanOrEqualTo(x => x.MaxArrivalTime)
            .WithMessage("MinArrivalTime must be before or equal to MaxArrivalTime");

        RuleFor(x => x.CommandNumber)
            .GreaterThan(0u)
            .WithMessage("CommandNumber must be greater than 0");

        RuleFor(x => x.CommandType)
            .Must(t => ValidCommandTypes.Contains(t))
            .WithMessage("CommandType must be a valid main-action type (Reconnaissance is not allowed here)");

        RuleFor(x => x.Village)
            .NotNull()
            .WithMessage("Village cannot be null");

        RuleFor(x => x.Village)
            .SetValidator(new VillageSmallDtoValidator())
            .When(x => x.Village is not null);
    }
}
