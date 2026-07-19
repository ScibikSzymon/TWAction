using AwesomeAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TWAction.Application.Schedules.Commands;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Application.Users.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;
using TWAction.Domain.Users;

namespace TWAction.UnitTests.Handlers;

public sealed class CreateScheduleHandlerTests
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITribesService _tribesService;
    private readonly CreateScheduleHandler _handler;

    public CreateScheduleHandlerTests()
    {
        _scheduleRepository = Substitute.For<IScheduleRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _tribesService = Substitute.For<ITribesService>();
        _handler = new CreateScheduleHandler(_scheduleRepository, _userRepository, _tribesService);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesScheduleSuccessfully()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _scheduleRepository.AddAsync(Arg.Any<ScheduleEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<ScheduleEntity>()!));

        var command = new CreateScheduleCommand(
            UserId: userId,
            Name: "Test Schedule",
            World: WorldType.pl218,
            ScheduleType: ScheduleType.Main,
            EnemyTribalWarsIds: []
        );

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Test Schedule");
        result.Value!.World.Should().Be(WorldType.pl218);
        result.Value!.ScheduleType.Should().Be(ScheduleType.Main);
        result.Value!.UserId.Should().Be(userId);

        await _scheduleRepository.Received(1).AddAsync(
            Arg.Is<ScheduleEntity>(s =>
                s != null &&
                s.Name == "Test Schedule" &&
                s.World == WorldType.pl218 &&
                s.ScheduleType == ScheduleType.Main &&
                s.UserGuid == userId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsFailure()
    {
        var userId = Guid.NewGuid();

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        var command = new CreateScheduleCommand(
            UserId: userId,
            Name: "Test Schedule",
            World: WorldType.pl218,
            ScheduleType: ScheduleType.Fake,
            EnemyTribalWarsIds: []
        );

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(userId.ToString());
        result.Error.Should().Contain("not found");

        await _scheduleRepository.DidNotReceive().AddAsync(
            Arg.Any<ScheduleEntity>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidEnemyTribes_AddsEnemiesToSchedule()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var tribes = new List<TribeInfo>
        {
            new() { TribalWarsId = 1, Name = "Tribe One", Short = "T1", VillagesCount = 100 },
            new() { TribalWarsId = 2, Name = "Tribe Two", Short = "T2", VillagesCount = 200 },
            new() { TribalWarsId = 3, Name = "Tribe Three", Short = "T3", VillagesCount = 300 }
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _tribesService.GetTribesAsync(WorldType.pl219, Arg.Any<CancellationToken>())
            .Returns(tribes);

        _scheduleRepository.AddAsync(Arg.Any<ScheduleEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<ScheduleEntity>()!));

        var command = new CreateScheduleCommand(
            UserId: userId,
            Name: "Schedule with Enemies",
            World: WorldType.pl219,
            ScheduleType: ScheduleType.Reconnaissance,
            EnemyTribalWarsIds: [1, 3]
        );

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.EnemyIds.Should().HaveCount(2);
        result.Value!.EnemyIds.Should().Contain(1);
        result.Value!.EnemyIds.Should().Contain(3);

        await _tribesService.Received(1).GetTribesAsync(WorldType.pl219, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentEnemyTribes_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var tribes = new List<TribeInfo>
        {
            new() { TribalWarsId = 1, Name = "Tribe One", Short = "T1", VillagesCount = 100 },
            new() { TribalWarsId = 2, Name = "Tribe Two", Short = "T2", VillagesCount = 200 }
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _tribesService.GetTribesAsync(WorldType.pl218, Arg.Any<CancellationToken>())
            .Returns(tribes);

        var command = new CreateScheduleCommand(
            UserId: userId,
            Name: "Schedule with Missing Enemies",
            World: WorldType.pl218,
            ScheduleType: ScheduleType.Main,
            EnemyTribalWarsIds: [1, 999, 888]
        );

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("999");
        result.Error.Should().Contain("888");
        result.Error.Should().Contain("not found");

        await _scheduleRepository.DidNotReceive().AddAsync(
            Arg.Any<ScheduleEntity>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTribesServiceThrowsException_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _tribesService.GetTribesAsync(WorldType.pl218, Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("Network error"));

        var command = new CreateScheduleCommand(
            UserId: userId,
            Name: "Schedule with Service Error",
            World: WorldType.pl218,
            ScheduleType: ScheduleType.Fake,
            EnemyTribalWarsIds: [1, 2]
        );

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Failed to fetch tribes");
        result.Error.Should().Contain("Network error");

        await _scheduleRepository.DidNotReceive().AddAsync(
            Arg.Any<ScheduleEntity>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyEnemyList_DoesNotCallTribesService()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _scheduleRepository.AddAsync(Arg.Any<ScheduleEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<ScheduleEntity>()!));

        var command = new CreateScheduleCommand(
            UserId: userId,
            Name: "Schedule without Enemies",
            World: WorldType.pl220,
            ScheduleType: ScheduleType.Main,
            EnemyTribalWarsIds: []
        );

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.EnemyIds.Should().BeEmpty();

        await _tribesService.DidNotReceive().GetTribesAsync(
            Arg.Any<WorldType>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(WorldType.pl218)]
    [InlineData(WorldType.pl219)]
    [InlineData(WorldType.pl220)]
    [InlineData(WorldType.pl221)]
    [InlineData(WorldType.pl222)]
    [InlineData(WorldType.pl223)]
    public async Task Handle_WithDifferentWorldTypes_CreatesScheduleWithCorrectWorld(WorldType world)
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _scheduleRepository.AddAsync(Arg.Any<ScheduleEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<ScheduleEntity>()!));

        var command = new CreateScheduleCommand(
            UserId: userId,
            Name: "World Test Schedule",
            World: world,
            ScheduleType: ScheduleType.Main,
            EnemyTribalWarsIds: []
        );

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.World.Should().Be(world);
    }

    [Theory]
    [InlineData(ScheduleType.Fake)]
    [InlineData(ScheduleType.Reconnaissance)]
    [InlineData(ScheduleType.Main)]
    public async Task Handle_WithDifferentScheduleTypes_CreatesScheduleWithCorrectType(ScheduleType scheduleType)
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _scheduleRepository.AddAsync(Arg.Any<ScheduleEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<ScheduleEntity>()!));

        var command = new CreateScheduleCommand(
            UserId: userId,
            Name: "Type Test Schedule",
            World: WorldType.pl218,
            ScheduleType: scheduleType,
            EnemyTribalWarsIds: []
        );

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ScheduleType.Should().Be(scheduleType);
    }

    [Fact]
    public async Task Handle_CreatesScheduleWithUniqueId()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _scheduleRepository.AddAsync(Arg.Any<ScheduleEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<ScheduleEntity>()!));

        var command = new CreateScheduleCommand(
            UserId: userId,
            Name: "ID Test Schedule",
            World: WorldType.pl218,
            ScheduleType: ScheduleType.Main,
            EnemyTribalWarsIds: []
        );

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_SetsCreationDateToUtcNow()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var beforeTest = DateTimeOffset.UtcNow;

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _scheduleRepository.AddAsync(Arg.Any<ScheduleEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<ScheduleEntity>()!));

        var command = new CreateScheduleCommand(
            UserId: userId,
            Name: "Date Test Schedule",
            World: WorldType.pl218,
            ScheduleType: ScheduleType.Main,
            EnemyTribalWarsIds: []
        );

        var result = await _handler.Handle(command);
        var afterTest = DateTimeOffset.UtcNow;

        result.IsSuccess.Should().BeTrue();
        result.Value!.CreationDate.Should().BeOnOrAfter(beforeTest);
        result.Value!.CreationDate.Should().BeOnOrBefore(afterTest);
    }
}
