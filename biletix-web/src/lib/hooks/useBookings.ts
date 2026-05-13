import { useMutation, useQuery } from "@tanstack/react-query"
import { bookingsApi, type CheckoutRequest } from "@/lib/api/bookings"

export function useMyBookings() {
  return useQuery({
    queryKey: ["bookings", "my"],
    queryFn: bookingsApi.getMyBookings,
  })
}

export function useBooking(id: string) {
  return useQuery({
    queryKey: ["bookings", id],
    queryFn: () => bookingsApi.getById(id),
    enabled: Boolean(id),
  })
}

export function useCheckout() {
  return useMutation({
    mutationFn: (request: CheckoutRequest) => bookingsApi.checkout(request),
  })
}

