import { Skeleton } from "@/shared/components/ui/Skeleton.tsx";
import { EmptyState } from "@/shared/components/ui/EmptyState.tsx";
import { ProductCard } from "./ProductCard.tsx";
import type { ProductResponse } from "@/shared/types/common.types.ts";

interface ProductGridProps {
  products?: ProductResponse[];
  isLoading: boolean;
}

export function ProductGrid({ products, isLoading }: ProductGridProps) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {Array.from({ length: 8 }).map((_, i) => (
          <div
            key={i}
            className="rounded-xl border border-border overflow-hidden animate-pulse"
          >
            <Skeleton className="aspect-[4/3]" />
            <div className="p-4 space-y-3">
              <Skeleton className="h-4 w-3/4" />
              <Skeleton className="h-3 w-1/4 rounded-full" />
              <div className="flex justify-between items-center pt-1">
                <Skeleton className="h-6 w-1/3" />
                <Skeleton className="h-3 w-12" />
              </div>
            </div>
          </div>
        ))}
      </div>
    );
  }

  if (!products?.length) {
    return (
      <EmptyState
        title="Ürün bulunamadı"
        description="Arama kriterlerinize uygun ürün bulunamadı. Filtreleri değiştirmeyi deneyin."
      />
    );
  }

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      {products.map((product, i) => (
        <div
          key={product.id}
          className="animate-slide-up"
          style={{ animationDelay: `${i * 50}ms`, animationFillMode: "both" }}
        >
          <ProductCard product={product} />
        </div>
      ))}
    </div>
  );
}
