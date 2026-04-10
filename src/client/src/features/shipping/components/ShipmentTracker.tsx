import { useShipment } from "../api/shipping.queries.ts";
import { Badge } from "@/shared/components/ui/Badge.tsx";
import { Skeleton } from "@/shared/components/ui/Skeleton.tsx";
import { formatDate } from "@/shared/lib/format.ts";
import { SHIPMENT_STATUS, SHIPMENT_STATUS_LABELS } from "@/config/constants.ts";

const STEPS = [
  { status: SHIPMENT_STATUS.Created, label: "Siparis Alindi" },
  { status: SHIPMENT_STATUS.Processing, label: "Hazirlaniyor" },
  { status: SHIPMENT_STATUS.Shipped, label: "Kargoya Verildi" },
  { status: SHIPMENT_STATUS.InTransit, label: "Yolda" },
  { status: SHIPMENT_STATUS.Delivered, label: "Teslim Edildi" },
] as const;

function statusVariant(status: number): "default" | "secondary" | "success" | "destructive" | "warning" {
  switch (status) {
    case SHIPMENT_STATUS.Delivered: return "success";
    case SHIPMENT_STATUS.Failed: return "destructive";
    case SHIPMENT_STATUS.Shipped:
    case SHIPMENT_STATUS.InTransit: return "warning";
    default: return "secondary";
  }
}

export function ShipmentTracker({ orderId }: { orderId: string }) {
  const { data: shipment, isLoading, isError } = useShipment(orderId);

  if (isLoading) {
    return (
      <div className="rounded-xl border border-border bg-card p-5 space-y-4">
        <Skeleton className="h-5 w-32" />
        <Skeleton className="h-16 w-full" />
      </div>
    );
  }

  if (isError || !shipment) {
    return (
      <div className="rounded-xl border border-border bg-card p-5">
        <h2 className="font-semibold mb-2">Kargo Takip</h2>
        <p className="text-sm text-muted-foreground">Kargo bilgisi henuz olusturulmadi.</p>
      </div>
    );
  }

  const currentStep = STEPS.findIndex((s) => s.status === shipment.status);
  const isFailed = shipment.status === SHIPMENT_STATUS.Failed;

  return (
    <div className="rounded-xl border border-border bg-card p-5 space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="font-semibold">Kargo Takip</h2>
        <Badge variant={statusVariant(shipment.status)}>
          {SHIPMENT_STATUS_LABELS[shipment.status] ?? "Bilinmiyor"}
        </Badge>
      </div>

      {shipment.trackingNumber && (
        <div className="flex items-center gap-2 text-sm">
          <span className="text-muted-foreground">Takip No:</span>
          <span className="font-mono font-medium">{shipment.trackingNumber}</span>
        </div>
      )}

      {/* Timeline */}
      {!isFailed && (
        <div className="relative">
          {STEPS.map((step, i) => {
            const isCompleted = i <= currentStep;
            const isCurrent = i === currentStep;

            return (
              <div key={step.status} className="flex items-start gap-3 relative">
                {/* Vertical line */}
                {i < STEPS.length - 1 && (
                  <div
                    className={`absolute left-[11px] top-6 w-0.5 h-[calc(100%-4px)] ${
                      i < currentStep ? "bg-primary" : "bg-border"
                    }`}
                  />
                )}

                {/* Dot */}
                <div
                  className={`relative z-10 mt-0.5 h-6 w-6 rounded-full border-2 flex items-center justify-center shrink-0 transition-all ${
                    isCurrent
                      ? "border-primary bg-primary"
                      : isCompleted
                        ? "border-primary bg-primary"
                        : "border-border bg-background"
                  }`}
                >
                  {isCompleted && (
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
                      <polyline points="20 6 9 17 4 12" />
                    </svg>
                  )}
                </div>

                {/* Label */}
                <div className={`pb-6 ${isCurrent ? "" : ""}`}>
                  <p className={`text-sm font-medium ${isCompleted ? "text-foreground" : "text-muted-foreground"}`}>
                    {step.label}
                  </p>
                  {isCurrent && step.status === SHIPMENT_STATUS.Shipped && shipment.shippedAt && (
                    <p className="text-xs text-muted-foreground mt-0.5">{formatDate(shipment.shippedAt)}</p>
                  )}
                  {isCurrent && step.status === SHIPMENT_STATUS.Delivered && shipment.deliveredAt && (
                    <p className="text-xs text-muted-foreground mt-0.5">{formatDate(shipment.deliveredAt)}</p>
                  )}
                  {isCurrent && step.status === SHIPMENT_STATUS.Created && (
                    <p className="text-xs text-muted-foreground mt-0.5">{formatDate(shipment.createdAt)}</p>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {isFailed && (
        <div className="rounded-lg bg-destructive/10 p-3 text-sm text-destructive">
          Kargo teslimati basarisiz oldu. Lutfen musteri hizmetleriyle iletisime gecin.
        </div>
      )}
    </div>
  );
}
