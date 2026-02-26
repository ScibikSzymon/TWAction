import { apiClient } from "../config/api";
import type { AttackCommandsSummary } from "../types/attackCommands";

export const attackCommandsService = {
  async getAttackCommandsSummary(
    scheduleId: string,
  ): Promise<AttackCommandsSummary | null> {
    try {
      const { data } = await apiClient.get<AttackCommandsSummary>(
        `/schedules/${scheduleId}/attack-commands/summary`,
      );
      return data;
    } catch (err: unknown) {
      if (
        err &&
        typeof err === "object" &&
        "response" in err &&
        (err as { response?: { status?: number } }).response?.status === 404
      ) {
        return null;
      }
      throw err;
    }
  },
};
