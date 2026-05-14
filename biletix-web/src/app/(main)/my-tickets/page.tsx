"use client"

import Link from "next/link"
import { Ticket } from "lucide-react"
import { BookingCard } from "@/components/tickets/BookingCard"
import { EmptyState } from "@/components/common/EmptyState"
import { LoadingSpinner } from "@/components/common/LoadingSpinner"
import { Button } from "@/components/ui/button"
import { useMyBookings } from "@/lib/hooks/useBookings"

export default function MyTicketsPage() {
  const { data, isLoading } = useMyBookings()
  const bookings = data?.items ?? []

  const confirmed = bookings.filter((booking) => booking.status === "Confirmed")
  const pending = bookings.filter((booking) => booking.status === "Pending")
  const past = bookings.filter(
    (booking) => booking.status === "Cancelled" || booking.status === "Expired"
  )

  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <LoadingSpinner size="lg" />
      </div>
    )
  }

  if (!bookings.length) {
    return (
      <EmptyState
        icon={Ticket}
        title="Henüz biletiniz yok"
        description="Etkinlikleri keşfedip bilet satın alabilirsiniz."
        action={
          <Link href="/events">
            <Button className="bg-red-500 hover:bg-red-600">Etkinlikleri Keşfet</Button>
          </Link>
        }
      />
    )
  }

  return (
    <div className="mx-auto max-w-2xl space-y-8">
      <h1 className="text-2xl font-bold">Biletlerim</h1>

      {pending.length > 0 && (
        <section className="space-y-3">
          <h2 className="text-sm font-medium uppercase tracking-wide text-orange-600">
            Ödeme Bekliyor ({pending.length})
          </h2>
          {pending.map((booking) => (
            <BookingCard key={booking.id} booking={booking} />
          ))}
        </section>
      )}

      {confirmed.length > 0 && (
        <section className="space-y-3">
          <h2 className="text-sm font-medium uppercase tracking-wide text-green-600">
            Onaylı Biletler ({confirmed.length})
          </h2>
          {confirmed.map((booking) => (
            <BookingCard key={booking.id} booking={booking} />
          ))}
        </section>
      )}

      {past.length > 0 && (
        <section className="space-y-3">
          <h2 className="text-sm font-medium uppercase tracking-wide text-gray-400">
            Geçmiş ({past.length})
          </h2>
          {past.map((booking) => (
            <BookingCard key={booking.id} booking={booking} />
          ))}
        </section>
      )}
    </div>
  )
}

