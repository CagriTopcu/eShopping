import { Input } from "@/shared/components/ui/Input.tsx";
import { Button } from "@/shared/components/ui/Button.tsx";
import type { ProductFilters as ProductFiltersType } from "@/shared/types/common.types.ts";

interface ProductFiltersProps {
  filters: ProductFiltersType;
  onFiltersChange: (filters: ProductFiltersType) => void;
}

export function ProductFilters({
  filters,
  onFiltersChange,
}: ProductFiltersProps) {
  const updateFilter = (key: keyof ProductFiltersType, value: string) => {
    onFiltersChange({
      ...filters,
      [key]: value || undefined,
      page: 1,
    });
  };

  const clearFilters = () => {
    onFiltersChange({ page: 1, pageSize: filters.pageSize });
  };

  const hasActiveFilters =
    filters.name || filters.category || filters.minPrice || filters.maxPrice;

  const activeFilterCount = [
    filters.name,
    filters.category,
    filters.minPrice,
    filters.maxPrice,
  ].filter(Boolean).length;

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end gap-3 p-4 bg-card rounded-xl border border-border shadow-sm">
        {/* Search */}
        <div className="flex-1 min-w-[200px]">
          <label className="text-xs font-medium text-muted-foreground mb-1.5 block">
            Ürün Ara
          </label>
          <div className="relative">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
            >
              <circle cx="11" cy="11" r="8" />
              <path d="m21 21-4.3-4.3" />
            </svg>
            <Input
              placeholder="Ürün adı yazın..."
              value={filters.name ?? ""}
              onChange={(e) => updateFilter("name", e.target.value)}
              className="pl-9"
            />
          </div>
        </div>

        {/* Category */}
        <div className="min-w-[150px]">
          <label className="text-xs font-medium text-muted-foreground mb-1.5 block">
            Kategori
          </label>
          <Input
            placeholder="Tümü"
            value={filters.category ?? ""}
            onChange={(e) => updateFilter("category", e.target.value)}
          />
        </div>

        {/* Price range */}
        <div className="min-w-[100px]">
          <label className="text-xs font-medium text-muted-foreground mb-1.5 block">
            Min Fiyat
          </label>
          <Input
            type="number"
            placeholder="0"
            value={filters.minPrice ?? ""}
            onChange={(e) => updateFilter("minPrice", e.target.value)}
          />
        </div>

        <div className="min-w-[100px]">
          <label className="text-xs font-medium text-muted-foreground mb-1.5 block">
            Max Fiyat
          </label>
          <Input
            type="number"
            placeholder="∞"
            value={filters.maxPrice ?? ""}
            onChange={(e) => updateFilter("maxPrice", e.target.value)}
          />
        </div>

        {hasActiveFilters && (
          <Button
            variant="ghost"
            size="sm"
            onClick={clearFilters}
            className="text-destructive hover:text-destructive"
          >
            Temizle
          </Button>
        )}
      </div>

      {/* Active filter chips */}
      {hasActiveFilters && (
        <div className="flex flex-wrap items-center gap-2">
          <span className="text-xs text-muted-foreground">
            {activeFilterCount} filtre aktif:
          </span>
          {filters.name && (
            <FilterChip
              label={`"${filters.name}"`}
              onRemove={() => updateFilter("name", "")}
            />
          )}
          {filters.category && (
            <FilterChip
              label={filters.category}
              onRemove={() => updateFilter("category", "")}
            />
          )}
          {filters.minPrice && (
            <FilterChip
              label={`Min: ${filters.minPrice}`}
              onRemove={() => updateFilter("minPrice", "")}
            />
          )}
          {filters.maxPrice && (
            <FilterChip
              label={`Max: ${filters.maxPrice}`}
              onRemove={() => updateFilter("maxPrice", "")}
            />
          )}
        </div>
      )}
    </div>
  );
}

function FilterChip({
  label,
  onRemove,
}: {
  label: string;
  onRemove: () => void;
}) {
  return (
    <span className="inline-flex items-center gap-1 px-2.5 py-1 bg-primary/10 text-primary text-xs font-medium rounded-full">
      {label}
      <button
        onClick={onRemove}
        className="ml-0.5 hover:text-primary/70 transition-colors"
      >
        ×
      </button>
    </span>
  );
}
