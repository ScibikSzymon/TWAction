namespace TWAction.Api.Validators;

using FluentValidation;
using TWAction.Api.Endpoints;

public sealed class GetTribesRequestValidator : AbstractValidator<GetTribesRequest>
{
    public GetTribesRequestValidator()
    {
        RuleFor(x => x.World)
            .IsInEnum()
            .WithMessage("World must be a valid WorldType value.");
    }
}
