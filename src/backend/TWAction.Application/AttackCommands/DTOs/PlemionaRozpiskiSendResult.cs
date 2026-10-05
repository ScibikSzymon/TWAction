namespace TWAction.Application.AttackCommands.DTOs;

/// <summary>
/// Result of sending commands to plemionarozpiski.pl API.
/// </summary>
public sealed class PlemionaRozpiskiSendResult
{
    public required bool Success { get; init; }
    public required int CommandsSentCount { get; init; }
    public string? ErrorMessage { get; init; }
}
