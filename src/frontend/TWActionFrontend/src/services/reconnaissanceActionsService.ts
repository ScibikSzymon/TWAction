import { apiClient } from "../config/api";
import type { GenerateReconnaissanceActionsResponse } from "../types/reconnaissanceActions";

export const reconnaissanceActionsService = {
  async generateReconnaissanceActions(
    scheduleId: string,
  ): Promise<GenerateReconnaissanceActionsResponse> {
    const { data } =
      await apiClient.post<GenerateReconnaissanceActionsResponse>(
        `/schedules/${scheduleId}/reconnaissance/actions`,
      );
    return data;
  },
};
