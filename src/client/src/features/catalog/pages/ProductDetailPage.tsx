import { useState } from "react";
import { useParams, Link } from "@tanstack/react-router";
import { useProduct, useProducts } from "../api/catalog.queries.ts";
import { useUpsertBasketItem } from "@/features/basket/api/basket.mutations.ts";
import { useAuth } from "@/features/auth/hooks/useAuth.ts";
import { ProductCard } from "../components/ProductCard.tsx";
import { Button } from "@/shared/components/ui/Button.tsx";
import { Badge } from "@/shared/components/ui/Badge.tsx";
import { Skeleton } from "@/shared/components/ui/Skeleton.tsx";
import { formatCurrency, formatDate } from "@/shared/lib/format.ts";

export default function ProductDetailPage() {
  const { productId } = useParams({ from: "/products/$productId" });
  const { data: product, isLoading, error } = useProduct(productId);
  const { isAuthenticated } = useAuth();
  const upsertItem = useUpsertBasketItem();
  const [quantity, setQuantity] = useState(1);

  // Related products from same category
  const { data: related } = useProducts({
    category: product?.category,
    pageSize: 4,
    page: 1,
  });
  const relatedProducts =
    related?.items.filter((p) => p.id !== productId).slice(0, 4) ?? [];

  if (isLoading) {
    return (
      <div className="animate-fade-in">
        <Skeleton className="h-5 w-48 mb-6" />
        <div className="grid md:grid-cols-2 gap-10">
          <Skeleton className="aspect-square rounded-2xl" />
          <div className="space-y-4">
            <Skeleton className="h-5 w-20" />
            <Skeleton className="h-9 w-3/4" />
            <Skeleton className="h-10 w-1/3" />
            <Skeleton className="h-24 w-full" />
            <Skeleton className="h-12 w-40" />
          </div>
        </div>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="text-center py-16 animate-fade-in">
        <h2 className="text-xl font-semibold">Ürün bulunamadı</h2>
        <Button asChild variant="outline" className="mt-4">
          <Link to="/products">Ürünlere Dön</Link>
        </Button>
      </div>
    );
  }

  return (
    <div className="animate-fade-in">
      {/* Breadcrumb */}
      <nav className="mb-6 flex items-center gap-2 text-sm text-muted-foreground">
        <Link to="/" className="hover:text-foreground transition-colors">
          Ana Sayfa
        </Link>
        <span>/</span>
        <Link
          to="/products"
          className="hover:text-foreground transition-colors"
        >
          Ürünler
        </Link>
        <span>/</span>
        <span className="text-foreground font-medium truncate max-w-[200px]">
          {product.name}
        </span>
      </nav>

      <div className="grid md:grid-cols-2 gap-10">
        {/* Image */}
        <div className="aspect-square bg-gradient-to-br from-muted to-accent rounded-2xl flex items-center justify-center overflow-hidden shadow-sm">
          {product.imageUrl ? (
            <img
              src={product.imageUrl}
              alt={product.name}
              className="w-full h-full object-cover"
            />
          ) : (
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="80"
              height="80"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="0.5"
              className="text-muted-foreground/20"
            >
              <rect width="18" height="18" x="3" y="3" rx="2" ry="2" />
              <circle cx="9" cy="9" r="2" />
              <path d="m21 15-3.086-3.086a2 2 0 0 0-2.828 0L6 21" />
            </svg>
          )}
        </div>

        {/* Info */}
        <div className="flex flex-col">
          <Badge variant="outline" className="w-fit">
            {product.category}
          </Badge>
          <h1 className="mt-3 text-2xl sm:text-3xl font-bold leading-tight">
            {product.name}
          </h1>

          <p className="mt-4 text-3xl font-bold text-primary">
            {formatCurrency(product.price, product.currency)}
          </p>

          {/* Stock status */}
          <div className="mt-4 flex items-center gap-2">
            <span
              className={`h-2.5 w-2.5 rounded-full ${product.stock > 0 ? "bg-success" : "bg-destructive"}`}
            />
            <span
              className={`text-sm font-medium ${product.stock > 0 ? "text-success" : "text-destructive"}`}
            >
              {product.stock > 0
                ? product.stock <= 5
                  ? `Son ${product.stock} adet!`
                  : `${product.stock} adet stokta`
                : "Stokta yok"}
            </span>
          </div>

          {product.description && (
            <p className="mt-6 text-muted-foreground leading-relaxed">
              {product.description}
            </p>
          )}

          {/* Quantity + Add to basket */}
          {isAuthenticated && product.stock > 0 && (
            <div className="mt-8 flex items-center gap-4">
              <div className="flex items-center border border-border rounded-lg">
                <button
                  onClick={() => setQuantity((q) => Math.max(1, q - 1))}
                  className="px-3 py-2 text-lg hover:bg-accent transition-colors rounded-l-lg"
                  disabled={quantity <= 1}
                >
                  −
                </button>
                <span className="px-4 py-2 text-sm font-semibold min-w-[40px] text-center border-x border-border">
                  {quantity}
                </span>
                <button
                  onClick={() =>
                    setQuantity((q) => Math.min(product.stock, q + 1))
                  }
                  className="px-3 py-2 text-lg hover:bg-accent transition-colors rounded-r-lg"
                  disabled={quantity >= product.stock}
                >
                  +
                </button>
              </div>

              <Button
                size="lg"
                onClick={() =>
                  upsertItem.mutate({
                    productId: product.id,
                    quantity,
                  })
                }
                disabled={upsertItem.isPending}
                className="flex-1 sm:flex-none shadow-md"
              >
                {upsertItem.isPending ? "Ekleniyor..." : "Sepete Ekle"}
              </Button>
            </div>
          )}

          <p className="mt-auto pt-6 text-xs text-muted-foreground">
            Eklenme tarihi: {formatDate(product.createdAt)}
          </p>
        </div>
      </div>

      {/* Related products */}
      {relatedProducts.length > 0 && (
        <section className="mt-16">
          <h2 className="text-xl font-bold mb-6">Benzer Ürünler</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {relatedProducts.map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
