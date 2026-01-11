using TWAction.Application.Interfaces;

namespace TWAction.Application.Handlers;

public sealed record DeleteSessionCommand(Guid SessionId);

public class DeleteSessionHandler
{
    private readonly IUserSessionRepository _sessionRepository;

    public DeleteSessionHandler(IUserSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task Handle(DeleteSessionCommand command, CancellationToken cancellationToken = default)
    {
        await _sessionRepository.DeleteByIdAsync(command.SessionId, cancellationToken);
    }
}
