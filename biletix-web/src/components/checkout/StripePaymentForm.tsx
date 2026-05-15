"use client"

import { useState } from "react"
import { PaymentElement, useElements, useStripe } from "@stripe/react-stripe-js"
import { AlertCircle, Loader2, Lock } from "lucide-react"
import { Button } from "@/components/ui/button"
import { formatPrice } from "@/lib/utils/format"

interface Props {
  amount: number
  bookingId: string
  onSuccess: (paymentIntentId: string) => Promise<void> | void
  onError: (message: string) => void
  onPaymentIntentRefresh?: () => Promise<void>
}

export function StripePaymentForm({
  amount,
  bookingId,
  onSuccess,
  onError,
  onPaymentIntentRefresh,
}: Props) {
  const stripe = useStripe()
  const elements = useElements()
  const [isProcessing, setIsProcessing] = useState(false)
  const [isPaymentElementReady, setIsPaymentElementReady] = useState(false)
  const [paymentElementError, setPaymentElementError] = useState<string | null>(null)

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    if (!stripe || !elements || !isPaymentElementReady) {
      onError("Ödeme formu henüz hazır değil. Lütfen birkaç saniye sonra tekrar deneyin.")
      return
    }

    setIsProcessing(true)

    try {
      const { error: submitError } = await elements.submit()
      if (submitError) {
        onError(submitError.message ?? "Ödeme bilgileri doğrulanamadı")
        setIsProcessing(false)
        return
      }

      const { error, paymentIntent } = await stripe.confirmPayment({
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

      if (!paymentIntent?.id) {
        onError("Ödeme tamamlandı ancak ödeme referansı alınamadı.")
        setIsProcessing(false)
        return
      }

      await onSuccess(paymentIntent.id)
    } catch (error) {
      onError(error instanceof Error ? error.message : "Ödeme başarısız")
      setIsProcessing(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="min-h-[180px]">
        {!isPaymentElementReady && !paymentElementError && (
          <div className="flex h-[180px] items-center justify-center rounded-lg border border-dashed text-sm text-gray-500">
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            Ödeme formu yükleniyor...
          </div>
        )}

        {paymentElementError && (
          <div className="flex min-h-[180px] items-center gap-3 rounded-lg border border-red-200 bg-red-50 p-4 text-left text-sm text-red-700">
            <AlertCircle className="h-5 w-5 shrink-0" />
            <span>{paymentElementError}</span>
          </div>
        )}

        <div className={paymentElementError ? "hidden" : "block"}>
          <PaymentElement
            onReady={() => {
              setPaymentElementError(null)
              setIsPaymentElementReady(true)
            }}
            onLoadError={(event) => {
              const message =
                event.error?.message ?? "Ödeme formu yüklenemedi. Sayfayı yenileyip tekrar deneyin."
              setPaymentElementError(message)

              if (message.toLowerCase().includes("terminal state")) {
                void onPaymentIntentRefresh?.()
              }
            }}
            options={{
              layout: "tabs",
              defaultValues: {
                billingDetails: { address: { country: "TR" } },
              },
            }}
          />
        </div>
      </div>

      <div className="flex items-center gap-2 text-xs text-gray-400">
        <Lock size={12} />
        <span>Ödeme bilgileriniz Stripe tarafından güvenle işleniyor</span>
      </div>

      <Button
        type="submit"
        disabled={!stripe || !isPaymentElementReady || isProcessing || Boolean(paymentElementError)}
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
