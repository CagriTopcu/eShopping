import { useQuery } from "@tanstack/react-query";
import { api, ApiRequestError } from "@/shared/api/client.ts";
import type { ProfileResponse } from "@/shared/types/common.types.ts";

export const profileKeys = {
  all: ["profile"] as const,
  me: () => [...profileKeys.all, "me"] as const,
};

export function useProfile() {
  return useQuery({
    queryKey: profileKeys.me(),
    queryFn: ({ signal }) =>
      api.get<ProfileResponse>("/api/v1/profile/me", undefined, signal),
    staleTime: 5 * 60 * 1000,
    gcTime: 30 * 60 * 1000,
    retry: (failureCount, error) => {
      // Don't retry on 404 (profile not created yet)
      if (error instanceof ApiRequestError && error.error.status === 404) return false;
      return failureCount < 2;
    },
  });
}
