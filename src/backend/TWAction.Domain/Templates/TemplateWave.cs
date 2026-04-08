namespace TWAction.Domain.Templates;

/// <summary>
/// Represents a single wave of attacks within a target template.
/// Defines a time window, number of commands, and the command type to send.
/// </summary>
public class TemplateWave
{
    /// <summary>Earliest time the wave should arrive / depart (hh:mm).</summary>
    public TimeOnly MinTime { get; set; }

    /// <summary>Latest time the wave should arrive / depart (hh:mm).</summary>
    public TimeOnly MaxTime { get; set; }

    /// <summary>Number of commands (attacks) in this wave.</summary>
    public int CommandNumber { get; set; }

    /// <summary>
    /// Command type identifier matching ActionGenerator's CommandType enum.
    /// Use <see cref="CommandTypeConstants"/> for valid values.
    /// </summary>
    public required string CommandType { get; set; }
}
