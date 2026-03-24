import { Badge } from "@/shared/components/ui/Badge.tsx";
import { ORDER_STATUS, ORDER_STATUS_LABELS } from "@/config/constants.ts";

interface OrderStatusBadgeProps {
  status: number;
}

function getVariant(
  status: number,
): "default" | "success" | "destructive" | "warning" | "secondary" {
  switch (status) {
    case ORDER_STATUS.Pending:
      return "warning";
    case ORDER_STATUS.PaymentReserved:
      return "secondary";
    case ORDER_STATUS.Completed:
      return "success";
    case ORDER_STATUS.Cancelled:
      return "destructive";
    default:
      return "default";
  }
}

export function OrderStatusBadge({ status }: OrderStatusBadgeProps) {
  return (
    <Badge variant={getVariant(status)}>
      {ORDER_STATUS_LABELS[status] ?? "Bilinmeyen"}
    </Badge>
  );
}
