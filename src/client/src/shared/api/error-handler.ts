import type { ApiError } from "@/shared/types/common.types.ts";

export function normalizeError(status: number, body: unknown): ApiError {
  if (body && typeof body === "object") {
    // RFC 7231 Problem Details format (backend uses this)
    if ("type" in body && "detail" in body) {
      const problem = body as {
        type?: string;
        title?: string;
        status?: number;
        detail?: string;
      };
      return {
        code: problem.type ?? "UNKNOWN",
        description: problem.detail ?? problem.title ?? getDefaultMessage(status),
        type: mapErrorType(undefined, problem.status ?? status),
        status: problem.status ?? status,
      };
    }

    // Custom format fallback
    if ("code" in body && "description" in body) {
      const err = body as { code: string; description: string; type?: string };
      return {
        code: err.code,
        description: err.description,
        type: mapErrorType(err.type, status),
        status,
      };
    }
  }

  return {
    code: "UNKNOWN",
    description: getDefaultMessage(status),
    type: mapErrorType(undefined, status),
    status,
  };
}

function mapErrorType(
  type: string | undefined,
  status: number,
): ApiError["type"] {
  if (type) {
    const validTypes = [
      "Failure",
      "Validation",
      "NotFound",
      "Conflict",
      "Unauthorized",
      "Forbidden",
    ] as const;
    if (validTypes.includes(type as (typeof validTypes)[number])) {
      return type as ApiError["type"];
    }
  }

  switch (status) {
    case 401:
      return "Unauthorized";
    case 403:
      return "Forbidden";
    case 404:
      return "NotFound";
    case 409:
      return "Conflict";
    case 422:
      return "Validation";
    default:
      return "Failure";
  }
}

function getDefaultMessage(status: number): string {
  switch (status) {
    case 401:
      return "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.";
    case 403:
      return "Bu işlem için yetkiniz bulunmamaktadır.";
    case 404:
      return "Aradığınız kaynak bulunamadı.";
    case 409:
      return "İşlem çakışması. Lütfen sayfayı yenileyip tekrar deneyin.";
    case 422:
      return "Gönderilen veriler geçersiz.";
    case 429:
      return "Çok fazla istek gönderildi. Lütfen bir süre bekleyin.";
    default:
      return "Beklenmeyen bir hata oluştu. Lütfen tekrar deneyin.";
  }
}
