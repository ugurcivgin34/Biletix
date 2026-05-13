import { apiClient } from "./client"
import type { EventDetail, EventSummary } from "@/lib/types/event"
import type { PagedResult } from "@/lib/types/common"

export interface SearchEventsParams {
  q?: string
  city?: string
  genre?: string
  startDateFrom?: string
  startDateTo?: string
  minPrice?: number
  maxPrice?: number
  sortBy?: "startDate" | "price" | "relevance"
  sortDesc?: boolean
  page?: number
  pageSize?: number
}

export const eventsApi = {
  search: async (params: SearchEventsParams = {}): Promise<PagedResult<EventSummary>> => {
    const { data } = await apiClient.get<PagedResult<EventSummary>>("/api/search/events", { params })
    return data
  },

  getAll: async (
    params: { page?: number; pageSize?: number; status?: string } = {}
  ): Promise<PagedResult<EventSummary>> => {
    const { data } = await apiClient.get<PagedResult<EventSummary>>("/api/events", { params })
    return data
  },

  getById: async (id: string): Promise<EventDetail> => {
    const { data } = await apiClient.get<EventDetail>(`/api/events/${id}`)
    return data
  },

  getMyEvents: async (params = {}): Promise<PagedResult<EventSummary>> => {
    const { data } = await apiClient.get<PagedResult<EventSummary>>("/api/events/my", { params })
    return data
  },
}

