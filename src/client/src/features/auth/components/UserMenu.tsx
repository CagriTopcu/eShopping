import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import { useAuth } from "../hooks/useAuth.ts";
import { Link } from "@tanstack/react-router";

export function UserMenu() {
  const { isAuthenticated, login, logout, username, isAdmin } = useAuth();

  if (!isAuthenticated) {
    return (
      <button
        onClick={() => login()}
        className="text-sm px-4 py-2 rounded-md bg-primary text-primary-foreground hover:bg-primary/90 transition-colors"
      >
        Giriş Yap
      </button>
    );
  }

  return (
    <DropdownMenu.Root>
      <DropdownMenu.Trigger asChild>
        <button className="flex items-center gap-2 px-3 py-2 rounded-md hover:bg-accent transition-colors">
          <div className="h-8 w-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center text-sm font-medium">
            {username?.charAt(0).toUpperCase()}
          </div>
          <span className="text-sm hidden sm:inline">{username}</span>
        </button>
      </DropdownMenu.Trigger>

      <DropdownMenu.Portal>
        <DropdownMenu.Content
          className="min-w-[180px] bg-card rounded-lg shadow-lg border border-border p-1 z-50"
          sideOffset={5}
          align="end"
        >
          <DropdownMenu.Label className="px-3 py-2 text-sm text-muted-foreground">
            {username}
          </DropdownMenu.Label>
          <DropdownMenu.Separator className="h-px bg-border my-1" />

          <DropdownMenu.Item asChild>
            <Link
              to="/orders"
              className="flex items-center px-3 py-2 text-sm rounded-md hover:bg-accent cursor-pointer outline-none"
            >
              Siparişlerim
            </Link>
          </DropdownMenu.Item>

          {isAdmin && (
            <DropdownMenu.Item asChild>
              <Link
                to="/admin"
                className="flex items-center px-3 py-2 text-sm rounded-md hover:bg-accent cursor-pointer outline-none"
              >
                Admin Panel
              </Link>
            </DropdownMenu.Item>
          )}

          <DropdownMenu.Separator className="h-px bg-border my-1" />

          <DropdownMenu.Item
            onSelect={() => logout()}
            className="flex items-center px-3 py-2 text-sm rounded-md hover:bg-destructive hover:text-destructive-foreground cursor-pointer outline-none"
          >
            Çıkış Yap
          </DropdownMenu.Item>
        </DropdownMenu.Content>
      </DropdownMenu.Portal>
    </DropdownMenu.Root>
  );
}
