import { apiClient } from "../config/api";
import type { User } from "../types/user";

export const authService = {
  async getMe(): Promise<User> {
    const { data } = await apiClient.get<User>("/auth/me");
    return data;
  },

  async logout(): Promise<void> {
    await apiClient.post("/auth/logout");
  },

  redirectToGoogleLogin(): void {
    window.location.href = `${apiClient.defaults.baseURL}/auth/google`;
  },
};
