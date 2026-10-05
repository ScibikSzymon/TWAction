export interface User {
  id: string;
  email: string;
  displayName?: string;
  provider: string;
  role: string;
  subscriptionTier: "Free" | "Premium";
  scheduleLimitOverride: number | null;
  templateLimitOverride: number | null;
  troopsUploadLimitOverride: number | null;
  createdAt: string;
}

export type UserRole = "User" | "Admin";

export interface UpdateUserRequest {
  email: string;
  displayName: string;
  role: UserRole;
  subscriptionTier: "Free" | "Premium";
  scheduleLimitOverride: number | null;
  templateLimitOverride: number | null;
  troopsUploadLimitOverride: number | null;
}

export interface UserLimits {
  subscriptionTier: "Free" | "Premium";
  scheduleLimit: number | null;
  scheduleCount: number;
  templateLimit: number | null;
  templateCount: number;
  troopsUploadLimit: number | null;
  troopsUploadWindowHours: number;
}

export interface UserSession {
  id: string;
  userId: string;
  expiresAt: string;
  isActive: boolean;
}
