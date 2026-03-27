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
