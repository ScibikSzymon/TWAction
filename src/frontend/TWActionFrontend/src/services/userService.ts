import { apiClient } from "../config/api";
import type { User } from "../types/user";

export const userService = {
  async getAllUsers(): Promise<User[]> {
    const { data } = await apiClient.get<User[]>("/users");
    return data;
  },
};
