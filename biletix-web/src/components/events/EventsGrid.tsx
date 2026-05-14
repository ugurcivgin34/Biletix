import { Ticket } from "lucide-react"
import { EmptyState } from "@/components/common/EmptyState"
import type { EventSummary } from "@/lib/types/event"
import { EventCard } from "./EventCard"
import { EventCardSkeleton } from "./EventCardSkeleton"

interface Props {
  events: EventSummary[]
  isLoading?: boolean
}

export function EventsGrid({ events, isLoading }: Props) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {Array.from({ length: 8 }).map((_, index) => (
          <EventCardSkeleton key={index} />
        ))}
      </div>
    )
  }

  if (!events.length) {
    return (
      <EmptyState
        icon={Ticket}
        title="Etkinlik bulunamadı"
        description="Arama kriterlerinize uygun etkinlik yok. Filtreleri değiştirmeyi deneyin."
      />
    )
  }

  return (
    <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
      {events.map((event) => (
        <EventCard key={event.id} event={event} />
      ))}
    </div>
  )
}

