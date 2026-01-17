using AwesomeAssertions;
using NSubstitute;
using TWAction.Application.Handlers;
using TWAction.Application.Interfaces;
using TWAction.Domain.Entities;

namespace TWAction.UnitTests.Handlers;

public sealed class GetUserBySessionHandlerTests
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly GetUserBySessionHandler _handler;

    public GetUserBySessionHandlerTests()
    {
        _sessionRepository = Substitute.For<IUserSessionRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new GetUserBySessionHandler(_sessionRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_WithValidSession_ReturnsUserDto()
    {
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new UserSessionEntity
        {
            Id = sessionId,
            UserId = userId,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2)
        };
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10)
        };
        var query = new GetUserBySessionQuery(sessionId);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(userId);
        result.Value.Email.Should().Be("test@example.com");
        result.Value.DisplayName.Should().Be("Test User");
        result.Value.Provider.Should().Be("google");
    }

    [Fact]
    public async Task Handle_WithNonExistentSession_ReturnsFailureResult()
    {
        var sessionId = Guid.NewGuid();
        var query = new GetUserBySessionQuery(sessionId);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((UserSessionEntity?)null);

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain($"Session with ID '{sessionId}' not found.");
        await _userRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExpiredSession_ReturnsFailureResult()
    {
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new UserSessionEntity
        {
            Id = sessionId,
            UserId = userId,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1)
        };
        var query = new GetUserBySessionQuery(sessionId);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Session has expired.");
        await _userRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsFailureResult()
    {
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new UserSessionEntity
        {
            Id = sessionId,
            UserId = userId,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2)
        };
        var query = new GetUserBySessionQuery(sessionId);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain($"User with ID '{userId}' not found.");
    }

    [Fact]
    public async Task Handle_WithSessionExpiringExactlyNow_ReturnsFailureResult()
    {
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var session = new UserSessionEntity
        {
            Id = sessionId,
            UserId = userId,
            ExpiresAt = now.AddMilliseconds(-100)
        };
        var query = new GetUserBySessionQuery(sessionId);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Session has expired.");
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PropagatesCancellationToken()
    {
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new UserSessionEntity
        {
            Id = sessionId,
            UserId = userId,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2)
        };
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var query = new GetUserBySessionQuery(sessionId);
        var cancellationToken = new CancellationToken();

        _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            .Returns(session);
        _userRepository.GetByIdAsync(userId, cancellationToken)
            .Returns(user);

        await _handler.Handle(query, cancellationToken);

        await _sessionRepository.Received(1).GetByIdAsync(sessionId, cancellationToken);
        await _userRepository.Received(1).GetByIdAsync(userId, cancellationToken);
    }

    [Fact]
    public async Task Handle_WithUserWithoutDisplayName_ReturnsUserDtoWithNullDisplayName()
    {
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new UserSessionEntity
        {
            Id = sessionId,
            UserId = userId,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2)
        };
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = null,
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var query = new GetUserBySessionQuery(sessionId);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.DisplayName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_VerifiesSessionExpirationBeforeRetrievingUser()
    {
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new UserSessionEntity
        {
            Id = sessionId,
            UserId = userId,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1)
        };
        var query = new GetUserBySessionQuery(sessionId);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        await _userRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
