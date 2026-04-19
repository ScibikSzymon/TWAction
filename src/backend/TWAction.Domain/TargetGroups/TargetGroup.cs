using TWAction.Domain.Templates;

namespace TWAction.Domain.TargetGroups;

/// <summary>
/// Domain entity representing a named group of enemy villages that are all targeted
/// with the same attack wave schedule during a Main Action.
/// </summary>
public class TargetGroup
{
    public Guid Id { get; set; }

    /// <summary>The schedule this group belongs to.</summary>
    public Guid ScheduleId { get; set; }

    /// <summary>Display name for the group (e.g., "Grimnar – fala 1").</summary>
    public required string Name { get; set; }

    /// <summary>Enemy village coordinates in "X|Y" format (e.g., "473|490").</summary>
    public List<string> VillageCoordinates { get; set; } = [];

    /// <summary>Attack waves applied to every village in this group.</summary>
    public List<TemplateWave> Waves { get; set; } = [];

    /// <summary>
    /// Optional reference to the template used as the starting point for these waves.
    /// Stored for informational purposes only — no FK constraint.
    /// </summary>
    public Guid? BaseTemplateId { get; set; }

    /// <summary>
    /// Snapshot of the base template name at the time of group creation.
    /// Kept for display even if the original template is later renamed or deleted.
    /// </summary>
    public string? BaseTemplateName { get; set; }
}
