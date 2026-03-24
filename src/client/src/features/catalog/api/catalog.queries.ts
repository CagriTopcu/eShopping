import { useQuery } from "@tanstack/react-query";
import { api } from "@/shared/api/client.ts";
import { CACHE_TIMES } from "@/config/constants.ts";
import type {
  ProductResponse,
  ProductFilters,
  PagedResponse,
} from "@/shared/types/common.types.ts";

export const catalogKeys = {
  all: ["catalog"] as const,
  lists: () => [...catalogKeys.all, "list"] as const,
  list: (filters: ProductFilters) =>
    [...catalogKeys.lists(), filters] as const,
  details: () => [...catalogKeys.all, "detail"] as const,
  detail: (id: string) => [...catalogKeys.details(), id] as const,
};

export function useProducts(filters: ProductFilters = {}) {
  return useQuery({
    queryKey: catalogKeys.list(filters),
    queryFn: ({ signal }) =>
      api.get<PagedResponse<ProductResponse>>(
        "/api/v1/catalog/products",
        {
          category: filters.category,
          name: filters.name,
          minPrice: filters.minPrice,
          maxPrice: filters.maxPrice,
          page: filters.page,
          pageSize: filters.pageSize,
        },
        signal,
      ),
    ...CACHE_TIMES.catalog,
  });
}

export function useProduct(id: string) {
  return useQuery({
    queryKey: catalogKeys.detail(id),
    queryFn: ({ signal }) =>
      api.get<ProductResponse>(`/api/v1/catalog/products/${id}`, undefined, signal),
    ...CACHE_TIMES.catalogDetail,
    enabled: !!id,
  });
}
