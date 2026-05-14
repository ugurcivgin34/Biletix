"use client"

import { useState } from "react"
import axios from "axios"
import Link from "next/link"
import { Calendar, Eye, MapPin, MoreVertical, Send, Users, XCircle } from "lucide-react"
import { Card, CardContent } from "@/components/ui/card"
import { useToast } from "@/hooks/use-toast"
import { organizerApi } from "@/lib/api/organizer"
import type { EventSummary } from "@/lib/types/event"
import { formatDate, formatPrice } from "@/lib/utils/format"
import { EventStatusBadge } from "./EventStatusBadge"

interface Props {
  event: EventSummary
  onRefresh: () => void
}

function getOrganizerErrorMessage(error: unknown, fallback: string) {
  if (axios.isAxiosError<{ detail?: string; error?: string }>(error)) {
    return error.response?.data?.detail || error.response?.data?.error || fallback
  }

  return fallback
}

export function OrganizerEventCard({ event, onRefresh }: Props) {
  const { toast } = useToast()
  const [isLoading, setIsLoading] = useState(false)
  const [showMenu, setShowMenu] = useState(false)

  const handlePublish = async () => {
    setIsLoading(true)
    try {
      await organizerApi.publishEvent(event.id)
      toast({ title: "Etkinlik yayınlandı!", description: event.title })
      onRefresh()
    } catch (error) {
      toast({
        title: "Hata",
        description: getOrganizerErrorMessage(error, "Yayınlanamadı"),
        variant: "destructive",
      })
    } finally {
      setIsLoading(false)
      setShowMenu(false)
    }
  }

  const handleCancel = async () => {
    if (!window.confirm("Etkinliği iptal etmek istediğinize emin misiniz?")) return

    setIsLoading(true)
    try {
      await organizerApi.cancelEvent(event.id, "Organizatör tarafından iptal edildi")
      toast({ title: "Etkinlik iptal edildi" })
      onRefresh()
    } catch (error) {
      toast({
        title: "Hata",
        description: getOrganizerErrorMessage(error, "İptal edilemedi"),
        variant: "destructive",
      })
    } finally {
      setIsLoading(false)
      setShowMenu(false)
    }
  }

  return (
    <Card className="relative">
      <CardContent className="p-5">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0 flex-1">
            <div className="mb-2 flex items-center gap-2">
              <EventStatusBadge status={event.status} />
            </div>
            <h3 className="truncate font-semibold text-gray-900">{event.title}</h3>
            <p className="mt-1 text-sm text-gray-500">{event.performerName}</p>

            <div className="mt-3 space-y-1 text-sm text-gray-500">
              <div className="flex items-center gap-1.5">
                <Calendar size={13} />
                <span>{formatDate(event.startDate)}</span>
              </div>
              <div className="flex items-center gap-1.5">
                <MapPin size={13} />
                <span>
                  {event.venueName}, {event.venueCity}
                </span>
              </div>
              <div className="flex items-center gap-1.5">
                <Users size={13} />
                <span>{event.totalAvailableTickets?.toLocaleString("tr-TR") ?? 0} bilet kaldı</span>
              </div>
            </div>

            <div className="mt-3 flex items-center justify-between border-t pt-3">
              <span className="text-sm font-medium text-red-500">
                {formatPrice(event.minPrice)}&apos;den başlayan
              </span>
            </div>
          </div>

          <div className="relative">
            <button
              type="button"
              onClick={() => setShowMenu((current) => !current)}
              className="rounded-lg p-1.5 transition-colors hover:bg-gray-100"
              aria-label="Etkinlik aksiyonları"
            >
              <MoreVertical size={18} className="text-gray-400" />
            </button>

            {showMenu && (
              <div className="absolute right-0 top-8 z-10 w-44 rounded-xl border bg-white py-1 shadow-lg">
                <Link
                  href={`/events/${event.id}`}
                  className="flex items-center gap-2 px-3 py-2 text-sm transition-colors hover:bg-gray-50"
                  onClick={() => setShowMenu(false)}
                >
                  <Eye size={14} />
                  Görüntüle
                </Link>

                {event.status === "Draft" && (
                  <button
                    type="button"
                    onClick={handlePublish}
                    disabled={isLoading}
                    className="flex w-full items-center gap-2 px-3 py-2 text-left text-sm text-green-600 hover:bg-gray-50"
                  >
                    <Send size={14} />
                    Yayınla
                  </button>
                )}

                {(event.status === "Draft" || event.status === "Published") && (
                  <button
                    type="button"
                    onClick={handleCancel}
                    disabled={isLoading}
                    className="flex w-full items-center gap-2 px-3 py-2 text-left text-sm text-red-500 hover:bg-gray-50"
                  >
                    <XCircle size={14} />
                    İptal Et
                  </button>
                )}
              </div>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
