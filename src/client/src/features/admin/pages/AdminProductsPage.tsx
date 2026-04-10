import { useState } from "react";
import { Link } from "@tanstack/react-router";
import { RoleGate } from "@/features/auth/components/RoleGate.tsx";
import { ROLES } from "@/config/constants.ts";
import { useProducts } from "@/features/catalog/api/catalog.queries.ts";
import { useDeleteProduct } from "@/features/catalog/api/catalog.mutations.ts";
import { Button } from "@/shared/components/ui/Button.tsx";
import { Badge } from "@/shared/components/ui/Badge.tsx";
import { Pagination } from "@/shared/components/ui/Pagination.tsx";
import { Dialog } from "@/shared/components/ui/Dialog.tsx";
import { formatCurrency } from "@/shared/lib/format.ts";

function AdminProductsContent() {
  const [page, setPage] = useState(1);
  const { data, isLoading } = useProducts({ page, pageSize: 20 });
  const deleteProduct = useDeleteProduct();
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const handleDelete = () => {
    if (deleteId) {
      deleteProduct.mutate(deleteId, {
        onSuccess: () => setDeleteId(null),
      });
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Ürün Yönetimi</h1>
        <Button asChild>
          <Link to="/admin/products/$productId/edit" params={{ productId: "new" }}>
            Yeni Ürün Ekle
          </Link>
        </Button>
      </div>

      <div className="rounded-lg border border-border bg-card overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-muted/50">
                <th className="text-left p-4 font-medium">Ürün</th>
                <th className="text-left p-4 font-medium">Kategori</th>
                <th className="text-right p-4 font-medium">Fiyat</th>
                <th className="text-right p-4 font-medium">İşlemler</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={4} className="p-8 text-center text-muted-foreground">
                    Yükleniyor...
                  </td>
                </tr>
              ) : !data?.items.length ? (
                <tr>
                  <td colSpan={4} className="p-8 text-center text-muted-foreground">
                    Ürün bulunamadı
                  </td>
                </tr>
              ) : (
                data.items.map((product) => (
                  <tr
                    key={product.id}
                    className="border-b border-border last:border-0 hover:bg-muted/30"
                  >
                    <td className="p-4">
                      <p className="font-medium">{product.name}</p>
                      <p className="text-xs text-muted-foreground mt-0.5">
                        {product.id.slice(0, 8)}...
                      </p>
                    </td>
                    <td className="p-4">
                      <Badge variant="secondary">{product.category}</Badge>
                    </td>
                    <td className="p-4 text-right">
                      {formatCurrency(product.price, product.currency)}
                    </td>
                    <td className="p-4 text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button variant="ghost" size="sm" asChild>
                          <Link
                            to="/admin/products/$productId/edit"
                            params={{ productId: product.id }}
                          >
                            Düzenle
                          </Link>
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-destructive"
                          onClick={() => setDeleteId(product.id)}
                        >
                          Sil
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {data && (
        <Pagination
          page={data.page}
          totalPages={data.totalPages}
          onPageChange={setPage}
        />
      )}

      <Dialog
        open={!!deleteId}
        onOpenChange={(open) => !open && setDeleteId(null)}
        title="Ürünü Sil"
        description="Bu işlem geri alınamaz. Ürünü silmek istediğinizden emin misiniz?"
      >
        <div className="flex justify-end gap-3 mt-4">
          <Button variant="outline" onClick={() => setDeleteId(null)}>
            İptal
          </Button>
          <Button
            variant="destructive"
            onClick={handleDelete}
            disabled={deleteProduct.isPending}
          >
            {deleteProduct.isPending ? "Siliniyor..." : "Sil"}
          </Button>
        </div>
      </Dialog>
    </div>
  );
}

export default function AdminProductsPage() {
  return (
    <RoleGate role={ROLES.ADMIN}>
      <AdminProductsContent />
    </RoleGate>
  );
}
