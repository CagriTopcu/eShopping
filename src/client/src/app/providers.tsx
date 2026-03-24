import type { ReactNode } from "react";
import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { AuthProvider } from "react-oidc-context";
import { queryClient } from "@/config/query-client.ts";
import { oidcConfig } from "@/config/keycloak.ts";
import { ErrorBoundary } from "@/shared/components/feedback/ErrorBoundary.tsx";
import { GlobalToast } from "@/shared/components/feedback/GlobalToast.tsx";

interface ProvidersProps {
  children: ReactNode;
}

export function Providers({ children }: ProvidersProps) {
  return (
    <ErrorBoundary>
      <AuthProvider {...oidcConfig}>
        <QueryClientProvider client={queryClient}>
          {children}
          <GlobalToast />
          <ReactQueryDevtools initialIsOpen={false} />
        </QueryClientProvider>
      </AuthProvider>
    </ErrorBoundary>
  );
}
