import { normalizeError } from "./error-handler.ts";
import type { ApiError } from "@/shared/types/common.types.ts";

type RequestOptions = {
  method?: string;
  headers?: Record<string, string>;
  body?: unknown;
  params?: Record<string, string | number | boolean | undefined>;
  signal?: AbortSignal;
};

let getAccessToken: (() => string | undefined) | null = null;
let onUnauthorized: (() => void) | null = null;

export function configureApiClient(options: {
  getAccessToken: () => string | undefined;
  onUnauthorized: () => void;
}) {
  getAccessToken = options.getAccessToken;
  onUnauthorized = options.onUnauthorized;
}

export class ApiRequestError extends Error {
  readonly error: ApiError;
  constructor(error: ApiError) {
    super(error.description);
    this.name = "ApiRequestError";
    this.error = error;
  }
}

export async function apiRequest<T>(
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  const { method = "GET", headers = {}, body, params, signal } = options;

  // Use relative path so Vite proxy handles routing in dev
  // In production, configure a reverse proxy (nginx) to forward /api to the gateway
  let url: string;
  if (params) {
    const searchParams = new URLSearchParams();
    for (const [key, value] of Object.entries(params)) {
      if (value !== undefined) {
        searchParams.set(key, String(value));
      }
    }
    const qs = searchParams.toString();
    url = qs ? `${path}?${qs}` : path;
  } else {
    url = path;
  }

  const requestHeaders: Record<string, string> = {
    "Content-Type": "application/json",
    ...headers,
  };

  const token = getAccessToken?.();
  if (token) {
    requestHeaders["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(url, {
    method,
    headers: requestHeaders,
    body: body ? JSON.stringify(body) : undefined,
    signal,
  });

  if (response.status === 401) {
    onUnauthorized?.();
    throw new ApiRequestError(normalizeError(401, null));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const responseBody = response.headers
    .get("content-type")
    ?.includes("application/json")
    ? await response.json()
    : await response.text();

  if (!response.ok) {
    throw new ApiRequestError(normalizeError(response.status, responseBody));
  }

  return responseBody as T;
}

export const api = {
  get: <T>(
    path: string,
    params?: RequestOptions["params"],
    signal?: AbortSignal,
  ) => apiRequest<T>(path, { method: "GET", params, signal }),

  post: <T>(path: string, body?: unknown) =>
    apiRequest<T>(path, { method: "POST", body }),

  put: <T>(path: string, body?: unknown) =>
    apiRequest<T>(path, { method: "PUT", body }),

  patch: <T>(path: string, body?: unknown) =>
    apiRequest<T>(path, { method: "PATCH", body }),

  delete: <T>(path: string) => apiRequest<T>(path, { method: "DELETE" }),
};
