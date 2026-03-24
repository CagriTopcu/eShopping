import { Link } from "@tanstack/react-router";

export function Footer() {
  return (
    <footer className="mt-auto border-t border-border bg-muted/30">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-10">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
          <div className="col-span-2 md:col-span-1">
            <Link
              to="/"
              className="text-lg font-bold bg-gradient-to-r from-primary to-indigo-500 bg-clip-text text-transparent"
            >
              eShopping
            </Link>
            <p className="mt-2 text-sm text-muted-foreground">
              Güvenli ve hızlı alışverişin adresi.
            </p>
          </div>

          <div>
            <h4 className="text-sm font-semibold mb-3">Alışveriş</h4>
            <ul className="space-y-2">
              <li>
                <Link
                  to="/products"
                  className="text-sm text-muted-foreground hover:text-foreground transition-colors"
                >
                  Tüm Ürünler
                </Link>
              </li>
            </ul>
          </div>

          <div>
            <h4 className="text-sm font-semibold mb-3">Hesabım</h4>
            <ul className="space-y-2">
              <li>
                <Link
                  to="/orders"
                  className="text-sm text-muted-foreground hover:text-foreground transition-colors"
                >
                  Siparişlerim
                </Link>
              </li>
              <li>
                <Link
                  to="/basket"
                  className="text-sm text-muted-foreground hover:text-foreground transition-colors"
                >
                  Sepetim
                </Link>
              </li>
            </ul>
          </div>

          <div>
            <h4 className="text-sm font-semibold mb-3">Yardım</h4>
            <ul className="space-y-2">
              <li>
                <span className="text-sm text-muted-foreground">
                  destek@eshopping.com
                </span>
              </li>
            </ul>
          </div>
        </div>

        <div className="mt-8 pt-6 border-t border-border">
          <p className="text-center text-xs text-muted-foreground">
            &copy; {new Date().getFullYear()} eShopping. Tüm hakları
            saklıdır.
          </p>
        </div>
      </div>
    </footer>
  );
}
