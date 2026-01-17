using TWAction.Application.Interfaces;
using TWAction.Application.Common;

namespace TWAction.Application.Handlers;

public sealed record DeleteSessionCommand(Guid SessionId);

public class DeleteSessionHandler
{
    private readonly IUserSessionRepository _sessionRepository;

    public DeleteSessionHandler(IUserSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    /// <summary>
    /// Handles the deletion of a user session.
    /// </summary>
    /// <returns>A result indicating success or failure with an error message.</returns>
    public async Task<Result> Handle(DeleteSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(command.SessionId, cancellationToken);
        
        if (session is null)
        {
            return Result.Failure($"Session with ID '{command.SessionId}' not found.");
        }

        await _sessionRepository.DeleteByIdAsync(command.SessionId, cancellationToken);
        
        return Result.Success();
    }
}
