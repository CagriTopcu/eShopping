import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/shared/api/client.ts";
import { catalogKeys } from "./catalog.queries.ts";
import { toast } from "@/shared/components/feedback/GlobalToast.tsx";
import type {
  CreateProductRequest,
  UpdateProductRequest,
} from "@/shared/types/common.types.ts";

export function useCreateProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateProductRequest) =>
      api.post<string>("/api/v1/catalog/products", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: catalogKeys.lists() });
      toast("Ürün başarıyla oluşturuldu", "success");
    },
    onError: () => {
      toast("Ürün oluşturulurken bir hata oluştu", "error");
    },
  });
}

export function useUpdateProduct(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateProductRequest) =>
      api.put<void>(`/api/v1/catalog/products/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: catalogKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: catalogKeys.lists() });
      toast("Ürün başarıyla güncellendi", "success");
    },
    onError: () => {
      toast("Ürün güncellenirken bir hata oluştu", "error");
    },
  });
}

export function useDeleteProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      api.delete<void>(`/api/v1/catalog/products/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: catalogKeys.lists() });
      toast("Ürün başarıyla silindi", "success");
    },
    onError: () => {
      toast("Ürün silinirken bir hata oluştu", "error");
    },
  });
}

