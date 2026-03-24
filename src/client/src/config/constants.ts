export const API_VERSION = "v1";

export const ROLES = {
  ADMIN: "Admin",
  CUSTOMER: "Customer",
} as const;

export const ORDER_STATUS = {
  Pending: 0,
  PaymentReserved: 1,
  Completed: 2,
  Cancelled: 3,
} as const;

export const ORDER_STATUS_LABELS: Record<number, string> = {
  [ORDER_STATUS.Pending]: "Beklemede",
  [ORDER_STATUS.PaymentReserved]: "Ödeme Alındı",
  [ORDER_STATUS.Completed]: "Tamamlandı",
  [ORDER_STATUS.Cancelled]: "İptal Edildi",
};

export const CACHE_TIMES = {
  catalog: { staleTime: 5 * 60 * 1000, gcTime: 30 * 60 * 1000 },
  catalogDetail: { staleTime: 10 * 60 * 1000, gcTime: 30 * 60 * 1000 },
  basket: { staleTime: 30 * 1000, gcTime: 5 * 60 * 1000 },
  orders: { staleTime: 60 * 1000, gcTime: 10 * 60 * 1000 },
} as const;

export const POLLING_INTERVAL = 10_000;
