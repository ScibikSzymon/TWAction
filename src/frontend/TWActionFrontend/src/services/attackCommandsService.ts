import { apiClient } from "../config/api";
import type { AttackCommand } from "../types/attackCommands";

export const attackCommandsService = {
  async getAttackCommands(scheduleId: string): Promise<AttackCommand[]> {
    const { data } = await apiClient.get<AttackCommand[]>(
      `/schedules/${scheduleId}/attack-commands`,
    );
    return data;
  },
};
