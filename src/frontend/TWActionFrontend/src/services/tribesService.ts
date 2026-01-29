import { apiClient } from "../config/api";
import type { Tribe } from "../types/tribe";

export const tribesService = {
  async getTribesForWorld(world: string): Promise<Tribe[]> {
    const { data } = await apiClient.get<Tribe[]>(`/worlds/${world}/tribes`);
    return data;
  },
};
