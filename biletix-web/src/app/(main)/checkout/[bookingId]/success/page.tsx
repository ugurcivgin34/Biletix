"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { CheckCircle, Download, Ticket } from "lucide-react"
import { Button } from "@/components/ui/button"
import { bookingsApi } from "@/lib/api/bookings"
import type { Booking } from "@/lib/types/booking"
import { formatPrice } from "@/lib/utils/format"

interface CheckoutSuccessPageProps {
  params: { bookingId: string }
}

export default function CheckoutSuccessPage({ params }: CheckoutSuccessPageProps) {
  const { bookingId } = params
  const [booking, setBooking] = useState<Booking | null>(null)

  useEffect(() => {
    bookingsApi.getById(bookingId).then(setBooking).catch(() => {})
  }, [bookingId])

  return (
    <div className="mx-auto max-w-lg space-y-6 py-12 text-center">
      <div className="flex justify-center">
        <div className="rounded-full bg-green-100 p-4">
          <CheckCircle className="text-green-500" size={48} />
        </div>
      </div>

      <div>
        <h1 className="text-2xl font-bold text-gray-900">Ödeme Başarılı!</h1>
        <p className="mt-2 text-gray-500">Biletiniz e-posta adresinize gönderildi.</p>
      </div>

      {booking && (
        <div className="space-y-2 rounded-xl bg-gray-50 p-4 text-left">
          <div className="flex justify-between gap-4 text-sm">
            <span className="text-gray-500">Rezervasyon ID</span>
            <span className="font-mono text-xs">{booking.id.substring(0, 8)}...</span>
          </div>
          <div className="flex justify-between gap-4 text-sm">
            <span className="text-gray-500">Toplam</span>
            <span className="font-bold text-red-500">{formatPrice(booking.totalAmount)}</span>
          </div>
          <div className="flex justify-between gap-4 text-sm">
            <span className="text-gray-500">Durum</span>
            <span className="font-medium text-green-600">Onaylandı</span>
          </div>
        </div>
      )}

      <div className="flex flex-col gap-3">
        <Link href="/my-tickets">
          <Button className="w-full gap-2 bg-red-500 hover:bg-red-600">
            <Ticket size={18} />
            Biletlerimi Görüntüle
          </Button>
        </Link>

        {booking && (
          <a
            href={`${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5157"}/api/tickets/${bookingId}/qr`}
            target="_blank"
            rel="noopener noreferrer"
          >
            <Button variant="outline" className="w-full gap-2">
              <Download size={18} />
              QR Bileti İndir
            </Button>
          </a>
        )}

        <Link href="/">
          <Button variant="ghost" className="w-full">
            Ana Sayfaya Dön
          </Button>
        </Link>
      </div>
    </div>
  )
}
