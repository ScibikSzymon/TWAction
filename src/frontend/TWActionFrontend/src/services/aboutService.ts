import { apiClient } from "../config/api";
import type { SubscriptionPlanLimits } from "../types/subscriptionPlan";

export const aboutService = {
  async getDefaultLimits(): Promise<SubscriptionPlanLimits[]> {
    const { data } = await apiClient.get<SubscriptionPlanLimits[]>(
      "/about/limits",
    );
    return data;
  },
};
