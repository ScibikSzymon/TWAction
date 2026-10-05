import { apiClient } from "../config/api";
import type { GenerateMainActionsResponse } from "../types/mainAction";

export const mainActionGeneratorService = {
  async generateMainActions(
    scheduleId: string,
  ): Promise<GenerateMainActionsResponse> {
    const { data } = await apiClient.post<GenerateMainActionsResponse>(
      `/schedules/${scheduleId}/main-action/generate`,
      undefined,
      { timeout: 5 * 60 * 1000 },
    );
    return data;
  },
};
