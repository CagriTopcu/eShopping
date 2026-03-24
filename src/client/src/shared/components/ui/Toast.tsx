import * as ToastPrimitive from "@radix-ui/react-toast";
import { cn } from "@/shared/lib/cn.ts";

export type ToastVariant = "default" | "success" | "error" | "warning";

interface ToastProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description?: string;
  variant?: ToastVariant;
  action?: { label: string; onClick: () => void };
}

const variantStyles: Record<ToastVariant, string> = {
  default: "bg-card border-border",
  success: "bg-card border-success",
  error: "bg-card border-destructive",
  warning: "bg-card border-warning",
};

export function Toast({
  open,
  onOpenChange,
  title,
  description,
  variant = "default",
  action,
}: ToastProps) {
  return (
    <ToastPrimitive.Root
      className={cn(
        "rounded-lg border-l-4 p-4 shadow-lg",
        variantStyles[variant],
      )}
      open={open}
      onOpenChange={onOpenChange}
    >
      <ToastPrimitive.Title className="text-sm font-semibold">
        {title}
      </ToastPrimitive.Title>
      {description && (
        <ToastPrimitive.Description className="mt-1 text-sm text-muted-foreground">
          {description}
        </ToastPrimitive.Description>
      )}
      {action && (
        <ToastPrimitive.Action altText={action.label} asChild>
          <button
            onClick={action.onClick}
            className="mt-2 text-sm font-medium text-primary hover:underline"
          >
            {action.label}
          </button>
        </ToastPrimitive.Action>
      )}
    </ToastPrimitive.Root>
  );
}

export function ToastViewport() {
  return (
    <ToastPrimitive.Viewport className="fixed bottom-4 right-4 z-[100] flex max-h-screen w-full max-w-[380px] flex-col gap-2" />
  );
}

export const ToastProvider = ToastPrimitive.Provider;
