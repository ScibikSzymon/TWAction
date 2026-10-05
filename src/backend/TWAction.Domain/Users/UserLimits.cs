namespace TWAction.Domain.Users;

public static class UserLimits
{
    public static int? Schedules(UserEntity user, SubscriptionPlanLimitsEntity plan) =>
        Resolve(user, user.ScheduleLimitOverride, plan.ScheduleLimit);

    public static int? Templates(UserEntity user, SubscriptionPlanLimitsEntity plan) =>
        Resolve(user, user.TemplateLimitOverride, plan.TemplateLimit);

    public static int? TroopsUploads(UserEntity user, SubscriptionPlanLimitsEntity plan) =>
        Resolve(user, user.TroopsUploadLimitOverride, plan.TroopsUploadLimit);

    public static TimeSpan UploadWindow(SubscriptionPlanLimitsEntity plan) =>
        TimeSpan.FromHours(plan.TroopsUploadWindowHours);

    private static int? Resolve(UserEntity user, int? userOverride, int? planLimit) =>
        userOverride ?? (user.Role == UserRole.Admin ? null : planLimit);
}
