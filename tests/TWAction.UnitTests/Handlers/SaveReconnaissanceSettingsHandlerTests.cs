using AwesomeAssertions;
using NSubstitute;
using TWAction.Application.Settings.Commands;
using TWAction.Application.Settings.Interfaces;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Settings;

namespace TWAction.UnitTests.Handlers;

public sealed class SaveReconnaissanceSettingsHandlerTests
{
    private readonly IReconnaissanceSettingsRepository _settingsRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly SaveReconnaissanceSettingsHandler _handler;

    public SaveReconnaissanceSettingsHandlerTests()
    {
        _settingsRepository = Substitute.For<IReconnaissanceSettingsRepository>();
        _scheduleRepository = Substitute.For<IScheduleRepository>();
        _handler = new SaveReconnaissanceSettingsHandler(_settingsRepository, _scheduleRepository);
    }

    [Fact]
    public async Task Handle_WithNewSettings_CreatesSettings()
    {
        var scheduleId = Guid.NewGuid();
        var minDepartureTime = DateTimeOffset.UtcNow;
        var minArrivalTime = minDepartureTime.AddHours(1);
        var maxArrivalTime = minArrivalTime.AddHours(1);
        var command = new SaveReconnaissanceSettingsCommand(
            scheduleId,
            minDepartureTime,
            minArrivalTime,
            maxArrivalTime,
            MinDistanceToFront: 5,
            MinSpyCount: 1,
            MaxPopulationInSourceVillage: 100,
            SkipNightSendings: false
        );

        var schedule = new ScheduleEntity
        {
            Id = scheduleId,
            ScheduleType = ScheduleType.Reconnaissance
        };

        _scheduleRepository.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(schedule);

        _settingsRepository.GetByScheduleIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns((ReconnaissanceSettings?)null);

        _settingsRepository.CreateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var settings = callInfo.Arg<ReconnaissanceSettings>();
                settings.Id = Guid.NewGuid();
                return settings;
            });

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.ScheduleId.Should().Be(scheduleId);
        result.Value.MinDepartureTime.Should().Be(minDepartureTime);
        result.Value.MinArrivalTime.Should().Be(minArrivalTime);
        result.Value.MaxArrivalTime.Should().Be(maxArrivalTime);
        result.Value.MinDistanceToFront.Should().Be(5);
        result.Value.MinSpyCount.Should().Be(1);
        result.Value.MaxPopulationInSourceVillage.Should().Be(100);
        result.Value.SkipNightSendings.Should().BeFalse();

        await _scheduleRepository.Received(1).GetByIdAsync(scheduleId, Arg.Any<CancellationToken>());
        await _settingsRepository.Received(1).GetByScheduleIdAsync(scheduleId, Arg.Any<CancellationToken>());
        await _settingsRepository.Received(1).CreateAsync(
            Arg.Is<ReconnaissanceSettings>(s =>
                s.ScheduleId == scheduleId &&
                s.MinDepartureTime == minDepartureTime &&
                s.MinArrivalTime == minArrivalTime &&
                s.MaxArrivalTime == maxArrivalTime &&
                s.MinDistanceToFront == 5 &&
                s.MinSpyCount == 1 &&
                s.MaxPopulationInSourceVillage == 100 &&
                s.SkipNightSendings == false),
            Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().UpdateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingSettings_UpdatesSettings()
    {
        var scheduleId = Guid.NewGuid();
        var settingsId = Guid.NewGuid();
        var minDepartureTime = DateTimeOffset.UtcNow;
        var minArrivalTime = minDepartureTime.AddHours(1);
        var maxArrivalTime = minArrivalTime.AddHours(1);
        var command = new SaveReconnaissanceSettingsCommand(
            scheduleId,
            minDepartureTime,
            minArrivalTime,
            maxArrivalTime,
            MinDistanceToFront: 10,
            MinSpyCount: 2,
            MaxPopulationInSourceVillage: 200,
            SkipNightSendings: true
        );

        var schedule = new ScheduleEntity
        {
            Id = scheduleId,
            ScheduleType = ScheduleType.Reconnaissance
        };

        var existingSettings = new ReconnaissanceSettings
        {
            Id = settingsId,
            ScheduleId = scheduleId,
            MinDepartureTime = DateTimeOffset.UtcNow.AddDays(-1),
            MinArrivalTime = DateTimeOffset.UtcNow.AddDays(-1).AddHours(1),
            MaxArrivalTime = DateTimeOffset.UtcNow.AddDays(-1).AddHours(2),
            MinDistanceToFront = 3,
            MinSpyCount = 1,
            MaxPopulationInSourceVillage = 50,
            SkipNightSendings = false
        };

        _scheduleRepository.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(schedule);

        _settingsRepository.GetByScheduleIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(existingSettings);

        _settingsRepository.UpdateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<ReconnaissanceSettings>());

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(settingsId);
        result.Value.ScheduleId.Should().Be(scheduleId);
        result.Value.MinDepartureTime.Should().Be(minDepartureTime);
        result.Value.MinArrivalTime.Should().Be(minArrivalTime);
        result.Value.MaxArrivalTime.Should().Be(maxArrivalTime);
        result.Value.MinDistanceToFront.Should().Be(10);
        result.Value.MinSpyCount.Should().Be(2);
        result.Value.MaxPopulationInSourceVillage.Should().Be(200);
        result.Value.SkipNightSendings.Should().BeTrue();

        await _scheduleRepository.Received(1).GetByIdAsync(scheduleId, Arg.Any<CancellationToken>());
        await _settingsRepository.Received(1).GetByScheduleIdAsync(scheduleId, Arg.Any<CancellationToken>());
        await _settingsRepository.Received(1).UpdateAsync(
            Arg.Is<ReconnaissanceSettings>(s =>
                s.Id == settingsId &&
                s.ScheduleId == scheduleId &&
                s.MinDepartureTime == minDepartureTime &&
                s.MinArrivalTime == minArrivalTime &&
                s.MaxArrivalTime == maxArrivalTime &&
                s.MinDistanceToFront == 10 &&
                s.MinSpyCount == 2 &&
                s.MaxPopulationInSourceVillage == 200 &&
                s.SkipNightSendings == true),
            Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().CreateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenScheduleNotFound_ReturnsFailure()
    {
        var scheduleId = Guid.NewGuid();
        var command = new SaveReconnaissanceSettingsCommand(
            scheduleId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2),
            MinDistanceToFront: 5,
            MinSpyCount: 1,
            MaxPopulationInSourceVillage: 100,
            SkipNightSendings: false
        );

        _scheduleRepository.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns((ScheduleEntity?)null);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        result.Error.Should().Contain(scheduleId.ToString());

        await _scheduleRepository.Received(1).GetByIdAsync(scheduleId, Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().GetByScheduleIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().CreateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().UpdateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenScheduleTypeIsNotReconnaissance_ReturnsFailure()
    {
        var scheduleId = Guid.NewGuid();
        var command = new SaveReconnaissanceSettingsCommand(
            scheduleId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2),
            MinDistanceToFront: 5,
            MinSpyCount: 1,
            MaxPopulationInSourceVillage: 100,
            SkipNightSendings: false
        );

        var schedule = new ScheduleEntity
        {
            Id = scheduleId,
            ScheduleType = ScheduleType.Main
        };

        _scheduleRepository.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(schedule);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Reconnaissance settings can only be set");
        result.Error.Should().Contain("Reconnaissance");
        result.Error.Should().Contain(ScheduleType.Main.ToString());

        await _scheduleRepository.Received(1).GetByIdAsync(scheduleId, Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().GetByScheduleIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().CreateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().UpdateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMinArrivalTimeBeforeOrEqualMinDepartureTime_ReturnsFailure()
    {
        var scheduleId = Guid.NewGuid();
        var minDepartureTime = DateTimeOffset.UtcNow;
        var command = new SaveReconnaissanceSettingsCommand(
            scheduleId,
            minDepartureTime,
            minDepartureTime,
            minDepartureTime.AddHours(1),
            MinDistanceToFront: 5,
            MinSpyCount: 1,
            MaxPopulationInSourceVillage: 100,
            SkipNightSendings: false
        );

        var schedule = new ScheduleEntity
        {
            Id = scheduleId,
            ScheduleType = ScheduleType.Reconnaissance
        };

        _scheduleRepository.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(schedule);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("MinArrivalTime must be after MinDepartureTime");

        await _scheduleRepository.Received(1).GetByIdAsync(scheduleId, Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().GetByScheduleIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().CreateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().UpdateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMaxArrivalTimeBeforeOrEqualMinArrivalTime_ReturnsFailure()
    {
        var scheduleId = Guid.NewGuid();
        var minDepartureTime = DateTimeOffset.UtcNow;
        var minArrivalTime = minDepartureTime.AddHours(1);
        var command = new SaveReconnaissanceSettingsCommand(
            scheduleId,
            minDepartureTime,
            minArrivalTime,
            minArrivalTime,
            MinDistanceToFront: 5,
            MinSpyCount: 1,
            MaxPopulationInSourceVillage: 100,
            SkipNightSendings: false
        );

        var schedule = new ScheduleEntity
        {
            Id = scheduleId,
            ScheduleType = ScheduleType.Reconnaissance
        };

        _scheduleRepository.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(schedule);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("MaxArrivalTime must be after MinArrivalTime");

        await _scheduleRepository.Received(1).GetByIdAsync(scheduleId, Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().GetByScheduleIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().CreateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().UpdateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMinSpyCountLessThanOne_ReturnsFailure()
    {
        var scheduleId = Guid.NewGuid();
        var minDepartureTime = DateTimeOffset.UtcNow;
        var command = new SaveReconnaissanceSettingsCommand(
            scheduleId,
            minDepartureTime,
            minDepartureTime.AddHours(1),
            minDepartureTime.AddHours(2),
            MinDistanceToFront: 5,
            MinSpyCount: 0,
            MaxPopulationInSourceVillage: 100,
            SkipNightSendings: false
        );

        var schedule = new ScheduleEntity
        {
            Id = scheduleId,
            ScheduleType = ScheduleType.Reconnaissance
        };

        _scheduleRepository.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(schedule);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("MinSpyCount must be at least 1");

        await _scheduleRepository.Received(1).GetByIdAsync(scheduleId, Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().GetByScheduleIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().CreateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().UpdateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMinDistanceToFrontNegative_ReturnsFailure()
    {
        var scheduleId = Guid.NewGuid();
        var minDepartureTime = DateTimeOffset.UtcNow;
        var command = new SaveReconnaissanceSettingsCommand(
            scheduleId,
            minDepartureTime,
            minDepartureTime.AddHours(1),
            minDepartureTime.AddHours(2),
            MinDistanceToFront: -1,
            MinSpyCount: 1,
            MaxPopulationInSourceVillage: 100,
            SkipNightSendings: false
        );

        var schedule = new ScheduleEntity
        {
            Id = scheduleId,
            ScheduleType = ScheduleType.Reconnaissance
        };

        _scheduleRepository.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(schedule);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("MinDistanceToFront cannot be negative");

        await _scheduleRepository.Received(1).GetByIdAsync(scheduleId, Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().GetByScheduleIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().CreateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().UpdateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMaxPopulationInSourceVillageNegative_ReturnsFailure()
    {
        var scheduleId = Guid.NewGuid();
        var minDepartureTime = DateTimeOffset.UtcNow;
        var command = new SaveReconnaissanceSettingsCommand(
            scheduleId,
            minDepartureTime,
            minDepartureTime.AddHours(1),
            minDepartureTime.AddHours(2),
            MinDistanceToFront: 5,
            MinSpyCount: 1,
            MaxPopulationInSourceVillage: -1,
            SkipNightSendings: false
        );

        var schedule = new ScheduleEntity
        {
            Id = scheduleId,
            ScheduleType = ScheduleType.Reconnaissance
        };

        _scheduleRepository.GetByIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(schedule);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("MaxPopulationInSourceVillage cannot be negative");

        await _scheduleRepository.Received(1).GetByIdAsync(scheduleId, Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().GetByScheduleIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().CreateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
        await _settingsRepository.DidNotReceive().UpdateAsync(Arg.Any<ReconnaissanceSettings>(), Arg.Any<CancellationToken>());
    }
}
