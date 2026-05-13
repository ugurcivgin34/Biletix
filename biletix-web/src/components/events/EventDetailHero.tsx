import { Calendar, MapPin, Users } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import type { EventDetail } from "@/lib/types/event"
import { formatDate, formatPrice } from "@/lib/utils/format"

interface Props {
  event: EventDetail
}

export function EventDetailHero({ event }: Props) {
  const totalAvailableTickets =
    event.totalAvailableTickets ?? event.ticketTypes.reduce((sum, ticketType) => sum + ticketType.availableCount, 0)
  const minPrice =
    event.minPrice ??
    event.ticketTypes.reduce(
      (currentMin, ticketType) => Math.min(currentMin, ticketType.price),
      event.ticketTypes[0]?.price ?? 0
    )

  return (
    <div className="relative overflow-hidden rounded-2xl bg-gray-950 text-white">
      <div className="absolute inset-0 bg-[linear-gradient(135deg,rgba(17,24,39,1)_0%,rgba(127,29,29,0.92)_52%,rgba(17,24,39,1)_100%)]" />
      <div className="relative p-8 md:p-12">
        <div className="flex flex-col items-start gap-8 md:flex-row">
          <div className="flex-1 space-y-4">
            <div className="flex items-center gap-2">
              <Badge className="bg-red-500">{event.performerGenre}</Badge>
              <Badge variant="outline" className="border-white/30 text-white">
                {event.status === "Published" ? "Satışta" : event.status}
              </Badge>
            </div>

            <h1 className="text-3xl font-bold md:text-4xl">{event.title}</h1>
            <p className="text-lg text-gray-300">{event.performerName}</p>

            <div className="space-y-2 text-gray-300">
              <div className="flex items-center gap-2">
                <Calendar size={18} />
                <span>{formatDate(event.startDate)}</span>
              </div>
              <div className="flex items-center gap-2">
                <MapPin size={18} />
                <span>
                  {event.venueName} — {event.venueCity}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <Users size={18} />
                <span>{totalAvailableTickets.toLocaleString("tr-TR")} bilet kaldı</span>
              </div>
            </div>
          </div>

          <div className="w-full space-y-2 rounded-xl bg-white/10 p-6 backdrop-blur md:w-64">
            <p className="text-sm text-gray-300">Başlangıç fiyatı</p>
            <p className="text-3xl font-bold text-red-300">{formatPrice(minPrice)}</p>
            <p className="text-xs text-gray-400">KDV dahil</p>
          </div>
        </div>
      </div>
    </div>
  )
}
