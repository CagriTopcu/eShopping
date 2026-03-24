import * as DialogPrimitive from "@radix-ui/react-dialog";
import { Link } from "@tanstack/react-router";
import { useAuth } from "@/features/auth/hooks/useAuth.ts";

interface MobileNavProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function MobileNav({ open, onOpenChange }: MobileNavProps) {
  const { isAuthenticated, isAdmin, login, logout, username } = useAuth();

  const close = () => onOpenChange(false);

  return (
    <DialogPrimitive.Root open={open} onOpenChange={onOpenChange}>
      <DialogPrimitive.Portal>
        <DialogPrimitive.Overlay className="fixed inset-0 z-50 bg-black/50 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <DialogPrimitive.Content className="fixed inset-y-0 left-0 z-50 w-72 bg-background shadow-xl data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:slide-out-to-left data-[state=open]:slide-in-from-left">
          <div className="flex flex-col h-full">
            <div className="flex items-center justify-between p-4 border-b border-border">
              <DialogPrimitive.Title className="text-lg font-bold text-primary">
                eShopping
              </DialogPrimitive.Title>
              <DialogPrimitive.Close className="p-2 rounded-md hover:bg-accent">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  width="20"
                  height="20"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                >
                  <line x1="18" y1="6" x2="6" y2="18" />
                  <line x1="6" y1="6" x2="18" y2="18" />
                </svg>
              </DialogPrimitive.Close>
            </div>

            <nav className="flex-1 p-4 space-y-1">
              <Link
                to="/"
                onClick={close}
                className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium hover:bg-accent transition-colors"
              >
                Ana Sayfa
              </Link>
              <Link
                to="/products"
                onClick={close}
                className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium hover:bg-accent transition-colors"
              >
                Ürünler
              </Link>
              {isAuthenticated && (
                <>
                  <Link
                    to="/basket"
                    onClick={close}
                    className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium hover:bg-accent transition-colors"
                  >
                    Sepetim
                  </Link>
                  <Link
                    to="/orders"
                    onClick={close}
                    className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium hover:bg-accent transition-colors"
                  >
                    Siparişlerim
                  </Link>
                  {isAdmin && (
                    <>
                      <div className="my-3 border-t border-border" />
                      <Link
                        to="/admin"
                        onClick={close}
                        className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium text-primary hover:bg-accent transition-colors"
                      >
                        Admin Panel
                      </Link>
                    </>
                  )}
                </>
              )}
            </nav>

            <div className="p-4 border-t border-border">
              {isAuthenticated ? (
                <div className="space-y-3">
                  <p className="text-sm text-muted-foreground px-3">
                    {username}
                  </p>
                  <button
                    onClick={() => {
                      close();
                      logout();
                    }}
                    className="w-full text-left px-3 py-2.5 rounded-lg text-sm font-medium text-destructive hover:bg-destructive/10 transition-colors"
                  >
                    Çıkış Yap
                  </button>
                </div>
              ) : (
                <button
                  onClick={() => {
                    close();
                    login();
                  }}
                  className="w-full px-4 py-2.5 rounded-lg text-sm font-medium bg-primary text-primary-foreground hover:bg-primary/90 transition-colors"
                >
                  Giriş Yap
                </button>
              )}
            </div>
          </div>
        </DialogPrimitive.Content>
      </DialogPrimitive.Portal>
    </DialogPrimitive.Root>
  );
}
