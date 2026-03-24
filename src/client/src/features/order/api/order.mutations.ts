import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/shared/api/client.ts";
import { orderKeys } from "./order.queries.ts";
import { basketKeys } from "@/features/basket/api/basket.queries.ts";
import { toast } from "@/shared/components/feedback/GlobalToast.tsx";
import { useAuth } from "@/features/auth/hooks/useAuth.ts";
import type {
  PlaceOrderRequest,
  OrderResponse,
} from "@/shared/types/common.types.ts";

export function usePlaceOrder() {
  const queryClient = useQueryClient();
  const { username } = useAuth();

  return useMutation({
    mutationFn: (data: PlaceOrderRequest) =>
      api.post<OrderResponse>("/api/v1/orders", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: orderKeys.lists() });
      queryClient.invalidateQueries({
        queryKey: basketKeys.detail(username ?? ""),
      });
      toast("Sipariş başarıyla oluşturuldu", "success");
    },
    onError: () => {
      toast("Sipariş oluşturulurken bir hata oluştu", "error");
    },
  });
}

export function useCancelOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (orderId: string) =>
      api.delete<void>(`/api/v1/orders/${orderId}`),
    onSuccess: (_data, orderId) => {
      queryClient.invalidateQueries({ queryKey: orderKeys.detail(orderId) });
      queryClient.invalidateQueries({ queryKey: orderKeys.lists() });
      toast("Sipariş iptal edildi", "success");
    },
    onError: () => {
      toast("Sipariş iptal edilirken bir hata oluştu", "error");
    },
  });
}
