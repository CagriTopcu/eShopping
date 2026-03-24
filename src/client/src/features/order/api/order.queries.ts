import { useQuery } from "@tanstack/react-query";
import { api } from "@/shared/api/client.ts";
import { CACHE_TIMES, ORDER_STATUS, POLLING_INTERVAL } from "@/config/constants.ts";
import type { OrderResponse } from "@/shared/types/common.types.ts";

export const orderKeys = {
  all: ["orders"] as const,
  lists: () => [...orderKeys.all, "list"] as const,
  detail: (id: string) => [...orderKeys.all, "detail", id] as const,
};

export function useOrders() {
  return useQuery({
    queryKey: orderKeys.lists(),
    queryFn: ({ signal }) =>
      api.get<OrderResponse[]>("/api/v1/orders", undefined, signal),
    ...CACHE_TIMES.orders,
  });
}

export function useOrder(id: string) {
  return useQuery({
    queryKey: orderKeys.detail(id),
    queryFn: ({ signal }) =>
      api.get<OrderResponse>(`/api/v1/orders/${id}`, undefined, signal),
    ...CACHE_TIMES.orders,
    enabled: !!id,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      if (
        status === ORDER_STATUS.Pending ||
        status === ORDER_STATUS.PaymentReserved
      ) {
        return POLLING_INTERVAL;
      }
      return false;
    },
    refetchIntervalInBackground: false,
  });
}
