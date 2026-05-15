import { v4 as uuidv4 } from "uuid"
import { apiClient } from "./client"
import type { Booking } from "@/lib/types/booking"
import type { PagedResult } from "@/lib/types/common"

export interface CheckoutRequest {
  eventId: string
  items: { ticketTypeId: string; quantity: number }[]
}

export interface CheckoutResponse {
  bookingId: string
  clientSecret: string
  message: string
}

export interface PaymentIntentResponse {
  bookingId: string
  clientSecret: string
  paymentIntentId: string
  amount: number
  expiresAt?: string
}

export interface ConfirmPaymentResponse {
  bookingId: string
  status: string
}

export const bookingsApi = {
  checkout: async (request: CheckoutRequest): Promise<CheckoutResponse> => {
    const idempotencyKey = uuidv4()
    const { data } = await apiClient.post<CheckoutResponse>("/api/bookings/checkout", request, {
      headers: { "Idempotency-Key": idempotencyKey },
    })
    return data
  },

  getById: async (id: string): Promise<Booking> => {
    const { data } = await apiClient.get<Booking>(`/api/bookings/${id}`)
    return data
  },

  getMyBookings: async (): Promise<PagedResult<Booking>> => {
    const { data } = await apiClient.get<PagedResult<Booking>>("/api/bookings/my")
    return data
  },

  getPaymentStatus: async (bookingId: string) => {
    const { data } = await apiClient.get(`/api/payments/booking/${bookingId}`)
    return data
  },

  createPaymentIntent: async (bookingId: string): Promise<PaymentIntentResponse> => {
    const { data } = await apiClient.post<PaymentIntentResponse>("/api/payments/create-intent", {
      bookingId,
    })
    return data
  },

  confirmPayment: async (
    bookingId: string,
    paymentIntentId: string
  ): Promise<ConfirmPaymentResponse> => {
    const { data } = await apiClient.post<ConfirmPaymentResponse>("/api/payments/confirm-client", {
      bookingId,
      paymentIntentId,
    })
    return data
  },
}
