namespace TWAction.Domain.Templates;

/// <summary>
/// Represents a named template of attack wave groups that can be applied to a target.
/// Default templates (IsDefault = true) are read-only and shared by all users.
/// User templates (IsDefault = false) are owned by a specific user.
/// </summary>
public class TargetTemplate
{
    public Guid Id { get; set; }

    /// <summary>Null for default system templates that are shared across all users.</summary>
    public Guid? UserId { get; set; }

    public required string Name { get; set; }

    public bool IsDefault { get; set; }

    public List<TemplateWave> Waves { get; set; } = [];
}
