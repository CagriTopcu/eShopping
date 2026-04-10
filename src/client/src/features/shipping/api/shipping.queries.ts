import { useQuery } from "@tanstack/react-query";
import { api, ApiRequestError } from "@/shared/api/client.ts";
import type { ShipmentResponse } from "@/shared/types/common.types.ts";

export const shippingKeys = {
  all: ["shipping"] as const,
  byOrder: (orderId: string) => [...shippingKeys.all, orderId] as const,
};

export function useShipment(orderId: string) {
  return useQuery({
    queryKey: shippingKeys.byOrder(orderId),
    queryFn: ({ signal }) =>
      api.get<ShipmentResponse>(`/api/v1/shipping/${orderId}`, undefined, signal),
    enabled: !!orderId,
    staleTime: 30 * 1000,
    gcTime: 5 * 60 * 1000,
    retry: (failureCount, error) => {
      if (error instanceof ApiRequestError && error.error.status === 404) return false;
      return failureCount < 2;
    },
  });
}
