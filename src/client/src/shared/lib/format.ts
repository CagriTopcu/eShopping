const currencyFormatter = new Intl.NumberFormat("tr-TR", {
  style: "currency",
  currency: "TRY",
  minimumFractionDigits: 2,
});

const usdFormatter = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  minimumFractionDigits: 2,
});

export function formatCurrency(amount: number, currency = "TRY"): string {
  if (currency === "USD") return usdFormatter.format(amount);
  return currencyFormatter.format(amount);
}

const dateFormatter = new Intl.DateTimeFormat("tr-TR", {
  year: "numeric",
  month: "long",
  day: "numeric",
  hour: "2-digit",
  minute: "2-digit",
});

export function formatDate(date: string | Date): string {
  return dateFormatter.format(new Date(date));
}

const shortDateFormatter = new Intl.DateTimeFormat("tr-TR", {
  year: "numeric",
  month: "short",
  day: "numeric",
});

export function formatShortDate(date: string | Date): string {
  return shortDateFormatter.format(new Date(date));
}
