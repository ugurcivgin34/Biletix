"use client"

import Link from "next/link"
import { Calendar, Plus, Ticket, TrendingUp, Users } from "lucide-react"
import { EmptyState } from "@/components/common/EmptyState"
import { LoadingSpinner } from "@/components/common/LoadingSpinner"
import { OrganizerEventCard } from "@/components/organizer/OrganizerEventCard"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { organizerApi } from "@/lib/api/organizer"
import type { EventSummary } from "@/lib/types/event"
import { useQuery } from "@tanstack/react-query"

export default function OrganizerDashboardPage() {
  const { data, isLoading, refetch } = useQuery({
    queryKey: ["organizer", "events"],
    queryFn: () => organizerApi.getMyEvents({ pageSize: 50 }),
  })

  const events = data?.items ?? []
  const stats = {
    total: events.length,
    published: events.filter((event) => event.status === "Published").length,
    draft: events.filter((event) => event.status === "Draft").length,
    totalTickets: events.reduce(
      (sum: number, event: EventSummary) => sum + (event.totalAvailableTickets ?? 0),
      0
    ),
  }

  return (
    <div className="space-y-8">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold">Organizatör Paneli</h1>
          <p className="mt-1 text-sm text-gray-500">Etkinliklerinizi yönetin</p>
        </div>
        <Link href="/organizer/events/new">
          <Button className="gap-2 bg-red-500 hover:bg-red-600">
            <Plus size={18} />
            Yeni Etkinlik
          </Button>
        </Link>
      </div>

      <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
        {[
          { icon: Calendar, label: "Toplam Etkinlik", value: stats.total, color: "text-blue-500" },
          { icon: TrendingUp, label: "Yayında", value: stats.published, color: "text-green-500" },
          { icon: Ticket, label: "Taslak", value: stats.draft, color: "text-orange-500" },
          {
            icon: Users,
            label: "Kalan Bilet",
            value: stats.totalTickets.toLocaleString("tr-TR"),
            color: "text-red-500",
          },
        ].map(({ icon: Icon, label, value, color }) => (
          <Card key={label}>
            <CardContent className="p-5">
              <div className="flex items-center gap-3">
                <div className="rounded-lg bg-gray-50 p-2">
                  <Icon size={18} className={color} />
                </div>
                <div>
                  <p className="text-2xl font-bold">{value}</p>
                  <p className="text-xs text-gray-500">{label}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div>
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-lg font-semibold">Etkinliklerim</h2>
          <Link href="/organizer/events" className="text-sm text-red-500 hover:underline">
            Tümünü gör →
          </Link>
        </div>

        {isLoading ? (
          <div className="flex justify-center py-12">
            <LoadingSpinner size="lg" />
          </div>
        ) : events.length === 0 ? (
          <EmptyState
            icon={Calendar}
            title="Henüz etkinlik yok"
            description="İlk etkinliğinizi oluşturun."
            action={
              <Link href="/organizer/events/new">
                <Button className="bg-red-500 hover:bg-red-600">Etkinlik Oluştur</Button>
              </Link>
            }
          />
        ) : (
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
            {events.map((event) => (
              <OrganizerEventCard key={event.id} event={event} onRefresh={() => void refetch()} />
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

