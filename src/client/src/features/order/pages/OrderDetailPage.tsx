import { useParams, Link } from "@tanstack/react-router";
import { ProtectedRoute } from "@/features/auth/components/ProtectedRoute.tsx";
import { useOrder } from "../api/order.queries.ts";
import { useCancelOrder } from "../api/order.mutations.ts";
import { OrderStatusBadge } from "../components/OrderStatusBadge.tsx";
import { ShipmentTracker } from "@/features/shipping/components/ShipmentTracker.tsx";
import { Button } from "@/shared/components/ui/Button.tsx";
import { Skeleton } from "@/shared/components/ui/Skeleton.tsx";
import { formatCurrency, formatDate } from "@/shared/lib/format.ts";
import { ORDER_STATUS } from "@/config/constants.ts";

function OrderDetailContent() {
  const { orderId } = useParams({ from: "/orders/$orderId" });
  const { data: order, isLoading } = useOrder(orderId);
  const cancelOrder = useCancelOrder();

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-1/3" />
        <Skeleton className="h-48 w-full rounded-xl" />
        <Skeleton className="h-32 w-full rounded-xl" />
      </div>
    );
  }

  if (!order) {
    return (
      <div className="text-center py-16">
        <h2 className="text-xl font-semibold">Sipariş bulunamadı</h2>
        <Button asChild variant="outline" className="mt-4">
          <Link to="/orders">Siparişlere Dön</Link>
        </Button>
      </div>
    );
  }

  const canCancel =
    order.status === ORDER_STATUS.Pending ||
    order.status === ORDER_STATUS.PaymentReserved;

  return (
    <div className="animate-fade-in">
      {/* Breadcrumb */}
      <nav className="mb-6 flex items-center gap-2 text-sm text-muted-foreground">
        <Link to="/orders" className="hover:text-foreground transition-colors">
          Siparişlerim
        </Link>
        <span>/</span>
        <span className="text-foreground font-medium">
          #{order.id.slice(0, 8).toUpperCase()}
        </span>
      </nav>

      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold">
              Sipariş #{order.id.slice(0, 8).toUpperCase()}
            </h1>
            <OrderStatusBadge status={order.status} />
          </div>
          <p className="text-sm text-muted-foreground mt-1">
            {formatDate(order.placedAt)}
          </p>
        </div>
        {canCancel && (
          <Button
            variant="destructive"
            onClick={() => cancelOrder.mutate(order.id)}
            disabled={cancelOrder.isPending}
          >
            {cancelOrder.isPending ? "İptal Ediliyor..." : "Siparişi İptal Et"}
          </Button>
        )}
      </div>

      <div className="grid md:grid-cols-3 gap-6">
        {/* Items */}
        <div className="md:col-span-2">
          <div className="rounded-xl border border-border bg-card overflow-hidden">
            <div className="p-4 border-b border-border bg-muted/30">
              <h2 className="font-semibold">Ürünler</h2>
            </div>
            {order.items.map((item) => (
              <div
                key={item.productId}
                className="flex items-center justify-between p-4 border-b border-border last:border-0"
              >
                <div className="flex items-center gap-3">
                  <div className="h-12 w-12 rounded-lg bg-muted flex items-center justify-center shrink-0">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1"
                      className="text-muted-foreground/40"
                    >
                      <rect
                        width="18"
                        height="18"
                        x="3"
                        y="3"
                        rx="2"
                        ry="2"
                      />
                    </svg>
                  </div>
                  <div>
                    <p className="font-medium text-sm">{item.productName}</p>
                    <p className="text-xs text-muted-foreground">
                      {formatCurrency(item.unitPrice)} × {item.quantity}
                    </p>
                  </div>
                </div>
                <p className="font-semibold text-sm">
                  {formatCurrency(item.lineTotal)}
                </p>
              </div>
            ))}
            <div className="flex items-center justify-between p-4 bg-primary/5">
              <span className="font-semibold">Toplam</span>
              <span className="text-xl font-bold text-primary">
                {formatCurrency(order.totalAmount)}
              </span>
            </div>
          </div>
        </div>

        {/* Sidebar */}
        <div className="space-y-4">
          {/* Shipping tracker */}
          {(order.status === ORDER_STATUS.Completed || order.status === ORDER_STATUS.PaymentReserved) && (
            <ShipmentTracker orderId={order.id} />
          )}

          <div className="rounded-xl border border-border bg-card p-5">
            <h2 className="font-semibold mb-3">Teslimat Adresi</h2>
            <address className="text-sm text-muted-foreground not-italic leading-relaxed">
              {order.shippingStreet}
              <br />
              {order.shippingCity}, {order.shippingState}
              <br />
              {order.shippingZipCode}
              <br />
              {order.shippingCountry}
            </address>
          </div>
        </div>
      </div>
    </div>
  );
}

export default function OrderDetailPage() {
  return (
    <ProtectedRoute>
      <OrderDetailContent />
    </ProtectedRoute>
  );
}
