import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/shared/api/client.ts";
import { adminKeys } from "./admin.queries.ts";
import { toast } from "@/shared/components/feedback/GlobalToast.tsx";

export function useAdminCancelOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (orderId: string) =>
      api.delete<void>(`/api/v1/orders/admin/${orderId}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.orders() });
      queryClient.invalidateQueries({ queryKey: adminKeys.orderStats() });
      toast("Siparis basariyla iptal edildi", "success");
    },
    onError: () => {
      toast("Siparis iptal edilirken bir hata olustu", "error");
    },
  });
}

export function useSetStock(productId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (quantity: number) =>
      api.put<void>(`/api/v1/stock/${productId}`, { availableQuantity: quantity }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.stock(productId) });
      toast("Stok basariyla guncellendi", "success");
    },
    onError: () => {
      toast("Stok guncellenirken bir hata olustu", "error");
    },
  });
}

export function useUpdateShipmentStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      shipmentId,
      action,
      trackingNumber,
    }: {
      shipmentId: string;
      action: number;
      trackingNumber?: string;
    }) =>
      api.patch<void>(`/api/v1/shipping/${shipmentId}/status`, {
        action,
        trackingNumber,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.shipments() });
      toast("Kargo durumu basariyla guncellendi", "success");
    },
    onError: () => {
      toast("Kargo durumu guncellenirken bir hata olustu", "error");
    },
  });
}
