import { format } from "date-fns"
import { tr } from "date-fns/locale"

export function formatDate(date: string | Date): string {
  return format(new Date(date), "dd MMMM yyyy, HH:mm", { locale: tr })
}

export function formatPrice(amount: number): string {
  return new Intl.NumberFormat("tr-TR", {
    style: "currency",
    currency: "TRY",
  }).format(amount)
}

export function formatDateShort(date: string | Date): string {
  return format(new Date(date), "dd MMM yyyy", { locale: tr })
}

