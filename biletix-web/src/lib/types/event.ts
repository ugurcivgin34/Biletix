export interface EventSummary {
  id: string
  title: string
  startDate: string
  status: string
  venueName: string
  venueCity: string
  performerName: string
  minPrice: number
  totalAvailableTickets: number
}

export interface EventDetail extends EventSummary {
  description: string
  endDate: string
  imageUrl?: string
  venueId: string
  venueCapacity: number
  performerId: string
  performerGenre: string
  ticketTypes: TicketType[]
  createdAt: string
}

export interface TicketType {
  id: string
  name: string
  price: number
  totalCapacity: number
  soldCount: number
  reservedCount: number
  availableCount: number
}

export interface EventSearchParams {
  q?: string
  city?: string
  genre?: string
  page?: number
  pageSize?: number
}

