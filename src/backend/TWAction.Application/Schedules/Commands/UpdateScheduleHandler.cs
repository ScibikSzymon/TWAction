using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Interfaces;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.Application.Schedules.Commands;

public sealed record UpdateScheduleCommand(
    Guid ScheduleId,
    string Name,
    WorldType World,
    ScheduleType ScheduleType,
    IReadOnlyList<int>? EnemyTribalWarsIds = null
);

public sealed class UpdateScheduleCommandValidator : AbstractValidator<UpdateScheduleCommand>
{
    public UpdateScheduleCommandValidator(
        ICurrentUserAccessor currentUser,
        IScheduleRepository scheduleRepository)
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID must not be empty.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Schedule name is required.")
            .MaximumLength(100)
            .WithMessage("Schedule name must not exceed 100 characters.");

        RuleFor(x => x.World)
            .IsInEnum()
            .WithMessage("World must be a valid WorldType value.");

        RuleFor(x => x.ScheduleType)
            .IsInEnum()
            .WithMessage("Schedule type must be a valid ScheduleType value.");

        RuleFor(x => x.EnemyTribalWarsIds)
            .Must(ids => ids is null || ids.All(id => id > 0))
            .WithMessage("All enemy Tribal Wars IDs must be positive integers.");

        RuleFor(x => x.ScheduleId)
            .MustAsync(async (scheduleId, cancellationToken) =>
            {
                var schedule = await scheduleRepository.GetByIdAsync(scheduleId, cancellationToken);
                return schedule is not null;
            })
            .WithMessage(command => $"Schedule with ID '{command.ScheduleId}' not found.")
            .When(command => currentUser.TryGetUserId(out _));

        RuleFor(x => x.ScheduleId)
            .MustAsync(async (scheduleId, cancellationToken) =>
            {
                var schedule = await scheduleRepository.GetByIdAsync(scheduleId, cancellationToken);
                if (schedule is null) return true;
                if (currentUser.IsAdmin) return true;
                currentUser.TryGetUserId(out var userId);
                return schedule.UserGuid == userId;
            })
            .WithMessage("Schedule not found for specified user.")
            .When(command => currentUser.TryGetUserId(out _));
    }
}

public class UpdateScheduleHandler(
    IScheduleRepository scheduleRepository,
    ITribesService tribesService,
    IValidator<UpdateScheduleCommand> fluentValidator)
{
    /// <summary>
    /// Handles schedule update — runs FluentValidation first, then applies changes.
    /// </summary>
    public async Task<Result<ScheduleDto>> Handle(UpdateScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync<UpdateScheduleCommand, ScheduleDto>(
            fluentValidator, command, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        var schedule = (await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken))!;

        schedule.Name = command.Name;

        // Clear enemies if world changed
        if (command.World != schedule.World)
        {
            schedule.Enemies.Clear();
        }

        schedule.World = command.World;
        schedule.ScheduleType = command.ScheduleType;

        // Handle enemies if provided        
        if (command.EnemyTribalWarsIds != null)
        {
            if (command.EnemyTribalWarsIds.Any())
            {
                try
                {
                    var tribes = await tribesService.GetTribesAsync(command.World, cancellationToken);

                    var enemies = tribes
                        .Where(t => command.EnemyTribalWarsIds.Contains(t.TribalWarsId))
                        .ToList();

                    if (enemies.Count != command.EnemyTribalWarsIds.Count)
                    {
                        var notFound = command.EnemyTribalWarsIds
                            .Except(enemies.Select(e => e.TribalWarsId))
                            .ToList();
                        return Result.Failure<ScheduleDto>($"The following tribe IDs were not found: {string.Join(", ", notFound)}");
                    }

                    schedule.Enemies = enemies;
                }
                catch (Exception ex)
                {
                    return Result.Failure<ScheduleDto>($"Failed to fetch tribes: {ex.Message}");
                }
            }
            else
            {
                // Empty list means clear enemies
                schedule.Enemies = new List<TribeInfo>();
            }
        }

        await scheduleRepository.UpdateAsync(schedule, cancellationToken);

        return Result.Success(IScheduleMapper.ToDto(schedule));

    }
}

