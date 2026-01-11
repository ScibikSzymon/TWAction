using AwesomeAssertions;
using NSubstitute;
using TWAction.Application.Handlers;
using TWAction.Application.Interfaces;
using TWAction.Domain.Entities;

namespace TWAction.UnitTests.Handlers;

public sealed class SignInWithGoogleHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly SignInWithGoogleHandler _handler;

    public SignInWithGoogleHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _sessionRepository = Substitute.For<IUserSessionRepository>();
        _handler = new SignInWithGoogleHandler(_userRepository, _sessionRepository);
    }

    [Fact]
    public async Task Handle_WithNewUser_CreatesUserAndSession()
    {
        var email = "test@example.com";
        var displayName = "Test User";
        var command = new SignInWithGoogleCommand(email, displayName);
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        _userRepository.FindByEmailAsync(email, "google", Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        _userRepository.AddAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var user = callInfo.Arg<UserEntity>();
                user.Id = userId;
                return user;
            });

        _sessionRepository.CreateSessionAsync(Arg.Any<UserSessionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var session = callInfo.Arg<UserSessionEntity>();
                session.Id = sessionId;
                return session;
            });

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SessionId.Should().Be(sessionId);
        result.Value.User.Email.Should().Be(email);
        result.Value.User.DisplayName.Should().Be(displayName);
        result.Value.User.Provider.Should().Be("google");

        await _userRepository.Received(1).FindByEmailAsync(email, "google", Arg.Any<CancellationToken>());
        await _userRepository.Received(1).AddAsync(
            Arg.Is<UserEntity>(u => 
                u.Email == email && 
                u.DisplayName == displayName && 
                u.Provider == "google"),
            Arg.Any<CancellationToken>());
        await _sessionRepository.Received(1).CreateSessionAsync(
            Arg.Is<UserSessionEntity>(s => s.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingUser_CreatesSessionOnly()
    {
        var email = "existing@example.com";
        var displayName = "Existing User";
        var command = new SignInWithGoogleCommand(email, displayName);
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var existingUser = new UserEntity
        {
            Id = userId,
            Email = email,
            DisplayName = "Original Name",
            Provider = "google",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-30)
        };

        _userRepository.FindByEmailAsync(email, "google", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        _sessionRepository.CreateSessionAsync(Arg.Any<UserSessionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var session = callInfo.Arg<UserSessionEntity>();
                session.Id = sessionId;
                return session;
            });

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SessionId.Should().Be(sessionId);
        result.Value.User.Id.Should().Be(userId);
        result.Value.User.Email.Should().Be(email);
        result.Value.User.DisplayName.Should().Be("Original Name");

        await _userRepository.Received(1).FindByEmailAsync(email, "google", Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await _sessionRepository.Received(1).CreateSessionAsync(
            Arg.Is<UserSessionEntity>(s => s.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNewUser_CreatesSessionWithEightHourExpiration()
    {
        var email = "test@example.com";
        var command = new SignInWithGoogleCommand(email, null);
        var beforeTest = DateTimeOffset.UtcNow;

        _userRepository.FindByEmailAsync(email, "google", Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        _userRepository.AddAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<UserEntity>());

        UserSessionEntity? capturedSession = null;
        _sessionRepository.CreateSessionAsync(Arg.Any<UserSessionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedSession = callInfo.Arg<UserSessionEntity>();
                return capturedSession;
            });

        await _handler.Handle(command);

        var afterTest = DateTimeOffset.UtcNow;
        capturedSession.Should().NotBeNull();
        capturedSession!.ExpiresAt.Should().BeCloseTo(beforeTest.AddHours(8), TimeSpan.FromSeconds(5));
        capturedSession.ExpiresAt.Should().BeCloseTo(afterTest.AddHours(8), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PropagatesCancellationToken()
    {
        var email = "test@example.com";
        var command = new SignInWithGoogleCommand(email, null);
        var cancellationToken = new CancellationToken();

        _userRepository.FindByEmailAsync(email, "google", cancellationToken)
            .Returns((UserEntity?)null);

        _userRepository.AddAsync(Arg.Any<UserEntity>(), cancellationToken)
            .Returns(callInfo => callInfo.Arg<UserEntity>());

        _sessionRepository.CreateSessionAsync(Arg.Any<UserSessionEntity>(), cancellationToken)
            .Returns(callInfo => callInfo.Arg<UserSessionEntity>());

        await _handler.Handle(command, cancellationToken);

        await _userRepository.Received(1).FindByEmailAsync(email, "google", cancellationToken);
        await _userRepository.Received(1).AddAsync(Arg.Any<UserEntity>(), cancellationToken);
        await _sessionRepository.Received(1).CreateSessionAsync(Arg.Any<UserSessionEntity>(), cancellationToken);
    }

    [Fact]
    public async Task Handle_WithNullDisplayName_CreatesUserWithNullDisplayName()
    {
        var email = "test@example.com";
        var command = new SignInWithGoogleCommand(email, null);

        _userRepository.FindByEmailAsync(email, "google", Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        _userRepository.AddAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<UserEntity>());

        _sessionRepository.CreateSessionAsync(Arg.Any<UserSessionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<UserSessionEntity>());

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.User.DisplayName.Should().BeNull();
        await _userRepository.Received(1).AddAsync(
            Arg.Is<UserEntity>(u => u.DisplayName == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCustomProvider_UsesSpecifiedProvider()
    {
        var email = "test@example.com";
        var customProvider = "custom-provider";
        var command = new SignInWithGoogleCommand(email, null, customProvider);

        _userRepository.FindByEmailAsync(email, customProvider, Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        _userRepository.AddAsync(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<UserEntity>());

        _sessionRepository.CreateSessionAsync(Arg.Any<UserSessionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<UserSessionEntity>());

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.User.Provider.Should().Be(customProvider);
        await _userRepository.Received(1).FindByEmailAsync(email, customProvider, Arg.Any<CancellationToken>());
    }
}
