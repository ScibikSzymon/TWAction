using AwesomeAssertions;
using NSubstitute;
using TWAction.Application.Users.Commands;
using TWAction.Application.Users.Interfaces;
using TWAction.Domain.Users;

namespace TWAction.UnitTests.Handlers;

public sealed class DeleteSessionHandlerTests
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly DeleteSessionHandler _handler;

    public DeleteSessionHandlerTests()
    {
        _sessionRepository = Substitute.For<IUserSessionRepository>();
        _handler = new DeleteSessionHandler(_sessionRepository);
    }

    [Fact]
    public async Task Handle_WithValidSessionId_ReturnsSuccessResult()
    {
        var sessionId = Guid.NewGuid();
        var session = new UserSessionEntity
        {
            Id = sessionId,
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };
        var command = new DeleteSessionCommand(sessionId);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        await _sessionRepository.Received(1).DeleteByIdAsync(sessionId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentSessionId_ReturnsFailureResult()
    {
        var sessionId = Guid.NewGuid();
        var command = new DeleteSessionCommand(sessionId);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((UserSessionEntity?)null);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain($"Session with ID '{sessionId}' not found.");
        await _sessionRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PropagatesCancellationToken()
    {
        var sessionId = Guid.NewGuid();
        var session = new UserSessionEntity
        {
            Id = sessionId,
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };
        var command = new DeleteSessionCommand(sessionId);
        var cancellationToken = new CancellationToken();

        _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            .Returns(session);

        await _handler.Handle(command, cancellationToken);

        await _sessionRepository.Received(1).GetByIdAsync(sessionId, cancellationToken);
        await _sessionRepository.Received(1).DeleteByIdAsync(sessionId, cancellationToken);
    }
}
