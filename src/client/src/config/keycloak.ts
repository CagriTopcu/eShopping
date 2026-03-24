import type { AuthProviderProps } from "react-oidc-context";
import { WebStorageStateStore } from "oidc-client-ts";
import { env } from "./env.ts";

export const oidcConfig: AuthProviderProps = {
  authority: env.VITE_KEYCLOAK_URL,
  client_id: env.VITE_KEYCLOAK_CLIENT_ID,
  redirect_uri: env.VITE_KEYCLOAK_REDIRECT_URI,
  post_logout_redirect_uri: env.VITE_KEYCLOAK_POST_LOGOUT_URI,
  response_type: "code",
  scope: "openid profile email",
  automaticSilentRenew: true,
  userStore: new WebStorageStateStore({ store: window.sessionStorage }),
  onSigninCallback: () => {
    window.history.replaceState({}, document.title, "/");
    window.location.replace("/");
  },
};

/**
 * Decode JWT access_token payload (without verification — just for reading claims).
 * Keycloak puts realm_access.roles in the access_token, NOT in id_token.
 */
function decodeAccessToken(
  accessToken: string,
): Record<string, unknown> | null {
  try {
    const parts = accessToken.split(".");
    if (parts.length !== 3) return null;
    const payload = atob(parts[1].replace(/-/g, "+").replace(/_/g, "/"));
    return JSON.parse(payload);
  } catch {
    return null;
  }
}

export function extractRoles(user: {
  access_token?: string;
  profile?: Record<string, unknown>;
}): string[] {
  // First try: realm_access from access_token (Keycloak puts roles here)
  if (user.access_token) {
    const payload = decodeAccessToken(user.access_token);
    if (payload) {
      const realmAccess = payload.realm_access as
        | { roles?: string[] }
        | undefined;
      if (realmAccess?.roles?.length) {
        return realmAccess.roles;
      }
    }
  }

  // Fallback: try from id_token profile
  const realmAccess = user.profile?.realm_access as
    | { roles?: string[] }
    | undefined;
  return realmAccess?.roles ?? [];
}

export function hasRole(
  user: {
    access_token?: string;
    profile?: Record<string, unknown>;
  } | null | undefined,
  role: string,
): boolean {
  if (!user) return false;
  return extractRoles(user).includes(role);
}
