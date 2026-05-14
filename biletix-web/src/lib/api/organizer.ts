import { apiClient } from "./client"
import type { EventSummary } from "@/lib/types/event"
import type { PagedResult } from "@/lib/types/common"

export interface CreateOrganizerEventPayload {
  title: string
  description: string
  startDate: string
  endDate: string
  venueId: string
  performerId: string
  imageUrl?: string
  ticketTypes: { name: string; price: number; totalCapacity: number }[]
}

export interface AddTicketTypePayload {
  name: string
  price: number
  totalCapacity: number
}

export const organizerApi = {
  getMyEvents: async (params: Record<string, unknown> = {}): Promise<PagedResult<EventSummary>> => {
    const { data } = await apiClient.get<PagedResult<EventSummary>>("/api/events/my", { params })
    return data
  },

  createEvent: async (payload: CreateOrganizerEventPayload): Promise<string> => {
    const { data } = await apiClient.post<string>("/api/events", payload)
    return data
  },

  publishEvent: async (eventId: string) => {
    const { data } = await apiClient.post(`/api/events/${eventId}/publish`)
    return data
  },

  cancelEvent: async (eventId: string, reason: string) => {
    const { data } = await apiClient.post(`/api/events/${eventId}/cancel`, { reason })
    return data
  },

  addTicketType: async (eventId: string, payload: AddTicketTypePayload): Promise<string> => {
    const { data } = await apiClient.post<string>(`/api/events/${eventId}/ticket-types`, payload)
    return data
  },

  createPerformer: async (name: string, genre: string) => {
    const { data } = await apiClient.post("/api/performers", { name, genre })
    return data
  },

  getPerformers: async () => {
    const { data } = await apiClient.get("/api/performers?pageSize=100")
    return data
  },

  getVenues: async () => {
    const { data } = await apiClient.get("/api/venues?pageSize=100")
    return data
  },
}
