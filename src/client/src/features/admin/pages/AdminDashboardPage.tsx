import { Link } from "@tanstack/react-router";
import { RoleGate } from "@/features/auth/components/RoleGate.tsx";
import { ROLES } from "@/config/constants.ts";
import { useCatalogStats, useOrderStats } from "../api/admin.queries.ts";
import { formatCurrency } from "@/shared/lib/format.ts";

function StatCard({
  label,
  value,
  alert,
}: {
  label: string;
  value: string | number;
  alert?: boolean;
}) {
  return (
    <div className="p-5 rounded-xl border border-border bg-card">
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className={`text-3xl font-bold mt-1 ${alert ? "text-destructive" : ""}`}>
        {value}
      </p>
    </div>
  );
}

function AdminDashboardContent() {
  const { data: catalogStats } = useCatalogStats();
  const { data: orderStats } = useOrderStats();

  return (
    <div className="space-y-8 animate-fade-in">
      <div>
        <h1 className="text-2xl font-bold">Admin Panel</h1>
        <p className="text-muted-foreground mt-1">Magazanizi yonetin</p>
      </div>

      {/* Stats */}
      <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <StatCard
          label="Toplam Urun"
          value={catalogStats?.totalProducts ?? "—"}
        />
        <StatCard
          label="Dusuk Stok Uyarisi"
          value={catalogStats?.lowStockCount ?? "—"}
          alert={(catalogStats?.lowStockCount ?? 0) > 0}
        />
        <StatCard
          label="Kategoriler"
          value={catalogStats?.categories.length ?? "—"}
        />
        <StatCard
          label="Toplam Siparis"
          value={orderStats?.totalOrders ?? "—"}
        />
        <StatCard
          label="Bekleyen Siparisler"
          value={orderStats?.pendingOrders ?? "—"}
          alert={(orderStats?.pendingOrders ?? 0) > 0}
        />
        <StatCard
          label="Toplam Gelir"
          value={
            orderStats?.totalRevenue != null
              ? formatCurrency(orderStats.totalRevenue)
              : "—"
          }
        />
      </div>

      {/* Quick actions */}
      <div>
        <h2 className="text-lg font-semibold mb-4">Hizli Islemler</h2>
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
            <h3 className="font-semibold">Urun Yonetimi</h3>
            <p className="text-sm text-muted-foreground mt-1">
              Urun ekleme, duzenleme ve stok yonetimi
            </p>
          </Link>

          <Link
            to="/admin/orders"
            className="group p-6 rounded-xl border border-border bg-card hover:shadow-md hover:border-primary/30 transition-all"
          >
            <div className="h-10 w-10 rounded-lg bg-warning/10 flex items-center justify-center mb-3 group-hover:bg-warning/20 transition-colors">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="20"
                height="20"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                className="text-warning"
              >
                <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
                <circle cx="9" cy="7" r="4" />
                <path d="M22 21v-2a4 4 0 0 0-3-3.87" />
                <path d="M16 3.13a4 4 0 0 1 0 7.75" />
              </svg>
            </div>
            <h3 className="font-semibold">Siparis Yonetimi</h3>
            <p className="text-sm text-muted-foreground mt-1">
              Siparisleri goruntuleyin ve yonetin
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
            <h3 className="font-semibold">Yeni Urun Ekle</h3>
            <p className="text-sm text-muted-foreground mt-1">
              Kataloga yeni urun ekleyin
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
