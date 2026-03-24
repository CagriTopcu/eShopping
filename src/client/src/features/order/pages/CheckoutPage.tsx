import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useNavigate, Link } from "@tanstack/react-router";
import { ProtectedRoute } from "@/features/auth/components/ProtectedRoute.tsx";
import { useBasket } from "@/features/basket/api/basket.queries.ts";
import { usePlaceOrder } from "../api/order.mutations.ts";
import { Button } from "@/shared/components/ui/Button.tsx";
import { Input } from "@/shared/components/ui/Input.tsx";
import { Textarea } from "@/shared/components/ui/Textarea.tsx";
import { EmptyState } from "@/shared/components/ui/EmptyState.tsx";
import { formatCurrency } from "@/shared/lib/format.ts";

const checkoutSchema = z.object({
  street: z.string().min(1, "Adres zorunludur"),
  city: z.string().min(1, "Şehir zorunludur"),
  state: z.string().min(1, "İlçe/Bölge zorunludur"),
  country: z.string().min(1, "Ülke zorunludur"),
  zipCode: z.string().min(1, "Posta kodu zorunludur"),
});

type CheckoutFormData = z.infer<typeof checkoutSchema>;

const STEPS = [
  { label: "Sepet", done: true },
  { label: "Adres", active: true },
  { label: "Onay", done: false },
];

function CheckoutContent() {
  const { data: basket } = useBasket();
  const placeOrder = usePlaceOrder();
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CheckoutFormData>({
    resolver: zodResolver(checkoutSchema) as never,
    defaultValues: { country: "Türkiye" },
  });

  const onSubmit = (data: CheckoutFormData) => {
    if (!basket?.items.length) return;

    placeOrder.mutate(
      {
        ...data,
        items: basket.items.map((item) => ({
          productId: item.productId,
          productName: item.productName,
          unitPrice: item.unitPrice,
          quantity: item.quantity,
        })),
      },
      {
        onSuccess: () => {
          navigate({ to: "/orders" });
        },
      },
    );
  };

  if (!basket?.items.length) {
    return (
      <EmptyState
        title="Sepetiniz boş"
        description="Sipariş oluşturmak için önce sepetinize ürün ekleyin"
        action={
          <Button asChild>
            <Link to="/products">Ürünlere Git</Link>
          </Button>
        }
      />
    );
  }

  return (
    <div className="space-y-8">
      {/* Step indicator */}
      <div className="flex items-center justify-center gap-0">
        {STEPS.map((step, i) => (
          <div key={step.label} className="flex items-center">
            <div className="flex flex-col items-center">
              <div
                className={`h-8 w-8 rounded-full flex items-center justify-center text-xs font-bold ${
                  step.done
                    ? "bg-success text-white"
                    : step.active
                      ? "bg-primary text-primary-foreground"
                      : "bg-muted text-muted-foreground"
                }`}
              >
                {step.done ? "✓" : i + 1}
              </div>
              <span
                className={`text-xs mt-1.5 ${step.active ? "text-primary font-medium" : "text-muted-foreground"}`}
              >
                {step.label}
              </span>
            </div>
            {i < STEPS.length - 1 && (
              <div
                className={`w-16 sm:w-24 h-0.5 mx-2 mb-5 ${
                  step.done ? "bg-success" : "bg-border"
                }`}
              />
            )}
          </div>
        ))}
      </div>

      <div className="grid lg:grid-cols-3 gap-8">
        <form
          onSubmit={handleSubmit(onSubmit)}
          className="lg:col-span-2 space-y-6"
        >
          <div className="rounded-xl border border-border bg-card p-6 space-y-5">
            <h2 className="text-lg font-semibold">Teslimat Adresi</h2>

            <div>
              <label className="text-sm font-medium mb-1.5 block">Adres</label>
              <Textarea
                {...register("street")}
                placeholder="Mahalle, sokak, bina no, daire no..."
                error={errors.street?.message}
                rows={3}
              />
            </div>

            <div className="grid sm:grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium mb-1.5 block">
                  Şehir
                </label>
                <Input
                  {...register("city")}
                  placeholder="İstanbul"
                  error={errors.city?.message}
                />
              </div>
              <div>
                <label className="text-sm font-medium mb-1.5 block">
                  İlçe/Bölge
                </label>
                <Input
                  {...register("state")}
                  placeholder="Kadıköy"
                  error={errors.state?.message}
                />
              </div>
            </div>

            <div className="grid sm:grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium mb-1.5 block">
                  Posta Kodu
                </label>
                <Input
                  {...register("zipCode")}
                  placeholder="34000"
                  error={errors.zipCode?.message}
                />
              </div>
              <div>
                <label className="text-sm font-medium mb-1.5 block">
                  Ülke
                </label>
                <Input
                  {...register("country")}
                  placeholder="Türkiye"
                  error={errors.country?.message}
                />
              </div>
            </div>
          </div>

          <Button
            type="submit"
            size="lg"
            className="w-full shadow-md"
            disabled={placeOrder.isPending}
          >
            {placeOrder.isPending
              ? "Sipariş oluşturuluyor..."
              : "Siparişi Onayla"}
          </Button>
        </form>

        {/* Order summary */}
        <div className="lg:col-span-1">
          <div className="sticky top-24 rounded-xl border border-border bg-card p-6 space-y-4">
            <h2 className="text-lg font-semibold">Sipariş Özeti</h2>
            <div className="space-y-3 max-h-[300px] overflow-y-auto">
              {basket.items.map((item) => (
                <div
                  key={item.productId}
                  className="flex justify-between text-sm gap-2"
                >
                  <span className="text-muted-foreground truncate">
                    {item.productName}{" "}
                    <span className="text-foreground">×{item.quantity}</span>
                  </span>
                  <span className="whitespace-nowrap font-medium">
                    {formatCurrency(item.lineTotal, item.currency)}
                  </span>
                </div>
              ))}
            </div>
            <hr className="border-border" />
            <div className="flex justify-between text-sm text-muted-foreground">
              <span>Kargo</span>
              <span className="text-success font-medium">Ücretsiz</span>
            </div>
            <div className="flex justify-between font-bold text-lg">
              <span>Toplam</span>
              <span className="text-primary">
                {formatCurrency(basket.totalPrice)}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default function CheckoutPage() {
  return (
    <ProtectedRoute>
      <div className="space-y-6 animate-fade-in">
        <h1 className="text-2xl font-bold">Sipariş Oluştur</h1>
        <CheckoutContent />
      </div>
    </ProtectedRoute>
  );
}
