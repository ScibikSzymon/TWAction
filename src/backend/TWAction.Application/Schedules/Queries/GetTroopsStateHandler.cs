using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Interfaces;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Schedules.Services;

namespace TWAction.Application.Schedules.Queries;

public sealed record GetTroopsStateQuery(Guid ScheduleId);

public sealed class GetTroopsStateValidator : AbstractValidator<GetTroopsStateQuery>
{
    public GetTroopsStateValidator(
        ICurrentUserAccessor currentUser,
        IScheduleRepository scheduleRepository,
        ITroopsStateRepository troopsStateRepository)
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID must not be empty.");

        RuleFor(x => x)
            .Custom((query, context) =>
            {
                if (!currentUser.TryGetUserId(out _))
                {
                    context.AddFailure("User is not authenticated.");
                }
            });

        RuleFor(x => x.ScheduleId)
            .MustAsync(async (scheduleId, cancellationToken) =>
            {
                var schedule = await scheduleRepository.GetByIdAsync(scheduleId, cancellationToken);
                return schedule is not null;
            })
            .WithMessage(query => $"Schedule with ID '{query.ScheduleId}' not found.")
            .When(query => currentUser.TryGetUserId(out _));

        RuleFor(x => x.ScheduleId)
            .MustAsync(async (scheduleId, cancellationToken) =>
            {
                var schedule = await scheduleRepository.GetByIdAsync(scheduleId, cancellationToken);
                if (schedule is null) return true; // Already caught by previous rule
                if (currentUser.IsAdmin) return true;
                currentUser.TryGetUserId(out var userId);
                return schedule.UserGuid == userId;
            })
            .WithMessage("Schedule not found for specified user.")
            .When(query => currentUser.TryGetUserId(out _));

        RuleFor(x => x.ScheduleId)
            .MustAsync(async (scheduleId, cancellationToken) =>
            {
                var troopsState = await troopsStateRepository.GetByScheduleIdAsync(scheduleId, cancellationToken);
                return troopsState is not null;
            })
            .WithMessage(query => $"Troops state for schedule '{query.ScheduleId}' not found.")
            .When(query => currentUser.TryGetUserId(out _));
    }
}

public class GetTroopsStateHandler(
    ITroopsStateRepository troopsStateRepository,
    TroopsStateCompressionService compressionService,
    TroopsStateValidator troopsValidator,
    TroopsStateStatsExtractor statsExtractor,
    IValidator<GetTroopsStateQuery> fluentValidator)
{
    /// <summary>
    /// Handles the query — runs FluentValidation first, then business logic.
    /// </summary>
    public async Task<Result<TroopsStateDto>> Handle(GetTroopsStateQuery query, CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync<GetTroopsStateQuery, TroopsStateDto>(
            fluentValidator, query, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        var troopsState = await troopsStateRepository.GetByScheduleIdAsync(query.ScheduleId, cancellationToken);

        // Decompress data to extract stats
        var decompressResult = compressionService.Decompress(troopsState!.CompressedData);
        if (decompressResult.IsFailure)
        {
            return Result.Failure<TroopsStateDto>(decompressResult.Error);
        }

        // Parse to get stats
        var parseResult = troopsValidator.ValidateAndParse(decompressResult.Value);
        if (parseResult.IsFailure)
        {
            return Result.Failure<TroopsStateDto>($"Failed to parse troops data: {parseResult.Error}");
        }

        var stats = statsExtractor.Extract(parseResult.Value);

        var dto = new TroopsStateDto
        {
            Id = troopsState.Id,
            ScheduleId = troopsState.ScheduleId,
            VillageCount = stats.VillageCount,
            PlayerCount = stats.PlayerCount,
            CreatedAt = troopsState.CreatedAt,
            UpdatedAt = troopsState.UpdatedAt
        };

        return Result.Success(dto);
    }
}
