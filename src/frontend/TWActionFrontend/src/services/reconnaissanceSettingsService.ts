import { apiClient } from "../config/api";
import type {
  ReconnaissanceSettings,
  SaveReconnaissanceSettingsRequest,
} from "../types/reconnaissanceSettings";

export const reconnaissanceSettingsService = {
  async getReconnaissanceSettings(
    scheduleId: string,
  ): Promise<ReconnaissanceSettings> {
    const { data } = await apiClient.get<ReconnaissanceSettings>(
      `/schedules/${scheduleId}/reconnaissance`,
    );
    return data;
  },

  async saveReconnaissanceSettings(
    scheduleId: string,
    request: SaveReconnaissanceSettingsRequest,
  ): Promise<ReconnaissanceSettings> {
    const { data } = await apiClient.put<ReconnaissanceSettings>(
      `/schedules/${scheduleId}/reconnaissance`,
      request,
    );
    return data;
  },
};
