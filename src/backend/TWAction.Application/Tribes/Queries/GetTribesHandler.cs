using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Tribes.DTOs;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.Schedules;

namespace TWAction.Application.Tribes.Queries;

public sealed record GetTribesQuery(WorldType World);

public sealed class GetTribesQueryValidator : AbstractValidator<GetTribesQuery>
{
    public GetTribesQueryValidator()
    {
        RuleFor(x => x.World)
            .IsInEnum()
            .WithMessage("World must be a valid world type.");
    }
}

public class GetTribesHandler(
    ITribesService tribesService,
    IValidator<GetTribesQuery> fluentValidator)
{
    public async Task<Result<IReadOnlyList<TribeDto>>> Handle(GetTribesQuery query, CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync<GetTribesQuery, IReadOnlyList<TribeDto>>(
            fluentValidator, query, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        try
        {
            var tribes = await tribesService.GetTribesAsync(query.World, cancellationToken);
            return Result.Success(ITribeMapper.ToDtos(tribes));
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<IReadOnlyList<TribeDto>>($"Failed to fetch tribes: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<IReadOnlyList<TribeDto>>($"Invalid tribes data: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<TribeDto>>($"Error fetching tribes: {ex.Message}");
        }
    }
}






