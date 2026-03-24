import { z } from "zod";

const envSchema = z.object({
  VITE_API_BASE_URL: z.string().url().default("http://localhost:5000"),
  VITE_KEYCLOAK_URL: z
    .string()
    .url()
    .default("http://localhost:8080/realms/eshopping"),
  VITE_KEYCLOAK_CLIENT_ID: z.string().default("eshopping-client"),
  VITE_KEYCLOAK_REDIRECT_URI: z
    .string()
    .url()
    .default("http://localhost:3000/auth/callback"),
  VITE_KEYCLOAK_POST_LOGOUT_URI: z
    .string()
    .url()
    .default("http://localhost:3000"),
});

function parseEnv() {
  const result = envSchema.safeParse(import.meta.env);
  if (!result.success) {
    console.error("Invalid environment variables:", result.error.format());
    throw new Error("Invalid environment variables");
  }
  return result.data;
}

export const env = parseEnv();
