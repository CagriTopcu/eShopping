import { useState } from "react";
import { Link } from "@tanstack/react-router";
import { ProtectedRoute } from "@/features/auth/components/ProtectedRoute.tsx";
import { useOrders } from "../api/order.queries.ts";
import { OrderStatusBadge } from "../components/OrderStatusBadge.tsx";
import { Button } from "@/shared/components/ui/Button.tsx";
import { Skeleton } from "@/shared/components/ui/Skeleton.tsx";
import { EmptyState } from "@/shared/components/ui/EmptyState.tsx";
import { formatCurrency, formatShortDate } from "@/shared/lib/format.ts";
import { ORDER_STATUS } from "@/config/constants.ts";

type FilterTab = "all" | "active" | "completed" | "cancelled";

function OrdersContent() {
  const { data: orders, isLoading } = useOrders();
  const [activeTab, setActiveTab] = useState<FilterTab>("all");

  const filteredOrders = orders?.filter((order) => {
    switch (activeTab) {
      case "active":
        return (
          order.status === ORDER_STATUS.Pending ||
          order.status === ORDER_STATUS.PaymentReserved
        );
      case "completed":
        return order.status === ORDER_STATUS.Completed;
      case "cancelled":
        return order.status === ORDER_STATUS.Cancelled;
      default:
        return true;
    }
  });

  if (isLoading) {
    return (
      <div className="space-y-4">
        {Array.from({ length: 3 }).map((_, i) => (
          <Skeleton key={i} className="h-36 w-full rounded-xl" />
        ))}
      </div>
    );
  }

  const tabs: { key: FilterTab; label: string }[] = [
    { key: "all", label: "Tümü" },
    { key: "active", label: "Aktif" },
    { key: "completed", label: "Tamamlanan" },
    { key: "cancelled", label: "İptal" },
  ];

  return (
    <div className="space-y-6">
      {/* Filter tabs */}
      <div className="flex gap-1 p-1 bg-muted rounded-lg w-fit">
        {tabs.map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            className={`px-4 py-2 text-sm font-medium rounded-md transition-all ${
              activeTab === tab.key
                ? "bg-background text-foreground shadow-sm"
                : "text-muted-foreground hover:text-foreground"
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {!filteredOrders?.length ? (
        <EmptyState
          title="Sipariş bulunamadı"
          description={
            activeTab === "all"
              ? "Henüz bir sipariş oluşturmadınız"
              : "Bu kategoride sipariş bulunamadı"
          }
          action={
            activeTab === "all" ? (
              <Button asChild>
                <Link to="/products">Ürünlere Git</Link>
              </Button>
            ) : undefined
          }
        />
      ) : (
        <div className="space-y-3">
          {filteredOrders.map((order, i) => (
            <Link
              key={order.id}
              to="/orders/$orderId"
              params={{ orderId: order.id }}
              className="block p-5 rounded-xl border border-border bg-card hover:shadow-md transition-all duration-200 animate-slide-up"
              style={{
                animationDelay: `${i * 50}ms`,
                animationFillMode: "both",
              }}
            >
              <div className="flex items-start justify-between gap-4">
                <div className="min-w-0">
                  <div className="flex items-center gap-3">
                    <p className="font-semibold text-sm">
                      #{order.id.slice(0, 8).toUpperCase()}
                    </p>
                    <OrderStatusBadge status={order.status} />
                  </div>
                  <p className="text-xs text-muted-foreground mt-1">
                    {formatShortDate(order.placedAt)}
                  </p>
                  {/* First few product names */}
                  <p className="text-xs text-muted-foreground mt-2 line-clamp-1">
                    {order.items
                      .map((item) => item.productName)
                      .join(", ")}
                  </p>
                </div>
                <div className="text-right shrink-0">
                  <p className="font-bold text-primary">
                    {formatCurrency(order.totalAmount)}
                  </p>
                  <p className="text-xs text-muted-foreground mt-0.5">
                    {order.items.length} ürün
                  </p>
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}

export default function OrdersPage() {
  return (
    <ProtectedRoute>
      <div className="space-y-6 animate-fade-in">
        <h1 className="text-2xl font-bold">Siparişlerim</h1>
        <OrdersContent />
      </div>
    </ProtectedRoute>
  );
}
