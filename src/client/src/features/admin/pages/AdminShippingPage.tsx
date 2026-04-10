import { useState } from "react";
import { RoleGate } from "@/features/auth/components/RoleGate.tsx";
import { ROLES, SHIPMENT_STATUS, SHIPMENT_STATUS_LABELS } from "@/config/constants.ts";
import { useAdminShipments } from "../api/admin.queries.ts";
import { useUpdateShipmentStatus } from "../api/admin.mutations.ts";
import { Button } from "@/shared/components/ui/Button.tsx";
import { Input } from "@/shared/components/ui/Input.tsx";
import { Badge } from "@/shared/components/ui/Badge.tsx";
import { Skeleton } from "@/shared/components/ui/Skeleton.tsx";
import { Pagination } from "@/shared/components/ui/Pagination.tsx";
import { Dialog } from "@/shared/components/ui/Dialog.tsx";
import { formatCurrency, formatDate } from "@/shared/lib/format.ts";
import type { ShipmentResponse } from "@/shared/types/common.types.ts";

// ── Shipment action enum (mirrors backend ShipmentAction) ──

const SHIPMENT_ACTION = {
  Ship: 1,
  Deliver: 2,
  Fail: 3,
} as const;

// ── Status badge variant mapping ───────────────────────────

function statusBadgeVariant(status: number) {
  switch (status) {
    case SHIPMENT_STATUS.Delivered:
      return "success" as const;
    case SHIPMENT_STATUS.Shipped:
    case SHIPMENT_STATUS.InTransit:
      return "warning" as const;
    case SHIPMENT_STATUS.Failed:
      return "destructive" as const;
    default:
      return "secondary" as const;
  }
}

// ── Available actions based on current status ──────────────

function getAvailableActions(status: number) {
  const actions: { label: string; action: number; needsTracking: boolean }[] = [];

  if (status === SHIPMENT_STATUS.Created || status === SHIPMENT_STATUS.Processing) {
    actions.push({ label: "Kargoya Ver", action: SHIPMENT_ACTION.Ship, needsTracking: true });
  }
  if (status === SHIPMENT_STATUS.Shipped || status === SHIPMENT_STATUS.InTransit) {
    actions.push({ label: "Teslim Edildi", action: SHIPMENT_ACTION.Deliver, needsTracking: false });
  }
  if (status !== SHIPMENT_STATUS.Delivered) {
    actions.push({ label: "Basarisiz", action: SHIPMENT_ACTION.Fail, needsTracking: false });
  }

  return actions;
}

// ── Update status dialog ───────────────────────────────────

