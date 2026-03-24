import { useAuth } from "./useAuth.ts";

export function useRequireRole(role: string): {
  authorized: boolean;
  isLoading: boolean;
  isAuthenticated: boolean;
} {
  const { isAuthenticated, isLoading, roles } = useAuth();

  return {
    authorized: isAuthenticated && roles.includes(role),
    isLoading,
    isAuthenticated,
  };
}
