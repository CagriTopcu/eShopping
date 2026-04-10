import { useParams, useNavigate } from "@tanstack/react-router";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useEffect, useState } from "react";
import { RoleGate } from "@/features/auth/components/RoleGate.tsx";
import { ROLES } from "@/config/constants.ts";
import { useProduct } from "@/features/catalog/api/catalog.queries.ts";
import {
  useCreateProduct,
  useUpdateProduct,
} from "@/features/catalog/api/catalog.mutations.ts";
import { useStockByProduct } from "@/features/admin/api/admin.queries.ts";
import { useSetStock } from "@/features/admin/api/admin.mutations.ts";
import { Button } from "@/shared/components/ui/Button.tsx";
import { Input } from "@/shared/components/ui/Input.tsx";
import { Skeleton } from "@/shared/components/ui/Skeleton.tsx";

const toNumber = (val: unknown) => {
  if (val === "" || val === undefined || val === null) return undefined;
  const n = Number(val);
  return isNaN(n) ? val : n;
};

const productSchema = z.object({
  name: z.string().min(1, "Ürün adı zorunludur"),
  category: z.string().min(1, "Kategori zorunludur"),
  price: z.preprocess(toNumber, z.number().positive("Fiyat sıfırdan büyük olmalıdır")),
  currency: z.string().min(1, "Para birimi zorunludur"),
  initialStock: z.preprocess(toNumber, z.number().int().min(0, "Stok negatif olamaz")),
  description: z.string().optional(),
  imageUrl: z.string().url("Geçerli bir URL giriniz").or(z.literal("")).optional(),
});

type ProductFormData = z.infer<typeof productSchema>;

function AdminProductEditContent() {
  const { productId } = useParams({
    from: "/admin/products/$productId/edit",
  });
  const isNew = productId === "new";
  const navigate = useNavigate();
  const { data: product, isLoading } = useProduct(isNew ? "" : productId);
  const { data: stockData } = useStockByProduct(isNew ? "" : productId);
  const createProduct = useCreateProduct();
  const updateProduct = useUpdateProduct(productId);
  const setStock = useSetStock(productId);
  const [newStockQuantity, setNewStockQuantity] = useState<number>(0);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ProductFormData>({
    resolver: zodResolver(productSchema) as never,
    defaultValues: {
      currency: "TRY",
      initialStock: 0,
    },
  });

  useEffect(() => {
    if (product && !isNew) {
      reset({
        name: product.name,
        category: product.category,
        price: product.price,
        currency: product.currency,
        initialStock: 0,
        description: product.description ?? "",
        imageUrl: product.imageUrl ?? "",
      });
    }
  }, [product, isNew, reset]);

  useEffect(() => {
    if (stockData) {
      setNewStockQuantity(stockData.availableQuantity);
    }
  }, [stockData]);

  const onSubmit = (data: ProductFormData) => {
    if (isNew) {
      createProduct.mutate(
        {
          name: data.name,
          category: data.category,
          price: data.price,
          currency: data.currency,
          initialStock: data.initialStock,
          description: data.description || undefined,
          imageUrl: data.imageUrl || undefined,
        },
        { onSuccess: () => navigate({ to: "/admin/products" }) },
      );
    } else {
      updateProduct.mutate(
        {
          name: data.name,
          category: data.category,
          price: data.price,
          currency: data.currency,
          description: data.description || undefined,
          imageUrl: data.imageUrl || undefined,
        },
        { onSuccess: () => navigate({ to: "/admin/products" }) },
      );
    }
  };

  if (!isNew && isLoading) {
    return (
      <div className="max-w-2xl space-y-4">
        <Skeleton className="h-8 w-1/3" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  const currentStock = stockData?.availableQuantity ?? 0;

  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-bold mb-6">
        {isNew ? "Yeni Ürün Ekle" : "Ürün Düzenle"}
      </h1>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <div className="rounded-lg border border-border bg-card p-6 space-y-4">
          <div>
            <label className="text-sm font-medium mb-1 block">Ürün Adı</label>
            <Input
              {...register("name")}
              placeholder="Ürün adını giriniz"
              error={errors.name?.message}
            />
          </div>

          <div>
            <label className="text-sm font-medium mb-1 block">Kategori</label>
            <Input
              {...register("category")}
              placeholder="Elektronik, Giyim, vb."
              error={errors.category?.message}
            />
          </div>

          <div className="grid sm:grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium mb-1 block">Fiyat</label>
              <Input
                type="number"
                step="0.01"
                {...register("price")}
                placeholder="99.99"
                error={errors.price?.message}
              />
            </div>
            <div>
              <label className="text-sm font-medium mb-1 block">
                Para Birimi
              </label>
              <Input
                {...register("currency")}
                placeholder="TRY"
                error={errors.currency?.message}
              />
            </div>
          </div>

          {isNew && (
            <div>
              <label className="text-sm font-medium mb-1 block">
                Başlangıç Stoğu
              </label>
              <Input
                type="number"
                {...register("initialStock")}
                placeholder="0"
                error={errors.initialStock?.message}
              />
            </div>
          )}

          <div>
            <label className="text-sm font-medium mb-1 block">Açıklama</label>
            <Input
              {...register("description")}
              placeholder="Ürün açıklaması (opsiyonel)"
              error={errors.description?.message}
            />
          </div>

          <div>
            <label className="text-sm font-medium mb-1 block">
              Görsel URL
            </label>
            <Input
              {...register("imageUrl")}
              placeholder="https://example.com/image.jpg (opsiyonel)"
              error={errors.imageUrl?.message}
            />
          </div>
        </div>

        <div className="flex gap-3">
          <Button
            type="submit"
            disabled={createProduct.isPending || updateProduct.isPending}
          >
            {createProduct.isPending || updateProduct.isPending
              ? "Kaydediliyor..."
              : isNew
                ? "Ürün Oluştur"
                : "Güncelle"}
          </Button>
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate({ to: "/admin/products" })}
          >
            İptal
          </Button>
        </div>
      </form>

      {!isNew && product && (
        <div className="mt-6 rounded-lg border border-border bg-card p-6 space-y-4">
          <h2 className="text-lg font-semibold">Stok Yonetimi</h2>
          <p className="text-xs text-muted-foreground">
            Stok, Stock servisi tarafindan yonetilmektedir.
          </p>
          <div className="flex items-center gap-4">
            <span className="text-sm text-muted-foreground">Mevcut Stok:</span>
            <span
              className={`text-2xl font-bold ${currentStock > 0 ? "text-success" : "text-destructive"}`}
            >
              {currentStock}
            </span>
          </div>

          <div className="flex flex-wrap gap-2">
            {[0, 10, 25, 50, 100].map((qty) => (
              <Button
                key={qty}
                type="button"
                variant={qty === newStockQuantity ? "default" : "outline"}
                size="sm"
                onClick={() => setNewStockQuantity(qty)}
              >
                {qty}
              </Button>
            ))}
          </div>

          <div className="flex items-center gap-3">
            <Input
              type="number"
              value={newStockQuantity}
              onChange={(e) => setNewStockQuantity(Number(e.target.value))}
              placeholder="Yeni stok miktari"
              className="w-32"
              min={0}
            />
            <Button
              type="button"
              disabled={setStock.isPending || newStockQuantity === currentStock}
              onClick={() => setStock.mutate(newStockQuantity)}
            >
              {setStock.isPending ? "Guncelleniyor..." : "Stok Guncelle"}
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}

export default function AdminProductEditPage() {
  return (
    <RoleGate role={ROLES.ADMIN}>
      <AdminProductEditContent />
    </RoleGate>
  );
}
