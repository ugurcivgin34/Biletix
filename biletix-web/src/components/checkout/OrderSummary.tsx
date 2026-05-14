import { Calendar, MapPin, Ticket } from "lucide-react"
import { formatDate, formatPrice } from "@/lib/utils/format"
import type { Booking } from "@/lib/types/booking"

interface Props {
  booking: Booking
  eventTitle?: string
  eventDate?: string
  venueName?: string
}

export function OrderSummary({ booking, eventTitle, eventDate, venueName }: Props) {
  return (
    <div className="space-y-4 rounded-xl bg-gray-50 p-6">
      <h3 className="font-semibold text-gray-900">Sipariş Özeti</h3>

      {eventTitle && (
        <div className="space-y-2 text-sm text-gray-600">
          <div className="flex items-center gap-2">
            <Ticket size={14} />
            <span className="font-medium text-gray-900">{eventTitle}</span>
          </div>
          {eventDate && (
            <div className="flex items-center gap-2">
              <Calendar size={14} />
              <span>{formatDate(eventDate)}</span>
            </div>
          )}
          {venueName && (
            <div className="flex items-center gap-2">
              <MapPin size={14} />
              <span>{venueName}</span>
            </div>
          )}
        </div>
      )}

      <div className="space-y-2 border-t pt-4">
        {booking.items.map((item) => (
          <div key={item.ticketTypeId} className="flex justify-between gap-4 text-sm">
            <span className="text-gray-600">
              {item.ticketTypeName} x {item.quantity}
            </span>
            <span>{formatPrice(item.totalPrice)}</span>
          </div>
        ))}
      </div>

      <div className="flex justify-between border-t pt-4 font-bold">
        <span>Toplam</span>
        <span className="text-lg text-red-500">{formatPrice(booking.totalAmount)}</span>
      </div>

      {booking.expiresAt && (
        <p className="text-center text-xs text-orange-500">
          Rezervasyon {formatDate(booking.expiresAt)} tarihinde sona eriyor
        </p>
      )}
    </div>
  )
}
