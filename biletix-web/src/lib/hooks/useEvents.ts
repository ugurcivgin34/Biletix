import { useQuery } from "@tanstack/react-query"
import { eventsApi, type SearchEventsParams } from "@/lib/api/events"

export const eventKeys = {
  all: ["events"] as const,
  search: (params: SearchEventsParams) => ["events", "search", params] as const,
  detail: (id: string) => ["events", id] as const,
  myEvents: () => ["events", "my"] as const,
}

export function useSearchEvents(params: SearchEventsParams = {}) {
  return useQuery({
    queryKey: eventKeys.search(params),
    queryFn: () => eventsApi.search(params),
    staleTime: 30 * 1000,
  })
}

export function useEvent(id: string) {
  return useQuery({
    queryKey: eventKeys.detail(id),
    queryFn: () => eventsApi.getById(id),
    enabled: Boolean(id),
  })
}

export function useMyEvents() {
  return useQuery({
    queryKey: eventKeys.myEvents(),
    queryFn: () => eventsApi.getMyEvents(),
  })
}

