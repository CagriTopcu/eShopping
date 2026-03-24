import type { ReactNode } from "react";
import { useRequireRole } from "../hooks/useRoles.ts";
import { ProtectedRoute } from "./ProtectedRoute.tsx";

interface RoleGateProps {
  role: string;
  children: ReactNode;
  fallback?: ReactNode;
}

export function RoleGate({ role, children, fallback }: RoleGateProps) {
  const { authorized, isLoading } = useRequireRole(role);

  return (
    <ProtectedRoute>
      {isLoading ? null : authorized ? (
        <>{children}</>
      ) : (
        fallback ?? (
          <div className="flex flex-col items-center justify-center min-h-[50vh] gap-4">
            <h1 className="text-2xl font-semibold">403 - Yetkisiz Erişim</h1>
            <p className="text-muted-foreground">
              Bu sayfaya erişim yetkiniz bulunmamaktadır.
            </p>
          </div>
        )
      )}
    </ProtectedRoute>
  );
}
