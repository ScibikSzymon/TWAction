import { apiClient } from "../config/api";
import type { UpdateUserRequest, User, UserSession } from "../types/user";

export const userService = {
  async getAllUsers(): Promise<User[]> {
    const { data } = await apiClient.get<User[]>("/users");
    return data;
  },

  async getUser(userId: string): Promise<User> {
    const { data } = await apiClient.get<User>(`/users/${userId}`);
    return data;
  },

  async updateUser(userId: string, request: UpdateUserRequest): Promise<User> {
    const { data } = await apiClient.put<User>(`/users/${userId}`, request);
    return data;
  },

  async deleteUser(userId: string): Promise<void> {
    await apiClient.delete(`/users/${userId}`);
  },

  async getUserSessions(userId: string): Promise<UserSession[]> {
    const { data } = await apiClient.get<UserSession[]>(`/users/${userId}/sessions`);
    return data;
  },

  async deleteSession(userId: string, sessionId: string): Promise<void> {
    await apiClient.delete(`/users/${userId}/sessions/${sessionId}`);
  },

  async deleteAllSessions(userId: string): Promise<void> {
    await apiClient.delete(`/users/${userId}/sessions`);
  },
};
