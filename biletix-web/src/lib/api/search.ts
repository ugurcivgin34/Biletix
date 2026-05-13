import { apiClient } from "@/lib/api/client"
import type { PagedResult } from "@/lib/types/common"
import type { EventSearchParams, EventSummary } from "@/lib/types/event"

export async function searchEvents(params?: EventSearchParams): Promise<PagedResult<EventSummary>> {
  const response = await apiClient.get<PagedResult<EventSummary>>("/api/search/events", { params })
  return response.data
}

