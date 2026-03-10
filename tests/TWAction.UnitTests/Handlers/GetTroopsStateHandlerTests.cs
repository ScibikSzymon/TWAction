using AwesomeAssertions;
using NSubstitute;
using TWAction.Application.Interfaces;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Schedules.Queries;
using TWAction.Application.Schedules.Services;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.UnitTests.Handlers;

public sealed class GetTroopsStateHandlerTests
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly ITroopsStateRepository _troopsStateRepository;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly TroopsStateCompressionService _compressionService;
    private readonly TroopsStateValidator _validator;
    private readonly TroopsStateStatsExtractor _statsExtractor;
    private readonly GetTroopsStateHandler _handler;

    // Initializes the handler and dependencies for each test.
    public GetTroopsStateHandlerTests()
    {
        _scheduleRepository = Substitute.For<IScheduleRepository>();
        _troopsStateRepository = Substitute.For<ITroopsStateRepository>();
        _currentUser = Substitute.For<ICurrentUserAccessor>();
        _compressionService = new TroopsStateCompressionService();
        _validator = new TroopsStateValidator();
        _statsExtractor = new TroopsStateStatsExtractor();
        _handler = new GetTroopsStateHandler(
            _scheduleRepository,
            _troopsStateRepository,
            _currentUser,
            _compressionService,
            _validator,
            _statsExtractor);
    }

    // Returns failure when the user is not authenticated.
    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsFailure()
    {
        var query = new GetTroopsStateQuery(Guid.NewGuid());

        _currentUser.TryGetUserId(out Arg.Any<Guid>()).Returns(false);

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("User is not authenticated.");
        await _scheduleRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _troopsStateRepository.DidNotReceive().GetByScheduleIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // Returns failure when the schedule does not exist.
    [Fact]
    public async Task Handle_WhenScheduleNotFound_ReturnsFailure()
    {
        var scheduleId = Guid.NewGuid();
        var query = new GetTroopsStateQuery(scheduleId);
        var userId = Guid.NewGuid();

        SetAuthenticatedUser(userId, isAdmin: false);
        _scheduleRepository.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns((ScheduleEntity?)null);

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(scheduleId.ToString());
        result.Error.Should().Contain("not found");
        await _troopsStateRepository.DidNotReceive().GetByScheduleIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // Returns failure when the schedule belongs to another user and the user is not admin.
    [Fact]
    public async Task Handle_WhenScheduleOwnedByAnotherUser_ReturnsFailure()
    {
        var scheduleId = Guid.NewGuid();
        var query = new GetTroopsStateQuery(scheduleId);
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        SetAuthenticatedUser(userId, isAdmin: false);
        _scheduleRepository.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(new ScheduleEntity
            {
                Id = scheduleId,
                UserGuid = otherUserId,
                Name = "Schedule",
                CreationDate = DateTimeOffset.UtcNow,
                World = WorldType.pl218,
                ScheduleType = ScheduleType.Main
            });

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Schedule not found for specified user.");
        await _troopsStateRepository.DidNotReceive().GetByScheduleIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // Returns the troops state when the user owns the schedule.
    [Fact]
    public async Task Handle_WhenScheduleOwnedByUser_ReturnsTroopsState()
    {
        var scheduleId = Guid.NewGuid();
        var query = new GetTroopsStateQuery(scheduleId);
        var userId = Guid.NewGuid();
        var rawData = BuildValidRawData();
        var compressedData = _compressionService.Compress(rawData);
        var troopsStateId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddHours(-2);
        var updatedAt = DateTimeOffset.UtcNow.AddHours(-1);

        SetAuthenticatedUser(userId, isAdmin: false);
        _scheduleRepository.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(new ScheduleEntity
            {
                Id = scheduleId,
                UserGuid = userId,
                Name = "Schedule",
                CreationDate = DateTimeOffset.UtcNow,
                World = WorldType.pl218,
                ScheduleType = ScheduleType.Main
            });
        _troopsStateRepository.GetByScheduleIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(new TroopsStateEntity
            {
                Id = troopsStateId,
                ScheduleId = scheduleId,
                CompressedData = compressedData,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            });

        var result = await _handler.Handle(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new TroopsStateDto
        {
            Id = troopsStateId,
            ScheduleId = scheduleId,
            VillageCount = 1,
            PlayerCount = 1,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        });
    }

    // Configures the current user accessor for authentication scenarios.
    private void SetAuthenticatedUser(Guid userId, bool isAdmin)
    {
        _currentUser.TryGetUserId(out Arg.Any<Guid>()).Returns(callInfo =>
        {
            callInfo[0] = userId;
            return true;
        });
        _currentUser.IsAdmin.Returns(isAdmin);
    }

    // Builds a valid troops state CSV payload for testing.
    private static string BuildValidRawData()
    {
        return "PlayerName,Village,Spear,Sword,Archer,Marcher,Catapult,Axe,Polearm,Ram,Trebuchet\n" +
               "PlayerOne,500|500,1,2,3,4,5,6,7,8,9";
    }
}
