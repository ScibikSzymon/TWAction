import { apiClient } from "../config/api";
import type {
  Schedule,
  CreateScheduleRequest,
  UpdateScheduleRequest,
} from "../types/schedule";

export const scheduleService = {
  async getSchedulesByUser(userId: string): Promise<Schedule[]> {
    const { data } = await apiClient.get<Schedule[]>(`/schedules/${userId}`);
    return data;
  },

  async getScheduleById(userId: string, scheduleId: string): Promise<Schedule> {
    const { data } = await apiClient.get<Schedule>(
      `/schedules/${userId}/${scheduleId}`,
    );
    return data;
  },

  async createSchedule(request: CreateScheduleRequest): Promise<Schedule> {
    const { data } = await apiClient.post<Schedule>("/schedules", request);
    return data;
  },

  async updateSchedule(
    scheduleId: string,
    request: UpdateScheduleRequest,
  ): Promise<Schedule> {
    const { data } = await apiClient.put<Schedule>(
      `/schedules/${scheduleId}`,
      request,
    );
    return data;
  },

  async deleteSchedule(scheduleId: string): Promise<void> {
    await apiClient.delete(`/schedules/${scheduleId}`);
  },
};
