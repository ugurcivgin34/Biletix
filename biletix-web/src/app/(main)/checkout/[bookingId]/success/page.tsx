"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { CheckCircle, Download, Ticket } from "lucide-react"
import axios from "axios"
import { Button } from "@/components/ui/button"
import { useToast } from "@/hooks/use-toast"
import { bookingsApi } from "@/lib/api/bookings"
import { ticketsApi } from "@/lib/api/tickets"
import type { Booking } from "@/lib/types/booking"
import { formatPrice } from "@/lib/utils/format"

interface CheckoutSuccessPageProps {
  params: { bookingId: string }
}

export default function CheckoutSuccessPage({ params }: CheckoutSuccessPageProps) {
  const { bookingId } = params
  const [booking, setBooking] = useState<Booking | null>(null)
  const [isDownloading, setIsDownloading] = useState(false)
  const { toast } = useToast()

  useEffect(() => {
    let isMounted = true

    const loadBooking = async () => {
      for (let attempt = 0; attempt < 6; attempt += 1) {
        try {
          const loadedBooking = await bookingsApi.getById(bookingId)
          if (!isMounted) return

          setBooking(loadedBooking)
          if (loadedBooking.status === "Confirmed") return
        } catch {
          return
        }

        await new Promise((resolve) => window.setTimeout(resolve, 2000))
      }
    }

    void loadBooking()

    return () => {
      isMounted = false
    }
  }, [bookingId])

  const handleDownloadQr = async () => {
    if (!booking || isDownloading) return

    setIsDownloading(true)
    try {
      const blob = await ticketsApi.getQr(booking.id)
      const objectUrl = URL.createObjectURL(blob)
      const link = document.createElement("a")
      link.href = objectUrl
      link.download = `ticket-${booking.id}.png`
      document.body.appendChild(link)
      link.click()
      link.remove()
      URL.revokeObjectURL(objectUrl)
    } catch (error) {
      const status = axios.isAxiosError(error) ? error.response?.status : undefined
      const message =
        status === 400
          ? "QR bilet henüz hazır değil. Ödeme onayı birkaç saniye sürebilir; Biletlerim sayfasından tekrar deneyin."
          : status === 401
            ? "Oturumunuz doğrulanamadı. Tekrar giriş yapıp deneyin."
            : "QR bilet indirilemedi. Lütfen biraz sonra tekrar deneyin."

      toast({
        title: "QR indirilemedi",
        description: message,
        variant: "destructive",
      })
    } finally {
      setIsDownloading(false)
    }
  }

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
            <span className={booking.status === "Confirmed" ? "font-medium text-green-600" : "font-medium text-orange-600"}>
              {booking.status === "Confirmed" ? "Onaylandı" : "Onay bekleniyor"}
            </span>
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
          <Button
            type="button"
            variant="outline"
            className="w-full gap-2"
            onClick={handleDownloadQr}
            disabled={isDownloading || booking.status !== "Confirmed"}
          >
            <Download size={18} />
            {booking.status !== "Confirmed"
              ? "QR hazırlanıyor"
              : isDownloading
                ? "İndiriliyor..."
                : "QR Bileti İndir"}
          </Button>
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
