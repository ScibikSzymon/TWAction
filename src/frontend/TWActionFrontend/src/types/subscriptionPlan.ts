export interface SubscriptionPlanLimits {
  subscriptionTier: "Free" | "Premium";
  scheduleLimit: number | null;
  templateLimit: number | null;
  troopsUploadLimit: number | null;
  troopsUploadWindowHours: number;
}
