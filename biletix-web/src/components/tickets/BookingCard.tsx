"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import {
  AlertCircle,
  CheckCircle,
  Clock,
  Download,
  QrCode,
  XCircle,
} from "lucide-react"
import axios from "axios"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { useToast } from "@/hooks/use-toast"
import { ticketsApi } from "@/lib/api/tickets"
import type { Booking } from "@/lib/types/booking"
import { formatDate, formatPrice } from "@/lib/utils/format"

const statusConfig = {
  Confirmed: {
    label: "Onaylandı",
    icon: CheckCircle,
    className: "border-green-200 bg-green-100 text-green-700",
  },
  Pending: {
    label: "Ödeme Bekleniyor",
    icon: Clock,
    className: "border-yellow-200 bg-yellow-100 text-yellow-700",
  },
  Cancelled: {
    label: "İptal Edildi",
    icon: XCircle,
    className: "border-red-200 bg-red-100 text-red-700",
  },
  Expired: {
    label: "Süresi Doldu",
    icon: AlertCircle,
    className: "border-gray-200 bg-gray-100 text-gray-600",
  },
}

interface Props {
  booking: Booking
}

export function BookingCard({ booking }: Props) {
  const [showQr, setShowQr] = useState(false)
  const [qrUrl, setQrUrl] = useState<string | null>(null)
  const [isQrLoading, setIsQrLoading] = useState(false)
  const { toast } = useToast()
  const config = statusConfig[booking.status]
  const StatusIcon = config.icon

  useEffect(() => {
    return () => {
      if (qrUrl) URL.revokeObjectURL(qrUrl)
    }
  }, [qrUrl])

  const fetchQrBlob = async () => {
    return ticketsApi.getQr(booking.id)
  }

  const showQrError = (error: unknown) => {
    const status = axios.isAxiosError(error) ? error.response?.status : undefined
    const message =
      status === 400
        ? "QR bilet henüz hazır değil. Ödeme onayı birkaç saniye sürebilir."
        : status === 401
          ? "Oturumunuz doğrulanamadı. Tekrar giriş yapıp deneyin."
          : "QR bilet alınamadı. Lütfen biraz sonra tekrar deneyin."

    toast({
      title: "QR alınamadı",
      description: message,
      variant: "destructive",
    })
  }

  const handleToggleQr = async () => {
    if (showQr) {
      setShowQr(false)
      return
    }

    setShowQr(true)
    if (qrUrl) return

    setIsQrLoading(true)
    try {
      const blob = await fetchQrBlob()
      setQrUrl(URL.createObjectURL(blob))
    } catch (error) {
      setShowQr(false)
      showQrError(error)
    } finally {
      setIsQrLoading(false)
    }
  }

  const handleDownload = async () => {
    try {
      const blob = qrUrl ? await fetch(qrUrl).then((response) => response.blob()) : await fetchQrBlob()
      const objectUrl = URL.createObjectURL(blob)
      const link = document.createElement("a")
      link.href = objectUrl
      link.download = `ticket-${booking.id}.png`
      document.body.appendChild(link)
      link.click()
      link.remove()
      URL.revokeObjectURL(objectUrl)
    } catch (error) {
      showQrError(error)
    }
  }

  return (
    <Card className={booking.status === "Confirmed" ? "border-green-200" : ""}>
      <CardContent className="p-5">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div className="flex-1 space-y-3">
            <Badge className={`gap-1.5 ${config.className}`} variant="outline">
              <StatusIcon size={12} />
              {config.label}
            </Badge>

            <div className="space-y-1">
              {booking.items.map((item) => (
                <div key={item.ticketTypeId} className="flex justify-between gap-4 text-sm">
                  <span className="text-gray-700">
                    {item.ticketTypeName} x {item.quantity}
                  </span>
                  <span className="text-gray-500">{formatPrice(item.totalPrice)}</span>
                </div>
              ))}
            </div>

            <div className="flex justify-between border-t pt-1 font-semibold">
              <span>Toplam</span>
              <span className="text-red-500">{formatPrice(booking.totalAmount)}</span>
            </div>

            {booking.status === "Pending" && booking.expiresAt && (
              <p className="text-xs text-orange-500">
                {formatDate(booking.expiresAt)} tarihine kadar ödeme yapın
              </p>
            )}
          </div>

          {booking.status === "Confirmed" && (
            <div className="flex shrink-0 gap-2 sm:flex-col">
              <Button
                variant="outline"
                size="sm"
                className="gap-1.5"
                onClick={handleToggleQr}
                disabled={isQrLoading}
              >
                <QrCode size={14} />
                {isQrLoading ? "Yükleniyor" : "QR Kod"}
              </Button>

              <Button variant="ghost" size="sm" className="w-full gap-1.5" onClick={handleDownload}>
                <Download size={14} />
                İndir
              </Button>
            </div>
          )}

          {booking.status === "Pending" && (
            <Link href={`/checkout/${booking.id}`} className="shrink-0">
              <Button size="sm" className="bg-red-500 hover:bg-red-600">
                Ödemeyi Tamamla
              </Button>
            </Link>
          )}
        </div>

        {showQr && booking.status === "Confirmed" && (
          <div className="mt-4 flex flex-col items-center gap-3 border-t pt-4">
            <p className="text-xs text-gray-500">Etkinlik girişinde bu kodu gösterin</p>
            {qrUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img
                src={qrUrl}
                alt="QR Bilet"
                className="h-48 w-48 rounded-lg border-4 border-gray-900"
              />
            ) : (
              <div className="flex h-48 w-48 items-center justify-center rounded-lg border text-sm text-gray-500">
                QR yükleniyor
              </div>
            )}
            <p className="font-mono text-xs text-gray-400">{booking.id.substring(0, 16)}...</p>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
