import { apiClient } from "../config/api";
import type {
  MainActionSettings,
  SaveMainActionSettingsRequest,
} from "../types/mainActionSettings";

export const mainActionSettingsService = {
  async getMainActionSettings(scheduleId: string): Promise<MainActionSettings> {
    const { data } = await apiClient.get<MainActionSettings>(
      `/schedules/${scheduleId}/mainaction`,
    );
    return data;
  },

  async saveMainActionSettings(
    scheduleId: string,
    request: SaveMainActionSettingsRequest,
  ): Promise<MainActionSettings> {
    const { data } = await apiClient.put<MainActionSettings>(
      `/schedules/${scheduleId}/mainaction`,
      request,
    );
    return data;
  },
};
