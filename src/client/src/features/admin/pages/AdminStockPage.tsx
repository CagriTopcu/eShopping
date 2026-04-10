import { useState } from "react";
import { RoleGate } from "@/features/auth/components/RoleGate.tsx";
import { ROLES } from "@/config/constants.ts";
import { useProducts } from "@/features/catalog/api/catalog.queries.ts";
import { useSetStock } from "../api/admin.mutations.ts";
import { useStockByProduct } from "../api/admin.queries.ts";
import { Button } from "@/shared/components/ui/Button.tsx";
import { Input } from "@/shared/components/ui/Input.tsx";
import { Badge } from "@/shared/components/ui/Badge.tsx";
import { Skeleton } from "@/shared/components/ui/Skeleton.tsx";
import { Pagination } from "@/shared/components/ui/Pagination.tsx";
import { Dialog } from "@/shared/components/ui/Dialog.tsx";
import { formatCurrency } from "@/shared/lib/format.ts";

// ── Stock cell with lazy-loaded stock data ───────────────

function StockCell({ productId }: { productId: string }) {
  const { data, isLoading } = useStockByProduct(productId);

  if (isLoading) return <Skeleton className="h-5 w-12 inline-block" />;

  const qty = data?.availableQuantity ?? 0;

  return (
    <div className="flex items-center gap-2 justify-end">
      <span
        className={`h-2 w-2 rounded-full ${
          qty === 0
            ? "bg-destructive"
            : qty <= 10
              ? "bg-warning"
              : "bg-success"
        }`}
      />
      <span
        className={`font-semibold tabular-nums ${
          qty === 0
            ? "text-destructive"
            : qty <= 10
              ? "text-warning"
              : "text-foreground"
        }`}
      >
        {qty}
      </span>
    </div>
  );
}

// ── Edit stock dialog ────────────────────────────────────

function EditStockDialog({
  productId,
  productName,
  open,
  onOpenChange,
}: {
  productId: string;
  productName: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const { data: stockData } = useStockByProduct(productId);
  const setStock = useSetStock(productId);
  const [quantity, setQuantity] = useState<number>(0);

  // Sync when stock data loads
  const currentStock = stockData?.availableQuantity ?? 0;

  return (
    <Dialog
      open={open}
      onOpenChange={onOpenChange}
      title="Stok Guncelle"
      description={productName}
    >
      <div className="space-y-4 mt-2">
        <div className="flex items-center justify-between p-3 rounded-lg bg-muted/50">
          <span className="text-sm text-muted-foreground">Mevcut Stok</span>
          <span className="text-2xl font-bold tabular-nums">{currentStock}</span>
        </div>

        <div className="flex flex-wrap gap-2">
          {[0, 5, 10, 25, 50, 100, 250, 500].map((q) => (
            <button
              key={q}
              type="button"
              onClick={() => setQuantity(q)}
              className={`px-3 py-1.5 text-sm rounded-md border transition-all ${
                quantity === q
                  ? "border-primary bg-primary/10 text-primary font-medium"
                  : "border-border hover:border-primary/50"
              }`}
            >
              {q}
            </button>
          ))}
        </div>

        <div>
          <label className="text-sm font-medium mb-1 block">Yeni Stok Miktari</label>
          <Input
            type="number"
            min={0}
            value={quantity}
            onChange={(e) => setQuantity(Math.max(0, Number(e.target.value)))}
          />
        </div>

        <div className="flex justify-end gap-3 pt-2">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Vazgec
          </Button>
          <Button
            onClick={() => {
              setStock.mutate(quantity, {
                onSuccess: () => onOpenChange(false),
              });
            }}
            disabled={setStock.isPending || quantity === currentStock}
          >
            {setStock.isPending ? "Guncelleniyor..." : "Stok Guncelle"}
          </Button>
        </div>
      </div>
    </Dialog>
  );
}

// ── Main page ────────────────────────────────────────────

function AdminStockContent() {
  const [page, setPage] = useState(1);
  const { data, isLoading } = useProducts({ page, pageSize: 20 });
  const [editProduct, setEditProduct] = useState<{ id: string; name: string } | null>(null);

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Stok Yonetimi</h1>
          <p className="text-sm text-muted-foreground mt-1">
            Tum urunlerin stok seviyelerini goruntuleyin ve yonetin
          </p>
        </div>
      </div>

      {/* Legend */}
      <div className="flex items-center gap-4 text-xs text-muted-foreground">
        <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-success" /> Yeterli ({">"}10)</span>
        <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-warning" /> Dusuk (1-10)</span>
        <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-destructive" /> Tukendi (0)</span>
      </div>

      {/* Table */}
      <div className="rounded-xl border border-border bg-card overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-muted/50">
                <th className="text-left p-4 font-medium">Urun</th>
                <th className="text-left p-4 font-medium">Kategori</th>
                <th className="text-right p-4 font-medium">Fiyat</th>
                <th className="text-right p-4 font-medium w-28">Stok</th>
                <th className="text-right p-4 font-medium w-32">Islem</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                Array.from({ length: 8 }).map((_, i) => (
                  <tr key={i} className="border-b border-border">
                    <td className="p-4"><Skeleton className="h-5 w-40" /></td>
                    <td className="p-4"><Skeleton className="h-5 w-20" /></td>
                    <td className="p-4"><Skeleton className="h-5 w-16 ml-auto" /></td>
                    <td className="p-4"><Skeleton className="h-5 w-12 ml-auto" /></td>
                    <td className="p-4"><Skeleton className="h-8 w-20 ml-auto" /></td>
                  </tr>
                ))
              ) : !data?.items.length ? (
                <tr>
                  <td colSpan={5} className="p-12 text-center text-muted-foreground">
                    Urun bulunamadi
                  </td>
                </tr>
              ) : (
                data.items.map((product) => (
                  <tr
                    key={product.id}
                    className="border-b border-border last:border-0 hover:bg-muted/30 transition-colors"
                  >
                    <td className="p-4">
                      <p className="font-medium">{product.name}</p>
                      <p className="text-xs text-muted-foreground mt-0.5 font-mono">
                        {product.id.slice(0, 8)}
                      </p>
                    </td>
                    <td className="p-4">
                      <Badge variant="secondary">{product.category}</Badge>
                    </td>
                    <td className="p-4 text-right tabular-nums">
                      {formatCurrency(product.price, product.currency)}
                    </td>
                    <td className="p-4">
                      <StockCell productId={product.id} />
                    </td>
                    <td className="p-4 text-right">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setEditProduct({ id: product.id, name: product.name })}
                      >
                        Duzenle
                      </Button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {data && data.totalPages > 1 && (
        <Pagination page={data.page} totalPages={data.totalPages} onPageChange={setPage} />
      )}

      {/* Edit stock dialog */}
      {editProduct && (
        <EditStockDialog
          productId={editProduct.id}
          productName={editProduct.name}
          open={!!editProduct}
          onOpenChange={(open) => !open && setEditProduct(null)}
        />
      )}
    </div>
  );
}

export default function AdminStockPage() {
  return (
    <RoleGate role={ROLES.ADMIN}>
      <AdminStockContent />
    </RoleGate>
  );
}
