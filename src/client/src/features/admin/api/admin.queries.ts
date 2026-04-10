import { useQuery } from "@tanstack/react-query";
import { api } from "@/shared/api/client.ts";
import type {
  PagedResponse,
  OrderResponse,
  CatalogStatsResponse,
  OrderStatsResponse,
  StockResponse,
  ShipmentResponse,
} from "@/shared/types/common.types.ts";
import { CACHE_TIMES } from "@/config/constants.ts";

export const adminKeys = {
  all: ["admin"] as const,
  orders: () => [...adminKeys.all, "orders"] as const,
  orderList: (page: number) => [...adminKeys.orders(), "list", page] as const,
  orderDetail: (id: string) => [...adminKeys.orders(), "detail", id] as const,
  catalogStats: () => [...adminKeys.all, "catalog-stats"] as const,
  orderStats: () => [...adminKeys.all, "order-stats"] as const,
  stock: (productId: string) => [...adminKeys.all, "stock", productId] as const,
  shipments: () => [...adminKeys.all, "shipments"] as const,
  shipmentList: (page: number, status?: number) =>
    [...adminKeys.shipments(), "list", page, status] as const,
};

export function useAdminOrders(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: adminKeys.orderList(page),
    queryFn: ({ signal }) =>
      api.get<PagedResponse<OrderResponse>>("/api/v1/orders/admin", { page, pageSize }, signal),
    ...CACHE_TIMES.orders,
  });
}

export function useAdminOrderDetail(orderId: string) {
  return useQuery({
    queryKey: adminKeys.orderDetail(orderId),
    queryFn: ({ signal }) =>
      api.get<OrderResponse>(`/api/v1/orders/${orderId}`, undefined, signal),
    enabled: !!orderId,
    ...CACHE_TIMES.orders,
  });
}

export function useCatalogStats() {
  return useQuery({
    queryKey: adminKeys.catalogStats(),
    queryFn: ({ signal }) =>
      api.get<CatalogStatsResponse>("/api/v1/catalog/stats", undefined, signal),
    staleTime: 60 * 1000,
    gcTime: 5 * 60 * 1000,
  });
}

export function useOrderStats() {
  return useQuery({
    queryKey: adminKeys.orderStats(),
    queryFn: ({ signal }) =>
      api.get<OrderStatsResponse>("/api/v1/orders/stats", undefined, signal),
    staleTime: 60 * 1000,
    gcTime: 5 * 60 * 1000,
  });
}

export function useStockByProduct(productId: string) {
  return useQuery({
    queryKey: adminKeys.stock(productId),
    queryFn: ({ signal }) =>
      api.get<StockResponse>(`/api/v1/stock/${productId}`, undefined, signal),
    enabled: !!productId,
    staleTime: 30 * 1000,
    gcTime: 2 * 60 * 1000,
  });
}

export function useAdminShipments(page = 1, pageSize = 20, status?: number) {
  return useQuery({
    queryKey: adminKeys.shipmentList(page, status),
    queryFn: ({ signal }) =>
      api.get<PagedResponse<ShipmentResponse>>("/api/v1/shipping", {
        page,
        pageSize,
        ...(status !== undefined && { status }),
      }, signal),
    staleTime: 30 * 1000,
    gcTime: 5 * 60 * 1000,
  });
}
