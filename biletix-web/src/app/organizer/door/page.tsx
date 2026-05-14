"use client"

import { useCallback, useState } from "react"
import { Camera, Keyboard } from "lucide-react"
import { QrScanner } from "@/components/door/QrScanner"
import { ScanResult } from "@/components/door/ScanResult"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ticketsApi, type ValidateTicketResponse } from "@/lib/api/tickets"
import { useAuthStore } from "@/lib/stores/authStore"
import { useToast } from "@/hooks/use-toast"

type Mode = "idle" | "camera" | "manual"

export default function DoorValidationPage() {
  const { user } = useAuthStore()
  const { toast } = useToast()
  const [mode, setMode] = useState<Mode>("idle")
  const [manualToken, setManualToken] = useState("")
  const [result, setResult] = useState<ValidateTicketResponse | null>(null)
  const [isValidating, setIsValidating] = useState(false)
  const [scanCount, setScanCount] = useState({ valid: 0, invalid: 0 })

  const scannedBy = user?.fullName ?? user?.email ?? "Kapı Görevlisi"

  const validate = useCallback(
    async (token: string) => {
      if (!token.trim() || isValidating) return

      setIsValidating(true)
      setMode("idle")

      try {
        const response = await ticketsApi.validate(token.trim(), scannedBy)
        setResult(response)
        setScanCount((current) => ({
          valid: current.valid + (response.isValid ? 1 : 0),
          invalid: current.invalid + (!response.isValid ? 1 : 0),
        }))
      } catch {
        toast({
          title: "Bağlantı Hatası",
          description: "Sunucuya bağlanılamadı",
          variant: "destructive",
        })
      } finally {
        setIsValidating(false)
      }
    },
    [isValidating, scannedBy, toast]
  )

  const handleReset = () => {
    setResult(null)
    setManualToken("")
    setMode("idle")
  }

  return (
    <div className="mx-auto max-w-lg space-y-6">
      <div className="text-center">
        <h1 className="text-2xl font-bold">Kapı Kontrolü</h1>
        <p className="mt-1 text-sm text-gray-500">QR biletleri doğrulayın</p>
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div className="rounded-xl border border-green-200 bg-green-50 p-3 text-center">
          <p className="text-2xl font-bold text-green-600">{scanCount.valid}</p>
          <p className="text-xs text-green-600">Geçerli Giriş</p>
        </div>
        <div className="rounded-xl border border-red-200 bg-red-50 p-3 text-center">
          <p className="text-2xl font-bold text-red-500">{scanCount.invalid}</p>
          <p className="text-xs text-red-500">Geçersiz / Reddedilen</p>
        </div>
      </div>

      {result ? (
        <ScanResult result={result} onReset={handleReset} />
      ) : (
        <div className="space-y-4">
          {mode === "idle" && (
            <div className="grid grid-cols-2 gap-3">
              <Button
                onClick={() => setMode("camera")}
                className="h-20 flex-col gap-2 bg-red-500 hover:bg-red-600"
                disabled={isValidating}
              >
                <Camera size={24} />
                <span>Kamera ile Tara</span>
              </Button>
              <Button
                onClick={() => setMode("manual")}
                variant="outline"
                className="h-20 flex-col gap-2"
                disabled={isValidating}
              >
                <Keyboard size={24} />
                <span>Manuel Giriş</span>
              </Button>
            </div>
          )}

          {mode === "camera" && (
            <div className="space-y-3">
              <QrScanner onScan={validate} isActive={mode === "camera"} />
              <Button variant="outline" onClick={() => setMode("idle")} className="w-full">
                İptal
              </Button>
            </div>
          )}

          {mode === "manual" && (
            <div className="space-y-3">
              <div className="space-y-1">
                <Label>QR Token</Label>
                <Input
                  placeholder="eyJ... token yapıştırın"
                  value={manualToken}
                  onChange={(event) => setManualToken(event.target.value)}
                  onKeyDown={(event) => {
                    if (event.key === "Enter") void validate(manualToken)
                  }}
                  className="font-mono text-xs"
                  autoFocus
                />
              </div>
              <div className="flex gap-2">
                <Button
                  onClick={() => validate(manualToken)}
                  disabled={!manualToken || isValidating}
                  className="flex-1 bg-red-500 hover:bg-red-600"
                >
                  {isValidating ? "Doğrulanıyor..." : "Doğrula"}
                </Button>
                <Button variant="outline" onClick={() => setMode("idle")}>
                  İptal
                </Button>
              </div>
            </div>
          )}

          {isValidating && (
            <div className="py-8 text-center">
              <div className="inline-block h-8 w-8 animate-spin rounded-full border-2 border-red-500 border-t-transparent" />
              <p className="mt-2 text-sm text-gray-500">Doğrulanıyor...</p>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
