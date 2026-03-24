import { useQuery } from "@tanstack/react-query";
import { api } from "@/shared/api/client.ts";
import { CACHE_TIMES } from "@/config/constants.ts";
import type { BasketResponse } from "@/shared/types/common.types.ts";
import { useAuth } from "@/features/auth/hooks/useAuth.ts";

export const basketKeys = {
  all: ["basket"] as const,
  detail: (username: string) => [...basketKeys.all, username] as const,
};

export function useBasket() {
  const { username, isAuthenticated } = useAuth();

  return useQuery({
    queryKey: basketKeys.detail(username ?? ""),
    queryFn: ({ signal }) =>
      api.get<BasketResponse>(
        `/api/v1/basket/${username}`,
        undefined,
        signal,
      ),
    ...CACHE_TIMES.basket,
    enabled: isAuthenticated && !!username,
  });
}
