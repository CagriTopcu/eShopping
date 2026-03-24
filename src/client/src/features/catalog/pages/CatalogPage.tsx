import { useState } from "react";
import { useProducts } from "../api/catalog.queries.ts";
import { ProductGrid } from "../components/ProductGrid.tsx";
import { ProductFilters } from "../components/ProductFilters.tsx";
import { Pagination } from "@/shared/components/ui/Pagination.tsx";
import { useDebounce } from "@/shared/hooks/useDebounce.ts";
import type { ProductFilters as ProductFiltersType } from "@/shared/types/common.types.ts";

export default function CatalogPage() {
  const [filters, setFilters] = useState<ProductFiltersType>({
    page: 1,
    pageSize: 12,
  });

  const debouncedFilters = useDebounce(filters, 300);
  const { data, isLoading } = useProducts(debouncedFilters);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Ürünler</h1>
        <p className="text-muted-foreground mt-1">
          {data?.totalCount
            ? `${data.totalCount} ürün bulundu`
            : "Ürünleri keşfedin"}
        </p>
      </div>

      <ProductFilters filters={filters} onFiltersChange={setFilters} />

      <ProductGrid products={data?.items} isLoading={isLoading} />

      {data && (
        <Pagination
          page={data.page}
          totalPages={data.totalPages}
          onPageChange={(page) => setFilters((prev) => ({ ...prev, page }))}
        />
      )}
    </div>
  );
}
