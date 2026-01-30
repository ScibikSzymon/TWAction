using AwesomeAssertions;
using NSubstitute;
using TWAction.Application.Settings.Queries;
using TWAction.Application.Settings.Interfaces;
using TWAction.Domain.Settings;

namespace TWAction.UnitTests.Handlers;

public sealed class GetReconnaissanceSettingsHandlerTests
{
    private readonly IReconnaissanceSettingsRepository _settingsRepository;
    private readonly GetReconnaissanceSettingsHandler _handler;

    public GetReconnaissanceSettingsHandlerTests()
    {
        _settingsRepository = Substitute.For<IReconnaissanceSettingsRepository>();
        _handler = new GetReconnaissanceSettingsHandler(_settingsRepository);
    }

    [Fact]
    public async Task Handle_WhenSettingsExist_ReturnsSettings()
    {
        var scheduleId = Guid.NewGuid();
        var settingsId = Guid.NewGuid();
        var query = new GetReconnaissanceSettingsQuery(scheduleId);

        var settings = new ReconnaissanceSettings
        {
            Id = settingsId,
            ScheduleId = scheduleId,
            MinDepartureTime = DateTimeOffset.UtcNow,
            MinArrivalTime = DateTimeOffset.UtcNow.AddHours(1),
            MaxArrivalTime = DateTimeOffset.UtcNow.AddHours(2),
            MinDistanceToFront = 5,
            MinSpyCount = 1,
            MaxPopulationInSourceVillage = 100,
            SkipNightSendings = false
        };

        _settingsRepository.GetByScheduleIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns(settings);

        var result = await _handler.Handle(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(settingsId);
        result.Value.ScheduleId.Should().Be(scheduleId);
        result.Value.MinDepartureTime.Should().Be(settings.MinDepartureTime);
        result.Value.MinArrivalTime.Should().Be(settings.MinArrivalTime);
        result.Value.MaxArrivalTime.Should().Be(settings.MaxArrivalTime);
        result.Value.MinDistanceToFront.Should().Be(5);
        result.Value.MinSpyCount.Should().Be(1);
        result.Value.MaxPopulationInSourceVillage.Should().Be(100);
        result.Value.SkipNightSendings.Should().BeFalse();

        await _settingsRepository.Received(1).GetByScheduleIdAsync(scheduleId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSettingsNotFound_ReturnsFailure()
    {
        var scheduleId = Guid.NewGuid();
        var query = new GetReconnaissanceSettingsQuery(scheduleId);

        _settingsRepository.GetByScheduleIdAsync(scheduleId, Arg.Any<CancellationToken>())
            .Returns((ReconnaissanceSettings?)null);

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        result.Error.Should().Contain(scheduleId.ToString());

        await _settingsRepository.Received(1).GetByScheduleIdAsync(scheduleId, Arg.Any<CancellationToken>());
    }
}
