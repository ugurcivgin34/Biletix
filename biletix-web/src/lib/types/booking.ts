export interface BookingItem {
  ticketTypeId: string
  ticketTypeName: string
  quantity: number
  unitPrice: number
  totalPrice: number
}

export interface Booking {
  id: string
  eventId: string
  eventTitle: string
  status: "Pending" | "Confirmed" | "Cancelled" | "Expired"
  totalAmount: number
  expiresAt?: string
  items: BookingItem[]
}

export interface ReserveRequest {
  eventId: string
  items: { ticketTypeId: string; quantity: number }[]
}

export interface ReserveResponse {
  bookingId: string
  expiresAt: string
  totalAmount: number
}

