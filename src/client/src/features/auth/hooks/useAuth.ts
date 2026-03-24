import { useAuth as useOidcAuth } from "react-oidc-context";
import { extractRoles, hasRole } from "@/config/keycloak.ts";
import { ROLES } from "@/config/constants.ts";

export function useAuth() {
  const auth = useOidcAuth();

  return {
    isAuthenticated: auth.isAuthenticated,
    isLoading: auth.isLoading,
    user: auth.user,
    accessToken: auth.user?.access_token,
    userId: auth.user?.profile?.sub,
    username:
      (auth.user?.profile?.preferred_username as string | undefined) ??
      auth.user?.profile?.sub,
    roles: auth.user ? extractRoles(auth.user) : [],
    isAdmin: auth.user ? hasRole(auth.user, ROLES.ADMIN) : false,
    isCustomer: auth.user ? hasRole(auth.user, ROLES.CUSTOMER) : false,
    login: () => auth.signinRedirect(),
    logout: () =>
      auth.signoutRedirect({ post_logout_redirect_uri: window.location.origin }),
    error: auth.error,
  };
}
