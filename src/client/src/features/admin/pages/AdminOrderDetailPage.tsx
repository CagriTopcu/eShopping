import { useParams, Link } from "@tanstack/react-router";
import { RoleGate } from "@/features/auth/components/RoleGate.tsx";
import { ROLES, ORDER_STATUS } from "@/config/constants.ts";
import { useAdminOrderDetail } from "../api/admin.queries.ts";
import { useAdminCancelOrder } from "../api/admin.mutations.ts";
import { OrderStatusBadge } from "@/features/order/components/OrderStatusBadge.tsx";
import { Button } from "@/shared/components/ui/Button.tsx";
import { Skeleton } from "@/shared/components/ui/Skeleton.tsx";
import { formatCurrency, formatDate } from "@/shared/lib/format.ts";

function AdminOrderDetailContent() {
  const { orderId } = useParams({ from: "/admin/orders/$orderId" });
  const { data: order, isLoading } = useAdminOrderDetail(orderId);
  const cancelOrder = useAdminCancelOrder();

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
        <h2 className="text-xl font-semibold">Siparis bulunamadi</h2>
        <Button asChild variant="outline" className="mt-4">
          <Link to="/admin/orders">Siparislere Don</Link>
        </Button>
      </div>
    );
  }

  const canCancel =
    order.status === ORDER_STATUS.Pending ||
    order.status === ORDER_STATUS.PaymentReserved;

  return (
    <div className="animate-fade-in">
      <nav className="mb-6 flex items-center gap-2 text-sm text-muted-foreground">
        <Link to="/admin/orders" className="hover:text-foreground transition-colors">
          Siparis Yonetimi
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
              Siparis #{order.id.slice(0, 8).toUpperCase()}
            </h1>
            <OrderStatusBadge status={order.status} />
          </div>
          <p className="text-sm text-muted-foreground mt-1">
            {formatDate(order.placedAt)}
          </p>
          <p className="text-xs text-muted-foreground mt-1">
            Musteri: {order.customerId.slice(0, 8)}...
          </p>
        </div>
        {canCancel && (
          <Button
            variant="destructive"
            onClick={() => cancelOrder.mutate(order.id)}
            disabled={cancelOrder.isPending}
          >
            {cancelOrder.isPending ? "Iptal Ediliyor..." : "Siparisi Iptal Et"}
          </Button>
        )}
      </div>

      <div className="grid md:grid-cols-3 gap-6">
        <div className="md:col-span-2">
          <div className="rounded-xl border border-border bg-card overflow-hidden">
            <div className="p-4 border-b border-border bg-muted/30">
              <h2 className="font-semibold">Urunler</h2>
            </div>
            {order.items.map((item) => (
              <div
                key={item.productId}
                className="flex items-center justify-between p-4 border-b border-border last:border-0"
              >
                <div>
                  <p className="font-medium text-sm">{item.productName}</p>
                  <p className="text-xs text-muted-foreground">
                    {formatCurrency(item.unitPrice)} x {item.quantity}
                  </p>
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

        <div className="space-y-4">
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

export default function AdminOrderDetailPage() {
  return (
    <RoleGate role={ROLES.ADMIN}>
      <AdminOrderDetailContent />
    </RoleGate>
  );
}
