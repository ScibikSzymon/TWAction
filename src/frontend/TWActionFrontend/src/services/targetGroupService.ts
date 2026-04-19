import { apiClient } from "../config/api";
import type {
  TargetGroup,
  CreateTargetGroupRequest,
  UpdateTargetGroupRequest,
} from "../types/targetGroup";

export const targetGroupService = {
  async getGroups(scheduleId: string): Promise<TargetGroup[]> {
    const response = await apiClient.get<TargetGroup[]>(
      `/schedules/${scheduleId}/target-groups`,
    );
    return response.data;
  },

  async createGroup(
    scheduleId: string,
    request: CreateTargetGroupRequest,
  ): Promise<TargetGroup> {
    const response = await apiClient.post<TargetGroup>(
      `/schedules/${scheduleId}/target-groups`,
      request,
    );
    return response.data;
  },

  async updateGroup(
    scheduleId: string,
    groupId: string,
    request: UpdateTargetGroupRequest,
  ): Promise<TargetGroup> {
    const response = await apiClient.put<TargetGroup>(
      `/schedules/${scheduleId}/target-groups/${groupId}`,
      request,
    );
    return response.data;
  },

  async deleteGroup(scheduleId: string, groupId: string): Promise<void> {
    await apiClient.delete(`/schedules/${scheduleId}/target-groups/${groupId}`);
  },
};
