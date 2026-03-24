import { useAuth } from "../hooks/useAuth.ts";

export function LoginButton() {
  const { isAuthenticated, login, logout, username } = useAuth();

  if (isAuthenticated) {
    return (
      <div className="flex items-center gap-3">
        <span className="text-sm text-muted-foreground">{username}</span>
        <button
          onClick={() => logout()}
          className="text-sm px-3 py-1.5 rounded-md border border-border hover:bg-accent transition-colors"
        >
          Çıkış Yap
        </button>
      </div>
    );
  }

  return (
    <button
      onClick={() => login()}
      className="text-sm px-4 py-1.5 rounded-md bg-primary text-primary-foreground hover:bg-primary/90 transition-colors"
    >
      Giriş Yap
    </button>
  );
}
