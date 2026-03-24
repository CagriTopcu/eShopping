import { Link } from "@tanstack/react-router";
import { RoleGate } from "@/features/auth/components/RoleGate.tsx";
import { ROLES } from "@/config/constants.ts";
import { useProducts } from "@/features/catalog/api/catalog.queries.ts";

function AdminDashboardContent() {
  const { data } = useProducts({ page: 1, pageSize: 1 });

  return (
    <div className="space-y-8 animate-fade-in">
      <div>
        <h1 className="text-2xl font-bold">Admin Panel</h1>
        <p className="text-muted-foreground mt-1">
          Mağazanızı yönetin
        </p>
      </div>

      {/* Stats */}
      <div className="grid sm:grid-cols-3 gap-4">
        <div className="p-5 rounded-xl border border-border bg-card">
          <p className="text-sm text-muted-foreground">Toplam Ürün</p>
          <p className="text-3xl font-bold mt-1">{data?.totalCount ?? "—"}</p>
        </div>
        <div className="p-5 rounded-xl border border-border bg-card">
          <p className="text-sm text-muted-foreground">Kategoriler</p>
          <p className="text-3xl font-bold mt-1">—</p>
        </div>
        <div className="p-5 rounded-xl border border-border bg-card">
          <p className="text-sm text-muted-foreground">Aktif Siparişler</p>
          <p className="text-3xl font-bold mt-1">—</p>
        </div>
      </div>

      {/* Quick actions */}
      <div>
        <h2 className="text-lg font-semibold mb-4">Hızlı İşlemler</h2>
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <Link
            to="/admin/products"
            className="group p-6 rounded-xl border border-border bg-card hover:shadow-md hover:border-primary/30 transition-all"
          >
            <div className="h-10 w-10 rounded-lg bg-primary/10 flex items-center justify-center mb-3 group-hover:bg-primary/20 transition-colors">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="20"
                height="20"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                className="text-primary"
              >
                <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z" />
              </svg>
            </div>
            <h3 className="font-semibold">Ürün Yönetimi</h3>
            <p className="text-sm text-muted-foreground mt-1">
              Ürün ekleme, düzenleme ve stok yönetimi
            </p>
          </Link>

          <Link
            to="/admin/products/$productId/edit"
            params={{ productId: "new" }}
            className="group p-6 rounded-xl border border-border bg-card hover:shadow-md hover:border-primary/30 transition-all"
          >
            <div className="h-10 w-10 rounded-lg bg-success/10 flex items-center justify-center mb-3 group-hover:bg-success/20 transition-colors">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="20"
                height="20"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                className="text-success"
              >
                <line x1="12" y1="5" x2="12" y2="19" />
                <line x1="5" y1="12" x2="19" y2="12" />
              </svg>
            </div>
            <h3 className="font-semibold">Yeni Ürün Ekle</h3>
            <p className="text-sm text-muted-foreground mt-1">
              Kataloga yeni ürün ekleyin
            </p>
          </Link>
        </div>
      </div>
    </div>
  );
}

export default function AdminDashboardPage() {
  return (
    <RoleGate role={ROLES.ADMIN}>
      <AdminDashboardContent />
    </RoleGate>
  );
}
