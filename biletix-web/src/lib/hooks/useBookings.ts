import { useMutation, useQuery } from "@tanstack/react-query"
import { bookingsApi, type CheckoutRequest } from "@/lib/api/bookings"

export const bookingKeys = {
  all: ["bookings"] as const,
  myBookings: () => ["bookings", "my"] as const,
  detail: (id: string) => ["bookings", id] as const,
}

export function useMyBookings() {
  return useQuery({
    queryKey: bookingKeys.myBookings(),
    queryFn: bookingsApi.getMyBookings,
  })
}

export function useBooking(id: string) {
  return useQuery({
    queryKey: bookingKeys.detail(id),
    queryFn: () => bookingsApi.getById(id),
    enabled: !!id,
  })
}

export function useCheckout() {
  return useMutation({
    mutationFn: (request: CheckoutRequest) => bookingsApi.checkout(request),
  })
}

