import { useState } from "react";
import { Link } from "@tanstack/react-router";
import { RoleGate } from "@/features/auth/components/RoleGate.tsx";
import { ROLES, ORDER_STATUS } from "@/config/constants.ts";
import { useAdminOrders } from "../api/admin.queries.ts";
import { useAdminCancelOrder } from "../api/admin.mutations.ts";
import { OrderStatusBadge } from "@/features/order/components/OrderStatusBadge.tsx";
import { Button } from "@/shared/components/ui/Button.tsx";
import { Pagination } from "@/shared/components/ui/Pagination.tsx";
import { Dialog } from "@/shared/components/ui/Dialog.tsx";
import { formatCurrency, formatShortDate } from "@/shared/lib/format.ts";

function AdminOrdersContent() {
  const [page, setPage] = useState(1);
  const { data, isLoading } = useAdminOrders(page);
  const cancelOrder = useAdminCancelOrder();
  const [cancelId, setCancelId] = useState<string | null>(null);

  const handleCancel = () => {
    if (cancelId) {
      cancelOrder.mutate(cancelId, {
        onSuccess: () => setCancelId(null),
      });
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Siparis Yonetimi</h1>
      </div>

      <div className="rounded-lg border border-border bg-card overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-muted/50">
                <th className="text-left p-4 font-medium">Siparis</th>
                <th className="text-left p-4 font-medium">Musteri</th>
                <th className="text-left p-4 font-medium">Durum</th>
                <th className="text-right p-4 font-medium">Urunler</th>
                <th className="text-right p-4 font-medium">Toplam</th>
                <th className="text-left p-4 font-medium">Tarih</th>
                <th className="text-right p-4 font-medium">Islemler</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={7} className="p-8 text-center text-muted-foreground">
                    Yukleniyor...
                  </td>
                </tr>
              ) : !data?.items.length ? (
                <tr>
                  <td colSpan={7} className="p-8 text-center text-muted-foreground">
                    Siparis bulunamadi
                  </td>
                </tr>
              ) : (
                data.items.map((order) => (
                  <tr
                    key={order.id}
                    className="border-b border-border last:border-0 hover:bg-muted/30"
                  >
                    <td className="p-4">
                      <Link
                        to="/admin/orders/$orderId"
                        params={{ orderId: order.id }}
                        className="font-medium hover:text-primary transition-colors"
                      >
                        #{order.id.slice(0, 8).toUpperCase()}
                      </Link>
                    </td>
                    <td className="p-4">
                      <span className="text-xs text-muted-foreground">
                        {order.customerId.slice(0, 8)}...
                      </span>
                    </td>
                    <td className="p-4">
                      <OrderStatusBadge status={order.status} />
                    </td>
                    <td className="p-4 text-right">
                      {order.items.length}
                    </td>
                    <td className="p-4 text-right font-medium">
                      {formatCurrency(order.totalAmount)}
                    </td>
                    <td className="p-4 text-muted-foreground">
                      {formatShortDate(order.placedAt)}
                    </td>
                    <td className="p-4 text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button variant="ghost" size="sm" asChild>
                          <Link
                            to="/admin/orders/$orderId"
                            params={{ orderId: order.id }}
                          >
                            Detay
                          </Link>
                        </Button>
                        {(order.status === ORDER_STATUS.Pending ||
                          order.status === ORDER_STATUS.PaymentReserved) && (
                          <Button
                            variant="ghost"
                            size="sm"
                            className="text-destructive"
                            onClick={() => setCancelId(order.id)}
                          >
                            Iptal
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {data && data.totalPages > 1 && (
        <Pagination
          page={data.page}
          totalPages={data.totalPages}
          onPageChange={setPage}
        />
      )}

      <Dialog
        open={!!cancelId}
        onOpenChange={(open) => !open && setCancelId(null)}
        title="Siparisi Iptal Et"
        description="Bu islemi geri alamazsiniz. Siparisi iptal etmek istediginizden emin misiniz?"
      >
        <div className="flex justify-end gap-3 mt-4">
          <Button variant="outline" onClick={() => setCancelId(null)}>
            Vazgec
          </Button>
          <Button
            variant="destructive"
            onClick={handleCancel}
            disabled={cancelOrder.isPending}
          >
            {cancelOrder.isPending ? "Iptal Ediliyor..." : "Iptal Et"}
          </Button>
        </div>
      </Dialog>
    </div>
  );
}

export default function AdminOrdersPage() {
  return (
    <RoleGate role={ROLES.ADMIN}>
      <AdminOrdersContent />
    </RoleGate>
  );
}
