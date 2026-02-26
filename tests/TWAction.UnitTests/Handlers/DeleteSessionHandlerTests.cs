using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using TWAction.Application.Users.Commands;
using TWAction.Application.Users.Interfaces;
using TWAction.Domain.Users;

namespace TWAction.UnitTests.Handlers;

public sealed class DeleteSessionHandlerTests
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IValidator<DeleteSessionCommand> _fluentValidator;
    private readonly DeleteSessionHandler _handler;

    public DeleteSessionHandlerTests()
    {
        _sessionRepository = Substitute.For<IUserSessionRepository>();
        _fluentValidator = Substitute.For<IValidator<DeleteSessionCommand>>();
        _fluentValidator.ValidateAsync(Arg.Any<DeleteSessionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new DeleteSessionHandler(_sessionRepository, _fluentValidator);
    }

    [Fact]
    public async Task Handle_WithValidSessionId_ReturnsSuccessResult()
    {
        var sessionId = Guid.NewGuid();
        var command = new DeleteSessionCommand(sessionId);

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        await _sessionRepository.Received(1).DeleteByIdAsync(sessionId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentSessionId_ReturnsFailureResult()
    {
        var sessionId = Guid.NewGuid();
        var command = new DeleteSessionCommand(sessionId);

        var failures = new List<ValidationFailure>
        {
            new("SessionId", $"Session with ID '{sessionId}' not found.")
        };
        _fluentValidator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain($"Session with ID '{sessionId}' not found.");
        await _sessionRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PropagatesCancellationToken()
    {
        var sessionId = Guid.NewGuid();
        var command = new DeleteSessionCommand(sessionId);
        var cancellationToken = new CancellationToken();

        await _handler.Handle(command, cancellationToken);

        await _sessionRepository.Received(1).DeleteByIdAsync(sessionId, cancellationToken);
    }
}
