import { create } from "zustand"

interface SelectedTicket {
  ticketTypeId: string
  name: string
  quantity: number
  unitPrice: number
}

interface BookingState {
  eventId: string | null
  selectedTickets: SelectedTicket[]
  setEventId: (eventId: string | null) => void
  setTicketQuantity: (ticket: SelectedTicket) => void
  clearBooking: () => void
}

export const useBookingStore = create<BookingState>((set) => ({
  eventId: null,
  selectedTickets: [],
  setEventId: (eventId) => set({ eventId }),
  setTicketQuantity: (ticket) =>
    set((state) => {
      const rest = state.selectedTickets.filter((item) => item.ticketTypeId !== ticket.ticketTypeId)
      return {
        selectedTickets: ticket.quantity > 0 ? [...rest, ticket] : rest,
      }
    }),
  clearBooking: () => set({ eventId: null, selectedTickets: [] }),
}))

