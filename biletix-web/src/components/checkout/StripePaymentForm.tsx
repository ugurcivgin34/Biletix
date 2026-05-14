"use client"

import { useState } from "react"
import { PaymentElement, useElements, useStripe } from "@stripe/react-stripe-js"
import { Loader2, Lock } from "lucide-react"
import { Button } from "@/components/ui/button"
import { formatPrice } from "@/lib/utils/format"

interface Props {
  amount: number
  bookingId: string
  onSuccess: () => void
  onError: (message: string) => void
}

export function StripePaymentForm({ amount, bookingId, onSuccess, onError }: Props) {
  const stripe = useStripe()
  const elements = useElements()
  const [isProcessing, setIsProcessing] = useState(false)

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    if (!stripe || !elements) return

    setIsProcessing(true)

    const { error } = await stripe.confirmPayment({
      elements,
      confirmParams: {
        return_url: `${window.location.origin}/checkout/${bookingId}/success`,
      },
      redirect: "if_required",
    })

    if (error) {
      onError(error.message ?? "Ödeme başarısız")
      setIsProcessing(false)
      return
    }

    onSuccess()
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <PaymentElement
        options={{
          layout: "tabs",
          defaultValues: {
            billingDetails: { address: { country: "TR" } },
          },
        }}
      />

      <div className="flex items-center gap-2 text-xs text-gray-400">
        <Lock size={12} />
        <span>Ödeme bilgileriniz Stripe tarafından güvenle işleniyor</span>
      </div>

      <Button
        type="submit"
        disabled={!stripe || isProcessing}
        className="h-12 w-full bg-red-500 text-base hover:bg-red-600"
      >
        {isProcessing ? (
          <>
            <Loader2 className="mr-2 h-5 w-5 animate-spin" />
            İşleniyor...
          </>
        ) : (
          `${formatPrice(amount)} Öde`
        )}
      </Button>
    </form>
  )
}
