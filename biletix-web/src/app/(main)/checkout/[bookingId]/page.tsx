"use client"

import { Suspense } from "react"
import { useSearchParams } from "next/navigation"

interface CheckoutPageProps {
  params: { bookingId: string }
}

function CheckoutContent({ bookingId }: { bookingId: string }) {
  const searchParams = useSearchParams()
  const clientSecret = searchParams.get("secret")

  return (
    <div className="mx-auto max-w-lg space-y-4 py-12 text-center">
      <h1 className="text-2xl font-bold">Ödeme</h1>
      <p className="text-gray-500">Booking ID: {bookingId}</p>
      <p className="truncate font-mono text-xs text-gray-400">
        Client Secret: {clientSecret ? `${clientSecret.substring(0, 30)}...` : "-"}
      </p>
      <p className="text-gray-500">Stripe ödeme formu burada olacak.</p>
    </div>
  )
}

export default function CheckoutPage({ params }: CheckoutPageProps) {
  return (
    <Suspense>
      <CheckoutContent bookingId={params.bookingId} />
    </Suspense>
  )
}

