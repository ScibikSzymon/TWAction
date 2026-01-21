import { useState, useEffect, useCallback } from "react";
import type { User } from "../types/user";
import { authService } from "../services/authService";

export const useAuth = () => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchUser = async () => {
      try {
        console.log("useAuth: Starting to fetch user...");
        setIsLoading(true);
        setError(null);
        const userData = await authService.getMe();
        console.log("useAuth: User fetched successfully", userData);
        setUser(userData);
      } catch (err) {
        console.error("useAuth: Error fetching user:", err);
        setUser(null);
        // Don't set error for unauthenticated users - it's expected
      } finally {
        console.log("useAuth: Setting isLoading to false");
        setIsLoading(false);
      }
    };

    fetchUser();
  }, []);

  const login = useCallback(() => {
    authService.redirectToGoogleLogin();
  }, []);

  const logout = useCallback(async () => {
    try {
      await authService.logout();
      setUser(null);
    } catch (err) {
      console.error("Logout error:", err);
      setError("Failed to logout");
    }
  }, []);

  return {
    user,
    isLoading,
    error,
    login,
    logout,
    isAuthenticated: !!user,
  };
};
