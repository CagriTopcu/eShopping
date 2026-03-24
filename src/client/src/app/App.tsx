import { RouterProvider } from "@tanstack/react-router";
import { useEffect } from "react";
import { useAuth as useOidcAuth } from "react-oidc-context";
import { router } from "./router.tsx";
import { configureApiClient } from "@/shared/api/client.ts";

function AppInner() {
  const auth = useOidcAuth();

  useEffect(() => {
    configureApiClient({
      getAccessToken: () => auth.user?.access_token,
      onUnauthorized: () => {
        auth.signinRedirect();
      },
    });
  }, [auth]);

  return <RouterProvider router={router} />;
}

export default function App() {
  return <AppInner />;
}