function UpdateStatusDialog({
  shipment,
  open,
  onOpenChange,
}: {
  shipment: ShipmentResponse;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const updateStatus = useUpdateShipmentStatus();
  const [selectedAction, setSelectedAction] = useState<number | null>(null);
  const [trackingNumber, setTrackingNumber] = useState("");

  const actions = getAvailableActions(shipment.status);
  const currentAction = actions.find((a) => a.action === selectedAction);

  const handleSubmit = () => {
    if (selectedAction === null) return;
    updateStatus.mutate(
      {
        shipmentId: shipment.id,
        action: selectedAction,
        trackingNumber: currentAction?.needsTracking ? trackingNumber : undefined,
      },
      {
        onSuccess: () => {
          onOpenChange(false);
          setSelectedAction(null);
          setTrackingNumber("");
        },
      },
    );
  };

  return (
    <Dialog
      open={open}
      onOpenChange={onOpenChange}
      title="Kargo Durumu Guncelle"
      description={`Siparis: ${shipment.orderId.slice(0, 8)}...`}
    >
      <div className="space-y-4 mt-2">
        {/* Current status */}
        <div className="flex items-center justify-between p-3 rounded-lg bg-muted/50">
          <span className="text-sm text-muted-foreground">Mevcut Durum</span>
          <Badge variant={statusBadgeVariant(shipment.status)}>
            {SHIPMENT_STATUS_LABELS[shipment.status]}
          </Badge>
        </div>

        {/* Action buttons */}
        <div>
          <label className="text-sm font-medium mb-2 block">Yeni Durum</label>
          <div className="flex flex-wrap gap-2">
            {actions.map((a) => (
              <button
                key={a.action}
                type="button"
                onClick={() => setSelectedAction(a.action)}
                className={`px-4 py-2 text-sm rounded-md border transition-all ${
                  selectedAction === a.action
                    ? "border-primary bg-primary/10 text-primary font-medium"
                    : "border-border hover:border-primary/50"
                }`}
              >
                {a.label}
              </button>
            ))}
          </div>
          {actions.length === 0 && (
            <p className="text-sm text-muted-foreground">
              Bu kargo icin yapilabilecek islem bulunmuyor.
            </p>
          )}
        </div>

        {/* Tracking number input */}
        {currentAction?.needsTracking && (
          <div>
            <label className="text-sm font-medium mb-1 block">Takip Numarasi</label>
            <Input
              placeholder="TR123456789"
              maxLength={100}
              value={trackingNumber}
              onChange={(e) => setTrackingNumber(e.target.value)}
            />
          </div>
        )}

        {/* Actions */}
        <div className="flex justify-end gap-3 pt-2">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Vazgec
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={
              selectedAction === null ||
              updateStatus.isPending ||
              (currentAction?.needsTracking && !trackingNumber.trim())
            }
          >
            {updateStatus.isPending ? "Guncelleniyor..." : "Guncelle"}
          </Button>
        </div>
      </div>
    </Dialog>
  );
}

// ── Status filter tabs ─────────────────────────────────────

const STATUS_FILTERS = [
  { label: "Tumu", value: undefined },
  { label: "Olusturuldu", value: SHIPMENT_STATUS.Created },
  { label: "Hazirlaniyor", value: SHIPMENT_STATUS.Processing },
  { label: "Kargoda", value: SHIPMENT_STATUS.Shipped },
  { label: "Yolda", value: SHIPMENT_STATUS.InTransit },
  { label: "Teslim Edildi", value: SHIPMENT_STATUS.Delivered },
  { label: "Basarisiz", value: SHIPMENT_STATUS.Failed },
] as const;

// ── Main page ──────────────────────────────────────────────

function AdminShippingContent() {
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<number | undefined>(undefined);
  const { data, isLoading } = useAdminShipments(page, 20, statusFilter);
  const [editShipment, setEditShipment] = useState<ShipmentResponse | null>(null);

  return (
    <div className="space-y-6 animate-fade-in">
      <div>
        <h1 className="text-2xl font-bold">Kargo Yonetimi</h1>
        <p className="text-sm text-muted-foreground mt-1">
          Tum kargolari goruntuleyin ve durum guncellemesi yapin
        </p>
      </div>

      {/* Status filter */}
      <div className="flex flex-wrap gap-2">
        {STATUS_FILTERS.map((f) => (
          <button
            key={f.label}
            type="button"
            onClick={() => {
              setStatusFilter(f.value);
              setPage(1);
            }}
            className={`px-3 py-1.5 text-sm rounded-full border transition-all ${
              statusFilter === f.value
                ? "border-primary bg-primary/10 text-primary font-medium"
                : "border-border hover:border-primary/50 text-muted-foreground"
            }`}
          >
            {f.label}
          </button>
        ))}
      </div>

      {/* Table */}
      <div className="rounded-xl border border-border bg-card overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-muted/50">
                <th className="text-left p-4 font-medium">Kargo ID</th>
                <th className="text-left p-4 font-medium">Siparis ID</th>
                <th className="text-left p-4 font-medium">Durum</th>
                <th className="text-left p-4 font-medium">Takip No</th>
                <th className="text-right p-4 font-medium">Tutar</th>
                <th className="text-left p-4 font-medium">Tarih</th>
                <th className="text-right p-4 font-medium w-32">Islem</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                Array.from({ length: 8 }).map((_, i) => (
                  <tr key={i} className="border-b border-border">
                    <td className="p-4"><Skeleton className="h-5 w-20" /></td>
                    <td className="p-4"><Skeleton className="h-5 w-20" /></td>
                    <td className="p-4"><Skeleton className="h-5 w-24" /></td>
                    <td className="p-4"><Skeleton className="h-5 w-28" /></td>
                    <td className="p-4"><Skeleton className="h-5 w-16 ml-auto" /></td>
                    <td className="p-4"><Skeleton className="h-5 w-32" /></td>
                    <td className="p-4"><Skeleton className="h-8 w-20 ml-auto" /></td>
                  </tr>
                ))
              ) : !data?.items.length ? (
                <tr>
                  <td colSpan={7} className="p-12 text-center text-muted-foreground">
                    Kargo bulunamadi
                  </td>
                </tr>
              ) : (
                data.items.map((shipment) => (
                  <tr
                    key={shipment.id}
                    className="border-b border-border last:border-0 hover:bg-muted/30 transition-colors"
                  >
                    <td className="p-4">
                      <span className="font-mono text-xs">{shipment.id.slice(0, 8)}...</span>
                    </td>
                    <td className="p-4">
                      <span className="font-mono text-xs">{shipment.orderId.slice(0, 8)}...</span>
                    </td>
                    <td className="p-4">
                      <Badge variant={statusBadgeVariant(shipment.status)}>
                        {SHIPMENT_STATUS_LABELS[shipment.status]}
                      </Badge>
                    </td>
                    <td className="p-4">
                      {shipment.trackingNumber ? (
                        <span className="font-mono text-xs">{shipment.trackingNumber}</span>
                      ) : (
                        <span className="text-muted-foreground">—</span>
                      )}
                    </td>
                    <td className="p-4 text-right tabular-nums">
                      {formatCurrency(shipment.orderTotal)}
                    </td>
                    <td className="p-4 text-xs text-muted-foreground">
                      {formatDate(shipment.createdAt)}
                    </td>
                    <td className="p-4 text-right">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setEditShipment(shipment)}
                        disabled={shipment.status === SHIPMENT_STATUS.Delivered}
                      >
                        Guncelle
                      </Button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {data && data.totalPages > 1 && (
        <Pagination page={data.page} totalPages={data.totalPages} onPageChange={setPage} />
      )}

      {/* Update status dialog */}
      {editShipment && (
        <UpdateStatusDialog
          shipment={editShipment}
          open={!!editShipment}
          onOpenChange={(open) => !open && setEditShipment(null)}
        />
      )}
    </div>
  );
}

export default function AdminShippingPage() {
  return (
    <RoleGate role={ROLES.ADMIN}>
      <AdminShippingContent />
    </RoleGate>
  );
}
