import { Link } from "@tanstack/react-router";
import { ProtectedRoute } from "@/features/auth/components/ProtectedRoute.tsx";
import { useBasket } from "../api/basket.queries.ts";
import {
  useUpsertBasketItem,
  useRemoveBasketItem,
  useClearBasket,
} from "../api/basket.mutations.ts";
import { Button } from "@/shared/components/ui/Button.tsx";
import { Skeleton } from "@/shared/components/ui/Skeleton.tsx";
import { EmptyState } from "@/shared/components/ui/EmptyState.tsx";
import { formatCurrency } from "@/shared/lib/format.ts";

function BasketContent() {
  const { data: basket, isLoading } = useBasket();
  const upsertItem = useUpsertBasketItem();
  const removeItem = useRemoveBasketItem();
  const clearBasket = useClearBasket();

  if (isLoading) {
    return (
      <div className="space-y-4">
        {Array.from({ length: 3 }).map((_, i) => (
          <Skeleton key={i} className="h-28 w-full rounded-xl" />
        ))}
      </div>
    );
  }

  if (!basket?.items.length) {
    return (
      <EmptyState
        icon={
          <div className="h-20 w-20 rounded-full bg-primary/10 flex items-center justify-center mb-4">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="32"
              height="32"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="1.5"
              className="text-primary"
            >
              <circle cx="8" cy="21" r="1" />
              <circle cx="19" cy="21" r="1" />
              <path d="M2.05 2.05h2l2.66 12.42a2 2 0 0 0 2 1.58h9.78a2 2 0 0 0 1.95-1.57l1.65-7.43H5.12" />
            </svg>
          </div>
        }
        title="Sepetiniz boş"
        description="Ürünleri keşfederek sepetinize ekleyebilirsiniz"
        action={
          <Button asChild>
            <Link to="/products">Ürünlere Git</Link>
          </Button>
        }
      />
    );
  }

  return (
    <div className="grid lg:grid-cols-3 gap-8">
      <div className="lg:col-span-2 space-y-3">
        {basket.items.map((item) => (
          <div
            key={item.productId}
            className="flex items-center gap-4 p-4 rounded-xl border border-border bg-card transition-all hover:shadow-sm"
          >
            {/* Product thumbnail */}
            <Link
              to="/products/$productId"
              params={{ productId: item.productId }}
              className="h-20 w-20 bg-gradient-to-br from-muted to-accent rounded-lg flex items-center justify-center shrink-0 hover:opacity-80 transition-opacity"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="1"
                className="text-muted-foreground/30"
              >
                <rect width="18" height="18" x="3" y="3" rx="2" ry="2" />
              </svg>
            </Link>

            {/* Info */}
            <div className="flex-1 min-w-0">
              <Link
                to="/products/$productId"
                params={{ productId: item.productId }}
                className="font-medium text-sm hover:text-primary transition-colors line-clamp-1"
              >
                {item.productName}
              </Link>
              <p className="text-sm text-muted-foreground mt-0.5">
                {formatCurrency(item.unitPrice, item.currency)} / adet
              </p>

              {/* Quantity controls */}
              <div className="flex items-center gap-2 mt-2">
                <div className="flex items-center border border-border rounded-md">
                  <button
                    onClick={() =>
                      item.quantity > 1
                        ? upsertItem.mutate({
                            productId: item.productId,
                            quantity: item.quantity - 1,
                          })
                        : removeItem.mutate(item.productId)
                    }
                    className="px-2 py-1 text-sm hover:bg-accent transition-colors rounded-l-md"
                  >
                    −
                  </button>
                  <span className="px-3 py-1 text-xs font-semibold border-x border-border min-w-[32px] text-center">
                    {item.quantity}
                  </span>
                  <button
                    onClick={() =>
                      upsertItem.mutate({
                        productId: item.productId,
                        quantity: item.quantity + 1,
                      })
                    }
                    className="px-2 py-1 text-sm hover:bg-accent transition-colors rounded-r-md"
                  >
                    +
                  </button>
                </div>
                <button
                  onClick={() => removeItem.mutate(item.productId)}
                  disabled={removeItem.isPending}
                  className="text-xs text-destructive hover:text-destructive/80 transition-colors ml-1"
                >
                  Kaldır
                </button>
              </div>
            </div>

            {/* Line total */}
            <p className="font-semibold whitespace-nowrap text-sm">
              {formatCurrency(item.lineTotal, item.currency)}
            </p>
          </div>
        ))}
      </div>

      {/* Summary */}
      <div className="lg:col-span-1">
        <div className="sticky top-24 p-6 rounded-xl border border-border bg-card shadow-sm space-y-4">
          <h2 className="text-lg font-semibold">Sipariş Özeti</h2>

          <div className="space-y-2 text-sm">
            <div className="flex justify-between text-muted-foreground">
              <span>Ürünler ({basket.items.length})</span>
              <span>{formatCurrency(basket.totalPrice)}</span>
            </div>
            <div className="flex justify-between text-muted-foreground">
              <span>Kargo</span>
              <span className="text-success font-medium">Ücretsiz</span>
            </div>
          </div>

          <hr className="border-border" />

          <div className="flex justify-between font-bold text-lg">
            <span>Toplam</span>
            <span className="text-primary">
              {formatCurrency(basket.totalPrice)}
            </span>
          </div>

          <Button asChild className="w-full shadow-md" size="lg">
            <Link to="/checkout">Siparişi Tamamla</Link>
          </Button>

          <Button
            variant="ghost"
            size="sm"
            className="w-full text-muted-foreground"
            onClick={() => clearBasket.mutate()}
            disabled={clearBasket.isPending}
          >
            Sepeti Temizle
          </Button>
        </div>
      </div>
    </div>
  );
}

export default function BasketPage() {
  return (
    <ProtectedRoute>
      <div className="space-y-6 animate-fade-in">
        <h1 className="text-2xl font-bold">Sepetim</h1>
        <BasketContent />
      </div>
    </ProtectedRoute>
  );
}
