import { Link } from "@tanstack/react-router";
import { Badge } from "@/shared/components/ui/Badge.tsx";
import { Button } from "@/shared/components/ui/Button.tsx";
import { formatCurrency } from "@/shared/lib/format.ts";
import type { ProductResponse } from "@/shared/types/common.types.ts";
import { useAuth } from "@/features/auth/hooks/useAuth.ts";
import { useUpsertBasketItem } from "@/features/basket/api/basket.mutations.ts";

interface ProductCardProps {
  product: ProductResponse;
}

const GRADIENTS = [
  "from-slate-100 to-slate-200",
  "from-blue-50 to-indigo-100",
  "from-emerald-50 to-teal-100",
  "from-amber-50 to-orange-100",
  "from-rose-50 to-pink-100",
  "from-violet-50 to-purple-100",
];

function getGradient(id: string) {
  let hash = 0;
  for (let i = 0; i < id.length; i++) {
    hash = id.charCodeAt(i) + ((hash << 5) - hash);
  }
  return GRADIENTS[Math.abs(hash) % GRADIENTS.length];
}

export function ProductCard({ product }: ProductCardProps) {
  const { isAuthenticated } = useAuth();
  const upsertItem = useUpsertBasketItem();

  const handleAddToBasket = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    upsertItem.mutate({ productId: product.id, quantity: 1 });
  };

  return (
    <Link
      to="/products/$productId"
      params={{ productId: product.id }}
      className="group block rounded-xl border border-border bg-card overflow-hidden transition-all duration-300 hover:shadow-lg hover:shadow-primary/5 hover:-translate-y-1"
    >
      {/* Image */}
      <div
        className={`aspect-[4/3] bg-gradient-to-br ${getGradient(product.id)} flex items-center justify-center relative overflow-hidden`}
      >
        {product.imageUrl ? (
          <img
            src={product.imageUrl}
            alt={product.name}
            className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
            loading="lazy"
          />
        ) : (
          <div className="flex flex-col items-center gap-2 text-muted-foreground/40">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="40"
              height="40"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="1"
            >
              <rect width="18" height="18" x="3" y="3" rx="2" ry="2" />
              <circle cx="9" cy="9" r="2" />
              <path d="m21 15-3.086-3.086a2 2 0 0 0-2.828 0L6 21" />
            </svg>
          </div>
        )}

        {/* Stock indicator */}
        {product.stock <= 0 && (
          <div className="absolute inset-0 bg-black/40 flex items-center justify-center">
            <span className="bg-destructive text-destructive-foreground px-3 py-1 rounded-full text-xs font-semibold">
              Tükendi
            </span>
          </div>
        )}

        {/* Add to basket - hover overlay */}
        {isAuthenticated && product.stock > 0 && (
          <div className="absolute inset-x-0 bottom-0 p-3 translate-y-full group-hover:translate-y-0 transition-transform duration-300">
            <Button
              size="sm"
              className="w-full shadow-lg"
              onClick={handleAddToBasket}
              disabled={upsertItem.isPending}
            >
              Sepete Ekle
            </Button>
          </div>
        )}
      </div>

      {/* Info */}
      <div className="p-4">
        <div className="flex items-start justify-between gap-2">
          <h3 className="font-medium text-sm leading-snug line-clamp-2 group-hover:text-primary transition-colors">
            {product.name}
          </h3>
        </div>

        <div className="flex items-center gap-2 mt-1.5">
          <Badge variant="outline" className="text-[10px] px-1.5 py-0">
            {product.category}
          </Badge>
          {product.stock > 0 && product.stock <= 5 && (
            <span className="text-[10px] text-warning font-medium">
              Son {product.stock} adet
            </span>
          )}
        </div>

        <div className="mt-3 flex items-center justify-between">
          <p className="text-lg font-bold text-primary">
            {formatCurrency(product.price, product.currency)}
          </p>
          {product.stock > 0 && (
            <div className="flex items-center gap-1">
              <span className="h-2 w-2 rounded-full bg-success" />
              <span className="text-xs text-muted-foreground">Stokta</span>
            </div>
          )}
        </div>
      </div>
    </Link>
  );
}
