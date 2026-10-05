namespace TWAction.Application.AttackCommands.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// DTO for sending a command to plemionarozpiski.pl API.
/// </summary>
public sealed class PlemionaRozpiskiCommandDto
{
    [JsonPropertyName("commandNumberId")]
    public required int CommandNumberId { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("minTime")]
    public required string MinTime { get; init; }

    [JsonPropertyName("maxTime")]
    public required string MaxTime { get; init; }

    [JsonPropertyName("attackTime")]
    public required string AttackTime { get; init; }

    [JsonPropertyName("source")]
    public required string Source { get; init; }

    [JsonPropertyName("sourceId")]
    public required string SourceId { get; init; }

    [JsonPropertyName("target")]
    public required string Target { get; init; }

    [JsonPropertyName("targetId")]
    public required string TargetId { get; init; }

    [JsonPropertyName("playerId")]
    public required string PlayerId { get; init; }

    [JsonPropertyName("playerName")]
    public required string PlayerName { get; init; }

    [JsonPropertyName("world")]
    public required string World { get; init; }

    [JsonPropertyName("operationName")]
    public required string OperationName { get; init; }
}
