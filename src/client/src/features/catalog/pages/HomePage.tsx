import { Link } from "@tanstack/react-router";
import { useProducts } from "../api/catalog.queries.ts";
import { ProductCard } from "../components/ProductCard.tsx";
import { Skeleton } from "@/shared/components/ui/Skeleton.tsx";
import { Button } from "@/shared/components/ui/Button.tsx";

const CATEGORIES = [
  {
    name: "Elektronik",
    icon: "M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z",
    gradient: "from-blue-500 to-indigo-600",
  },
  {
    name: "Giyim",
    icon: "M6.29 18.251c7.547 0 11.675-6.253 11.675-11.675 0-.178 0-.355-.012-.53A8.348 8.348 0 0020 3.92a8.19 8.19 0 01-2.357.646 4.118 4.118 0 001.804-2.27 8.224 8.224 0 01-2.605.996 4.107 4.107 0 00-6.993 3.743 11.65 11.65 0 01-8.457-4.287 4.106 4.106 0 001.27 5.477A4.073 4.073 0 01.8 7.713v.052a4.105 4.105 0 003.292 4.022 4.095 4.095 0 01-1.853.07 4.108 4.108 0 003.834 2.85A8.233 8.233 0 010 16.407a11.616 11.616 0 006.29 1.84",
    gradient: "from-pink-500 to-rose-600",
  },
  {
    name: "Ev & Yaşam",
    icon: "M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6",
    gradient: "from-emerald-500 to-teal-600",
  },
  {
    name: "Spor",
    icon: "M13 10V3L4 14h7v7l9-11h-7z",
    gradient: "from-orange-500 to-amber-600",
  },
];

export default function HomePage() {
  const { data, isLoading } = useProducts({ page: 1, pageSize: 8 });

  return (
    <div className="space-y-12 animate-fade-in">
      {/* Hero Section */}
      <section className="relative overflow-hidden rounded-2xl bg-gradient-to-br from-primary via-indigo-600 to-purple-700 text-white">
        <div className="absolute inset-0 bg-[url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48ZyBmaWxsPSJub25lIiBmaWxsLXJ1bGU9ImV2ZW5vZGQiPjxnIGZpbGw9IiNmZmYiIGZpbGwtb3BhY2l0eT0iMC4wNSI+PHBhdGggZD0iTTM2IDE4YzMuMzE0IDAgNiAyLjY4NiA2IDZzLTIuNjg2IDYtNiA2LTYtMi42ODYtNi02IDIuNjg2LTYgNi02eiIvPjwvZz48L2c+PC9zdmc+')] opacity-30" />
        <div className="relative px-8 py-16 sm:px-12 sm:py-20 lg:px-16 lg:py-24">
          <div className="max-w-2xl">
            <h1 className="text-3xl sm:text-4xl lg:text-5xl font-bold leading-tight">
              Alışverişin Yeni Adresi
            </h1>
            <p className="mt-4 text-lg sm:text-xl text-white/80 leading-relaxed">
              Binlerce ürünü keşfedin, güvenle alışveriş yapın.
              En iyi fiyatlar ve hızlı teslimat ile tanışın.
            </p>
            <div className="mt-8 flex flex-wrap gap-4">
              <Button
                asChild
                size="lg"
                className="bg-white text-primary hover:bg-white/90 font-semibold shadow-lg"
              >
                <Link to="/products">Ürünleri Keşfet</Link>
              </Button>
            </div>
          </div>
        </div>
      </section>

      {/* Categories */}
      <section>
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-bold">Kategoriler</h2>
        </div>
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          {CATEGORIES.map((cat) => (
            <Link
              key={cat.name}
              to="/products"
              search={{ category: cat.name }}
              className={`group relative overflow-hidden rounded-xl bg-gradient-to-br ${cat.gradient} p-6 text-white transition-all duration-300 hover:scale-[1.02] hover:shadow-xl`}
            >
              <div className="relative z-10">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  width="28"
                  height="28"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="1.5"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  className="mb-3 opacity-80"
                >
                  <path d={cat.icon} />
                </svg>
                <h3 className="font-semibold text-sm sm:text-base">
                  {cat.name}
                </h3>
              </div>
              <div className="absolute inset-0 bg-black/0 group-hover:bg-black/10 transition-colors duration-300" />
            </Link>
          ))}
        </div>
      </section>

      {/* Recent Products */}
      <section>
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-bold">Son Eklenen Ürünler</h2>
          <Button variant="ghost" asChild>
            <Link to="/products">Tümünü Gör →</Link>
          </Button>
        </div>

        {isLoading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {Array.from({ length: 4 }).map((_, i) => (
              <div
                key={i}
                className="rounded-xl border border-border overflow-hidden"
              >
                <Skeleton className="aspect-[4/3]" />
                <div className="p-4 space-y-3">
                  <Skeleton className="h-4 w-3/4" />
                  <Skeleton className="h-6 w-1/3" />
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {data?.items.slice(0, 8).map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        )}
      </section>

      {/* Features */}
      <section className="grid sm:grid-cols-3 gap-6 py-8 border-t border-border">
        <div className="flex items-start gap-4">
          <div className="shrink-0 h-12 w-12 rounded-xl bg-primary/10 flex items-center justify-center">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="22"
              height="22"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              className="text-primary"
            >
              <path d="M5 12h14M12 5l7 7-7 7" />
            </svg>
          </div>
          <div>
            <h3 className="font-semibold">Hızlı Teslimat</h3>
            <p className="text-sm text-muted-foreground mt-1">
              Siparişleriniz en kısa sürede kapınıza gelir
            </p>
          </div>
        </div>
        <div className="flex items-start gap-4">
          <div className="shrink-0 h-12 w-12 rounded-xl bg-success/10 flex items-center justify-center">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="22"
              height="22"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              className="text-success"
            >
              <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
            </svg>
          </div>
          <div>
            <h3 className="font-semibold">Güvenli Ödeme</h3>
            <p className="text-sm text-muted-foreground mt-1">
              256-bit SSL şifreleme ile güvenli alışveriş
            </p>
          </div>
        </div>
        <div className="flex items-start gap-4">
          <div className="shrink-0 h-12 w-12 rounded-xl bg-warning/10 flex items-center justify-center">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="22"
              height="22"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              className="text-warning"
            >
              <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07 19.5 19.5 0 01-6-6 19.79 19.79 0 01-3.07-8.67A2 2 0 014.11 2h3a2 2 0 012 1.72 12.84 12.84 0 00.7 2.81 2 2 0 01-.45 2.11L8.09 9.91a16 16 0 006 6l1.27-1.27a2 2 0 012.11-.45 12.84 12.84 0 002.81.7A2 2 0 0122 16.92z" />
            </svg>
          </div>
          <div>
            <h3 className="font-semibold">7/24 Destek</h3>
            <p className="text-sm text-muted-foreground mt-1">
              Her zaman yanınızdayız, sorularınızı bekliyoruz
            </p>
          </div>
        </div>
      </section>
    </div>
  );
}
