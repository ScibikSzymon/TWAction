import { apiClient } from "../config/api";
import type {
  NobleBudget,
  PlayerNobleStats,
  SaveNobleBudgetsRequest,
} from "../types/nobleBudget";

export const nobleBudgetService = {
  async getNobleBudgets(scheduleId: string): Promise<NobleBudget[]> {
    const { data } = await apiClient.get<NobleBudget[]>(
      `/schedules/${scheduleId}/noble-budgets`,
    );
    return data;
  },

  async saveNobleBudgets(
    scheduleId: string,
    request: SaveNobleBudgetsRequest,
  ): Promise<NobleBudget[]> {
    const { data } = await apiClient.post<NobleBudget[]>(
      `/schedules/${scheduleId}/noble-budgets`,
      request,
    );
    return data;
  },

  async getPlayerNobleStats(scheduleId: string): Promise<PlayerNobleStats[]> {
    const { data } = await apiClient.get<PlayerNobleStats[]>(
      `/schedules/${scheduleId}/player-noble-stats`,
    );
    return data;
  },
};
