"use client"

import { useState } from "react"
import { Minus, Plus, ShoppingCart } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import type { TicketType } from "@/lib/types/event"
import { formatPrice } from "@/lib/utils/format"

export interface SelectedTicket {
  ticketTypeId: string
  quantity: number
  unitPrice: number
  name: string
}

interface Props {
  ticketTypes: TicketType[]
  onCheckout: (items: SelectedTicket[]) => void
  isLoading?: boolean
}

export function TicketTypeSelector({ ticketTypes, onCheckout, isLoading }: Props) {
  const [quantities, setQuantities] = useState<Record<string, number>>({})

  const updateQty = (id: string, delta: number, max: number) => {
    setQuantities((current) => {
      const qty = current[id] ?? 0
      const next = Math.max(0, Math.min(max, qty + delta))
      return { ...current, [id]: next }
    })
  }

  const selectedItems = ticketTypes
    .filter((ticketType) => (quantities[ticketType.id] ?? 0) > 0)
    .map((ticketType) => ({
      ticketTypeId: ticketType.id,
      quantity: quantities[ticketType.id],
      unitPrice: ticketType.price,
      name: ticketType.name,
    }))

  const totalAmount = selectedItems.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0)
  const totalTickets = selectedItems.reduce((sum, item) => sum + item.quantity, 0)

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-bold">Bilet Seçin</h2>

      {ticketTypes.map((ticketType) => {
        const qty = quantities[ticketType.id] ?? 0
        const isAvailable = ticketType.availableCount > 0
        const maxPerOrder = Math.min(10, ticketType.availableCount)

        return (
          <Card key={ticketType.id} className={isAvailable ? "" : "opacity-60"}>
            <CardContent className="p-4">
              <div className="flex items-center justify-between gap-4">
                <div className="flex-1">
                  <div className="mb-1 flex items-center gap-2">
                    <h3 className="font-semibold">{ticketType.name}</h3>
                    {ticketType.availableCount < 50 && isAvailable && (
                      <Badge variant="destructive" className="text-xs">
                        Son {ticketType.availableCount}!
                      </Badge>
                    )}
                    {!isAvailable && (
                      <Badge variant="secondary" className="text-xs">
                        Tükendi
                      </Badge>
                    )}
                  </div>
                  <p className="text-lg font-bold text-red-500">{formatPrice(ticketType.price)}</p>
                  <p className="text-xs text-gray-400">
                    {ticketType.availableCount.toLocaleString("tr-TR")} kaldı
                  </p>
                </div>

                {isAvailable && (
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => updateQty(ticketType.id, -1, maxPerOrder)}
                      disabled={qty === 0}
                      className="flex h-8 w-8 items-center justify-center rounded-full border transition-colors hover:bg-gray-50 disabled:opacity-30"
                      aria-label={`${ticketType.name} azalt`}
                    >
                      <Minus size={14} />
                    </button>
                    <span className="w-8 text-center text-lg font-semibold">{qty}</span>
                    <button
                      type="button"
                      onClick={() => updateQty(ticketType.id, 1, maxPerOrder)}
                      disabled={qty === maxPerOrder}
                      className="flex h-8 w-8 items-center justify-center rounded-full border transition-colors hover:bg-gray-50 disabled:opacity-30"
                      aria-label={`${ticketType.name} artır`}
                    >
                      <Plus size={14} />
                    </button>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        )
      })}

      {totalTickets > 0 && (
        <Card className="border-red-200 bg-red-50">
          <CardContent className="space-y-3 p-4">
            <h3 className="font-semibold">Sipariş Özeti</h3>
            {selectedItems.map((item) => (
              <div key={item.ticketTypeId} className="flex justify-between text-sm">
                <span>
                  {item.name} × {item.quantity}
                </span>
                <span>{formatPrice(item.quantity * item.unitPrice)}</span>
              </div>
            ))}
            <div className="flex justify-between border-t pt-2 font-bold">
              <span>Toplam</span>
              <span className="text-lg text-red-500">{formatPrice(totalAmount)}</span>
            </div>
            <Button
              onClick={() => onCheckout(selectedItems)}
              disabled={isLoading}
              className="w-full gap-2 bg-red-500 hover:bg-red-600"
              size="lg"
            >
              <ShoppingCart size={18} />
              {isLoading ? "İşleniyor..." : `${totalTickets} Bilet Al — ${formatPrice(totalAmount)}`}
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
