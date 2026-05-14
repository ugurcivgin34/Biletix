"use client"

import { Suspense, useEffect, useMemo, useState } from "react"
import Link from "next/link"
import { useRouter, useSearchParams } from "next/navigation"
import { Elements } from "@stripe/react-stripe-js"
import { loadStripe } from "@stripe/stripe-js"
import { Ticket } from "lucide-react"
import { LoadingSpinner } from "@/components/common/LoadingSpinner"
import { OrderSummary } from "@/components/checkout/OrderSummary"
import { StripePaymentForm } from "@/components/checkout/StripePaymentForm"
import { useToast } from "@/hooks/use-toast"
import { bookingsApi } from "@/lib/api/bookings"
import { eventsApi } from "@/lib/api/events"
import type { Booking } from "@/lib/types/booking"
import type { EventDetail } from "@/lib/types/event"

interface CheckoutPageProps {
  params: { bookingId: string }
}

function CheckoutContent({ bookingId }: { bookingId: string }) {
  const searchParams = useSearchParams()
  const clientSecret = searchParams.get("secret")
  const router = useRouter()
  const { toast } = useToast()
  const [booking, setBooking] = useState<Booking | null>(null)
  const [event, setEvent] = useState<EventDetail | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const stripePromise = useMemo(
    () => loadStripe(process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY ?? ""),
    []
  )

  useEffect(() => {
    const loadCheckout = async () => {
      try {
        const loadedBooking = await bookingsApi.getById(bookingId)
        setBooking(loadedBooking)

        const loadedEvent = await eventsApi.getById(loadedBooking.eventId)
        setEvent(loadedEvent)
      } catch {
        toast({
          title: "Hata",
          description: "Rezervasyon yüklenemedi",
          variant: "destructive",
        })
        router.push("/")
      } finally {
        setIsLoading(false)
      }
    }

    loadCheckout()
  }, [bookingId, router, toast])

  const handleSuccess = () => {
    toast({
      title: "Ödeme alındı!",
      description: "Biletiniz e-posta adresinize gönderilecek.",
    })
    router.push(`/checkout/${bookingId}/success`)
  }

  const handleError = (message: string) => {
    toast({
      title: "Ödeme Başarısız",
      description: message,
      variant: "destructive",
    })
  }

  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <LoadingSpinner size="lg" />
      </div>
    )
  }

  if (!booking || !clientSecret) {
    return (
      <div className="py-20 text-center">
        <p className="text-gray-500">Rezervasyon bulunamadı.</p>
        <Link href="/" className="mt-2 inline-block text-red-500 hover:underline">
          Ana sayfaya dön
        </Link>
      </div>
    )
  }

  if (booking.status !== "Pending") {
    return (
      <div className="space-y-3 py-20 text-center">
        <p className="font-medium text-gray-700">
          {booking.status === "Confirmed"
            ? "Bu rezervasyon zaten tamamlandı."
            : "Bu rezervasyon artık geçerli değil."}
        </p>
        <Link href="/my-tickets" className="text-red-500 hover:underline">
          Biletlerime git
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-4xl">
      <div className="mb-8 flex items-center gap-3">
        <Ticket className="text-red-500" size={28} />
        <div>
          <h1 className="text-2xl font-bold">Ödeme</h1>
          <p className="text-sm text-gray-500">Güvenli ödeme - Stripe ile korunuyor</p>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-2">
        <div className="space-y-6">
          <div className="rounded-xl border bg-white p-6">
            <h2 className="mb-4 font-semibold">Kart Bilgileri</h2>
            <Elements
              stripe={stripePromise}
              options={{
                clientSecret,
                appearance: {
                  theme: "stripe",
                  variables: {
                    colorPrimary: "#ef4444",
                    borderRadius: "8px",
                    fontFamily: "Inter, sans-serif",
                  },
                },
                locale: "tr",
              }}
            >
              <StripePaymentForm
                amount={booking.totalAmount}
                bookingId={bookingId}
                onSuccess={handleSuccess}
                onError={handleError}
              />
            </Elements>
          </div>
        </div>

        <OrderSummary
          booking={booking}
          eventTitle={event?.title ?? booking.eventTitle}
          eventDate={event?.startDate}
          venueName={event?.venueName}
        />
      </div>
    </div>
  )
}

export default function CheckoutPage({ params }: CheckoutPageProps) {
  return (
    <Suspense
      fallback={
        <div className="flex justify-center py-20">
          <LoadingSpinner size="lg" />
        </div>
      }
    >
      <CheckoutContent bookingId={params.bookingId} />
    </Suspense>
  )
}

