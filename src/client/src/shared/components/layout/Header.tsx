import { useState } from "react";
import { Link } from "@tanstack/react-router";
import { UserMenu } from "@/features/auth/components/UserMenu.tsx";
import { useAuth } from "@/features/auth/hooks/useAuth.ts";
import { useBasket } from "@/features/basket/api/basket.queries.ts";
import { MobileNav } from "./MobileNav.tsx";

export function Header() {
  const { isAuthenticated } = useAuth();
  const { data: basket } = useBasket();
  const [mobileOpen, setMobileOpen] = useState(false);

  const itemCount = basket?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0;

  return (
    <>
      <header className="sticky top-0 z-40 bg-background/80 backdrop-blur-lg border-b border-border/50">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="flex h-16 items-center justify-between">
            {/* Left: hamburger + logo */}
            <div className="flex items-center gap-4">
              <button
                onClick={() => setMobileOpen(true)}
                className="md:hidden p-2 -ml-2 rounded-lg hover:bg-accent transition-colors"
                aria-label="Menüyü aç"
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  width="22"
                  height="22"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                >
                  <line x1="4" y1="6" x2="20" y2="6" />
                  <line x1="4" y1="12" x2="20" y2="12" />
                  <line x1="4" y1="18" x2="20" y2="18" />
                </svg>
              </button>

              <Link
                to="/"
                className="text-xl font-bold bg-gradient-to-r from-primary to-indigo-500 bg-clip-text text-transparent"
              >
                eShopping
              </Link>

              <nav className="hidden md:flex items-center gap-1 ml-6">
                <Link
                  to="/products"
                  className="px-3 py-2 rounded-lg text-sm font-medium text-muted-foreground hover:text-foreground hover:bg-accent transition-colors [&.active]:text-foreground [&.active]:bg-accent"
                >
                  Ürünler
                </Link>
                {isAuthenticated && (
                  <Link
                    to="/orders"
                    className="px-3 py-2 rounded-lg text-sm font-medium text-muted-foreground hover:text-foreground hover:bg-accent transition-colors [&.active]:text-foreground [&.active]:bg-accent"
                  >
                    Siparişlerim
                  </Link>
                )}
              </nav>
            </div>

            {/* Right: basket + user */}
            <div className="flex items-center gap-2">
              {isAuthenticated && (
                <Link
                  to="/basket"
                  className="relative p-2.5 rounded-lg hover:bg-accent transition-colors"
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="20"
                    height="20"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  >
                    <circle cx="8" cy="21" r="1" />
                    <circle cx="19" cy="21" r="1" />
                    <path d="M2.05 2.05h2l2.66 12.42a2 2 0 0 0 2 1.58h9.78a2 2 0 0 0 1.95-1.57l1.65-7.43H5.12" />
                  </svg>
                  {itemCount > 0 && (
                    <span className="absolute -top-0.5 -right-0.5 h-5 w-5 rounded-full bg-primary text-primary-foreground text-[10px] font-bold flex items-center justify-center animate-scale-in">
                      {itemCount > 99 ? "99+" : itemCount}
                    </span>
                  )}
                </Link>
              )}
              <UserMenu />
            </div>
          </div>
        </div>
      </header>

      <MobileNav open={mobileOpen} onOpenChange={setMobileOpen} />
    </>
  );
}
