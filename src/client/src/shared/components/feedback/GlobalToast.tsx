import { Toast, ToastProvider, ToastViewport, type ToastVariant } from "../ui/Toast.tsx";
import { create } from "zustand";

interface ToastItem {
  id: string;
  title: string;
  description?: string;
  variant: ToastVariant;
  action?: { label: string; onClick: () => void };
}

interface ToastStore {
  toasts: ToastItem[];
  addToast: (toast: Omit<ToastItem, "id">) => void;
  removeToast: (id: string) => void;
}

export const useToastStore = create<ToastStore>((set) => ({
  toasts: [],
  addToast: (toast) =>
    set((state) => ({
      toasts: [
        ...state.toasts,
        { ...toast, id: crypto.randomUUID() },
      ],
    })),
  removeToast: (id) =>
    set((state) => ({
      toasts: state.toasts.filter((t) => t.id !== id),
    })),
}));

export function toast(title: string, variant: ToastVariant = "default", description?: string) {
  useToastStore.getState().addToast({ title, description, variant });
}

export function GlobalToast() {
  const { toasts, removeToast } = useToastStore();

  return (
    <ToastProvider duration={4000}>
      {toasts.map((t) => (
        <Toast
          key={t.id}
          open={true}
          onOpenChange={(open) => {
            if (!open) removeToast(t.id);
          }}
          title={t.title}
          description={t.description}
          variant={t.variant}
          action={t.action}
        />
      ))}
      <ToastViewport />
    </ToastProvider>
  );
}
