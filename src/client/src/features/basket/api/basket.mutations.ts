import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/shared/api/client.ts";
import { basketKeys } from "./basket.queries.ts";
import { toast } from "@/shared/components/feedback/GlobalToast.tsx";
import { useAuth } from "@/features/auth/hooks/useAuth.ts";
import type {
  BasketResponse,
  UpsertBasketItemRequest,
} from "@/shared/types/common.types.ts";

export function useUpsertBasketItem() {
  const queryClient = useQueryClient();
  const { username } = useAuth();

  return useMutation({
    mutationFn: (data: UpsertBasketItemRequest) =>
      api.put<BasketResponse>(`/api/v1/basket/${username}/items`, data),
    onMutate: async () => {
      const key = basketKeys.detail(username ?? "");
      await queryClient.cancelQueries({ queryKey: key });
      const previous = queryClient.getQueryData<BasketResponse>(key);
      return { previous, key };
    },
    onSuccess: (data, _variables, context) => {
      if (context?.key) {
        queryClient.setQueryData(context.key, data);
      }
      toast("Sepet güncellendi", "success");
    },
    onError: (_error, _variables, context) => {
      if (context?.previous && context.key) {
        queryClient.setQueryData(context.key, context.previous);
      }
      toast("Sepet güncellenirken bir hata oluştu", "error");
    },
    onSettled: (_data, _error, _variables, context) => {
      if (context?.key) {
        queryClient.invalidateQueries({ queryKey: context.key });
      }
    },
  });
}

export function useRemoveBasketItem() {
  const queryClient = useQueryClient();
  const { username } = useAuth();

  return useMutation({
    mutationFn: (productId: string) =>
      api.delete<void>(`/api/v1/basket/${username}/items/${productId}`),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: basketKeys.detail(username ?? ""),
      });
      toast("Ürün sepetten çıkarıldı", "success");
    },
    onError: () => {
      toast("Ürün sepetten çıkarılırken bir hata oluştu", "error");
    },
  });
}

export function useClearBasket() {
  const queryClient = useQueryClient();
  const { username } = useAuth();

  return useMutation({
    mutationFn: () => api.delete<void>(`/api/v1/basket/${username}`),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: basketKeys.detail(username ?? ""),
      });
      toast("Sepet temizlendi", "success");
    },
    onError: () => {
      toast("Sepet temizlenirken bir hata oluştu", "error");
    },
  });
}
