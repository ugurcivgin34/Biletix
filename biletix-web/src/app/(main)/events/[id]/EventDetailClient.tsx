"use client"

import { useState } from "react"
import axios from "axios"
import { useRouter } from "next/navigation"
import { ErrorBoundaryWrapper } from "@/components/common/ErrorBoundaryWrapper"
import { ErrorMessage } from "@/components/common/ErrorMessage"
import { PageLoader } from "@/components/common/PageLoader"
import { EventDetailHero } from "@/components/events/EventDetailHero"
import { type SelectedTicket, TicketTypeSelector } from "@/components/events/TicketTypeSelector"
import { WaitingQueueModal } from "@/components/queue/WaitingQueueModal"
import { Separator } from "@/components/ui/separator"
import { bookingsApi } from "@/lib/api/bookings"
import { useEvent } from "@/lib/hooks/useEvents"
import { useJoinQueue } from "@/lib/hooks/useQueue"
import { useAuthStore } from "@/lib/stores/authStore"
import { useToast } from "@/hooks/use-toast"

interface Props {
  id: string
}

function getCheckoutErrorMessage(error: unknown) {
  if (axios.isAxiosError<{ detail?: string; error?: string }>(error)) {
    return error.response?.data?.detail || error.response?.data?.error || "Rezervasyon başarısız"
  }

  return "Rezervasyon başarısız"
}

function EventDetailContent({ id }: Props) {
  const { data: event, isLoading, error, refetch } = useEvent(id)
  const { isAuthenticated } = useAuthStore()
  const router = useRouter()
  const { toast } = useToast()
  const [isCheckingOut, setIsCheckingOut] = useState(false)
  const [showQueue, setShowQueue] = useState(false)
  const [pendingItems, setPendingItems] = useState<SelectedTicket[]>([])
  const { status: queueStatus, joinQueue, leaveQueue } = useJoinQueue(id)

  if (isLoading) {
    return <PageLoader />
  }

  if (error || !event) {
    return (
      <ErrorMessage
        title="Etkinlik bulunamadı"
        message="Etkinlik yüklenemedi veya artık yayında değil."
        onRetry={() => void refetch()}
      />
    )
  }

  const proceedToCheckout = async (items: SelectedTicket[]) => {
    setShowQueue(false)
    setIsCheckingOut(true)

    try {
      const result = await bookingsApi.checkout({
        eventId: id,
        items: items.map((item) => ({
          ticketTypeId: item.ticketTypeId,
          quantity: item.quantity,
        })),
      })

      router.push(`/checkout/${result.bookingId}?secret=${encodeURIComponent(result.clientSecret)}`)
    } catch (checkoutError) {
      const message = getCheckoutErrorMessage(checkoutError)
      toast({ title: "Hata", description: message, variant: "destructive" })
    } finally {
      setIsCheckingOut(false)
    }
  }

  const handleCheckout = async (items: SelectedTicket[]) => {
    if (!isAuthenticated) {
      router.push(`/login?from=/events/${id}`)
      return
    }

    setPendingItems(items)
    setIsCheckingOut(true)

    try {
      const nextQueueStatus = await joinQueue()

      if (!nextQueueStatus || nextQueueStatus.canProceed) {
        await proceedToCheckout(items)
        return
      }

      setShowQueue(true)
      setIsCheckingOut(false)
    } catch {
      await proceedToCheckout(items)
    }
  }

  const handleQueueClose = async () => {
    setShowQueue(false)
    await leaveQueue()
  }

  return (
    <div className="mx-auto max-w-4xl space-y-8">
      <EventDetailHero event={event} />

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          <div>
            <h2 className="mb-3 text-xl font-bold">Etkinlik Hakkında</h2>
            <p className="leading-relaxed text-gray-600">{event.description}</p>
          </div>

          <Separator />

          <div>
            <h2 className="mb-3 text-xl font-bold">Mekan Bilgisi</h2>
            <div className="space-y-2 rounded-xl bg-gray-50 p-4">
              <p className="font-semibold">{event.venueName}</p>
              <p className="text-gray-500">{event.venueCity}</p>
              <p className="text-sm text-gray-400">
                Kapasite: {event.venueCapacity.toLocaleString("tr-TR")} kişi
              </p>
            </div>
          </div>
        </div>

        <div className="lg:col-span-1">
          <TicketTypeSelector
            ticketTypes={event.ticketTypes}
            onCheckout={handleCheckout}
            isLoading={isCheckingOut}
          />
        </div>
      </div>

      {showQueue && queueStatus && (
        <WaitingQueueModal
          eventId={id}
          isOpen={showQueue}
          initialStatus={queueStatus}
          onProceed={() => proceedToCheckout(pendingItems)}
          onClose={handleQueueClose}
          onLeave={handleQueueClose}
        />
      )}
    </div>
  )
}

export function EventDetailClient({ id }: Props) {
  return (
    <ErrorBoundaryWrapper>
      <EventDetailContent id={id} />
    </ErrorBoundaryWrapper>
  )
}
