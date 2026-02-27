namespace TWAction.Application.AttackCommands.Interfaces;

using TWAction.Application.AttackCommands.DTOs;

public interface IPlemionaRozpiskiApiClient
{
    /// <summary>
    /// Sends attack commands to plemionarozpiski.pl API.
    /// </summary>
    Task<PlemionaRozpiskiSendResult> SendCommandsAsync(
        IReadOnlyList<PlemionaRozpiskiCommandDto> commands,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all commands for a given operation name from plemionarozpiski.pl API.
    /// </summary>
    Task<bool> DeleteCommandsAsync(
        string operationName,
        CancellationToken cancellationToken = default);
}
