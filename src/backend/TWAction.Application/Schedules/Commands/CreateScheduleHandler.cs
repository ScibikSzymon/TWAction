using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Interfaces;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Application.Users.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.Application.Schedules.Commands;

public sealed record CreateScheduleCommand(
    Guid UserId,
    string Name,
    WorldType World,
    ScheduleType ScheduleType,
    IReadOnlyList<int> EnemyTribalWarsIds
);

public sealed class CreateScheduleCommandValidator : AbstractValidator<CreateScheduleCommand>
{
    public CreateScheduleCommandValidator(
        ICurrentUserAccessor currentUser,
        IUserRepository userRepository)
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID must not be empty.");

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

        RuleFor(x => x.UserId)
            .MustAsync(async (userId, cancellationToken) =>
            {
                var user = await userRepository.GetByIdAsync(userId, cancellationToken);
                return user is not null;
            })
            .WithMessage(command => $"User with ID '{command.UserId}' not found.")
            .When(command => currentUser.TryGetUserId(out _));
    }
}

public class CreateScheduleHandler(
    IScheduleRepository scheduleRepository,
    ITribesService tribesService,
    IValidator<CreateScheduleCommand> fluentValidator)
{
    public async Task<Result<ScheduleDto>> Handle(CreateScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync<CreateScheduleCommand, ScheduleDto>(
            fluentValidator, command, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        var schedule = new ScheduleEntity
        {
            Id = Guid.NewGuid(),
            UserGuid = command.UserId,
            Name = command.Name,
            CreationDate = DateTimeOffset.UtcNow,
            World = command.World,
            ScheduleType = command.ScheduleType,
            Enemies = []
        };


        // Handle enemies if provided
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

        await scheduleRepository.AddAsync(schedule, cancellationToken);

        return Result.Success(IScheduleMapper.ToDto(schedule));
    }
}


