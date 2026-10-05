namespace TWAction.Domain.Users;

public sealed class SubscriptionPlanLimitsEntity
{
    public SubscriptionTier SubscriptionTier { get; set; }

    public int? ScheduleLimit { get; set; }

    public int? TemplateLimit { get; set; }

    public int? TroopsUploadLimit { get; set; }

    public int TroopsUploadWindowHours { get; set; }
}
