import { apiClient } from "../config/api";
import type {
  TroopsState,
  UploadTroopsStateRequest,
} from "../types/troopsState";

export const troopsStateService = {
  async getTroopsState(scheduleId: string): Promise<TroopsState> {
    const { data } = await apiClient.get<TroopsState>(
      `/schedules/${scheduleId}/troops`,
    );
    return data;
  },

  async uploadTroopsState(
    scheduleId: string,
    request: UploadTroopsStateRequest,
  ): Promise<TroopsState> {
    const { data } = await apiClient.post<TroopsState>(
      `/schedules/${scheduleId}/troops`,
      request,
    );
    return data;
  },
};
