export interface User {
  id: string;
  email: string;
  displayName?: string;
  provider: string;
  role: string;
  createdAt: string;
}

export type UserRole = "User" | "Admin";

export interface UpdateUserRequest {
  email: string;
  displayName: string;
  role: UserRole;
}

export interface UserSession {
  id: string;
  userId: string;
  expiresAt: string;
  isActive: boolean;
}
