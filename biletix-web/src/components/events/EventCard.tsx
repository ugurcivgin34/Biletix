"use client"

import Link from "next/link"
import { Calendar, MapPin, Ticket, Users } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import { Card, CardContent } from "@/components/ui/card"
import type { EventSummary } from "@/lib/types/event"
import { formatDate, formatPrice } from "@/lib/utils/format"

interface Props {
  event: EventSummary
}

export function EventCard({ event }: Props) {
  const isAvailable = event.totalAvailableTickets > 0
  const isAlmostSoldOut = event.totalAvailableTickets > 0 && event.totalAvailableTickets < 100

  return (
    <Link href={`/events/${event.id}`} className="block h-full">
      <Card className="h-full cursor-pointer overflow-hidden rounded-lg py-0 transition-all duration-200 hover:-translate-y-1 hover:shadow-lg">
        <div className="relative flex h-48 items-center justify-center overflow-hidden bg-red-700">
          <div className="absolute inset-0 bg-[linear-gradient(135deg,#ef4444_0%,#991b1b_50%,#111827_100%)]" />
          <div className="relative flex flex-col items-center justify-center p-4 text-center text-white">
            <Ticket size={40} className="mb-2 opacity-70" />
            <span className="text-lg font-bold leading-tight">{event.performerName}</span>
          </div>
          {!isAvailable && (
            <div className="absolute inset-0 flex items-center justify-center bg-black/60">
              <Badge variant="destructive" className="text-sm">
                Tükendi
              </Badge>
            </div>
          )}
          {isAlmostSoldOut && isAvailable && (
            <div className="absolute right-2 top-2">
              <Badge className="bg-orange-500 text-xs text-white">Son {event.totalAvailableTickets} bilet!</Badge>
            </div>
          )}
        </div>

        <CardContent className="p-4">
          <h3 className="mb-2 line-clamp-2 font-semibold text-gray-900 transition-colors group-hover:text-red-500">
            {event.title}
          </h3>

          <div className="space-y-1.5 text-sm text-gray-500">
            <div className="flex items-center gap-1.5">
              <Calendar size={14} className="shrink-0" />
              <span>{formatDate(event.startDate)}</span>
            </div>
            <div className="flex items-center gap-1.5">
              <MapPin size={14} className="shrink-0" />
              <span className="truncate">
                {event.venueName}, {event.venueCity}
              </span>
            </div>
            <div className="flex items-center gap-1.5">
              <Users size={14} className="shrink-0" />
              <span>{event.totalAvailableTickets.toLocaleString("tr-TR")} bilet kaldı</span>
            </div>
          </div>

          <div className="mt-3 flex items-center justify-between border-t pt-3">
            <span className="text-lg font-bold text-red-500">{formatPrice(event.minPrice)}&apos;den başlayan</span>
          </div>
        </CardContent>
      </Card>
    </Link>
  )
}

