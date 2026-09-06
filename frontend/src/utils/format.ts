export const money = (value: number, currency = "TRY") =>
  new Intl.NumberFormat("tr-TR", {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(value);
export const financialDate = (value: string) =>
  new Intl.DateTimeFormat("tr-TR", {
    day: "numeric",
    month: "short",
    year: "numeric",
  }).format(new Date(value + "T12:00:00"));
export const today = () =>
  new Intl.DateTimeFormat("en-CA", {
    timeZone: import.meta.env.VITE_FINANCIAL_TIME_ZONE ?? "Europe/Istanbul",
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  }).format(new Date());
export const monthName = (month: number) =>
  new Intl.DateTimeFormat("tr-TR", { month: "short" }).format(
    new Date(2026, month - 1, 1),
  );
