using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Schedules.Queries;
using TWAction.Application.Schedules.Services;
using TWAction.Domain.Schedules;

namespace TWAction.UnitTests.Handlers;

public sealed class GetTroopsStateHandlerTests
{
    private readonly ITroopsStateRepository _troopsStateRepository;
    private readonly TroopsStateCompressionService _compressionService;
    private readonly TroopsStateValidator _troopsValidator;
    private readonly TroopsStateStatsExtractor _statsExtractor;
    private readonly IValidator<GetTroopsStateQuery> _fluentValidator;
    private readonly GetTroopsStateHandler _handler;

    public GetTroopsStateHandlerTests()
    {
        _troopsStateRepository = Substitute.For<ITroopsStateRepository>();
        _compressionService = new TroopsStateCompressionService();
        _troopsValidator = new TroopsStateValidator();
        _statsExtractor = new TroopsStateStatsExtractor();
        _fluentValidator = Substitute.For<IValidator<GetTroopsStateQuery>>();
        _handler = new GetTroopsStateHandler(
            _troopsStateRepository,
            _compressionService,
            _troopsValidator,
            _statsExtractor,
            _fluentValidator);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsFailure()
    {
        var query = new GetTroopsStateQuery(Guid.NewGuid());
        var failures = new List<ValidationFailure> { new("ScheduleId", "Schedule not found.") };

        _fluentValidator.ValidateAsync(query, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Schedule not found.");
        await _troopsStateRepository.DidNotReceive().GetByScheduleIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_ReturnsTroopsState()
    {
        var scheduleId = Guid.NewGuid();
        var query = new GetTroopsStateQuery(scheduleId);
        var rawData = BuildValidRawData();
        var compressedData = _compressionService.Compress(rawData);
        var troopsStateId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddHours(-2);
        var updatedAt = DateTimeOffset.UtcNow.AddHours(-1);

        _fluentValidator.ValidateAsync(query, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

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

    private static string BuildValidRawData()
    {
        return "PlayerName,Village,Spear,Sword,Archer,Marcher,Catapult,Axe,Polearm,Ram,Trebuchet\n" +
               "PlayerOne,500|500,1,2,3,4,5,6,7,8,9";
    }
}
