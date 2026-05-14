"use client"

import { useEffect, useRef, useState } from "react"

interface Props {
  onScan: (result: string) => void
  isActive: boolean
}

type Html5QrcodeInstance = InstanceType<typeof import("html5-qrcode").Html5Qrcode>

export function QrScanner({ onScan, isActive }: Props) {
  const scannerRef = useRef<Html5QrcodeInstance | null>(null)
  const [hasCamera, setHasCamera] = useState(true)
  const [isStarting, setIsStarting] = useState(false)

  useEffect(() => {
    const stopScanner = async () => {
      if (!scannerRef.current) return

      try {
        await scannerRef.current.stop()
      } catch {
      } finally {
        scannerRef.current = null
      }
    }

    if (!isActive) {
      void stopScanner()
      return
    }

    let cancelled = false

    const startScanner = async () => {
      setIsStarting(true)
      setHasCamera(true)

      try {
        const { Html5Qrcode } = await import("html5-qrcode")
        if (cancelled) return

        const scanner = new Html5Qrcode("qr-reader")
        scannerRef.current = scanner

        await scanner.start(
          { facingMode: "environment" },
          { fps: 10, qrbox: { width: 250, height: 250 } },
          (decodedText) => onScan(decodedText),
          () => {}
        )
      } catch {
        setHasCamera(false)
      } finally {
        setIsStarting(false)
      }
    }

    void startScanner()

    return () => {
      cancelled = true
      void stopScanner()
    }
  }, [isActive, onScan])

  if (!hasCamera) {
    return (
      <div className="py-8 text-center text-sm text-gray-500">
        Kamera erişimi sağlanamadı. Manuel token girişi kullanın.
      </div>
    )
  }

  return (
    <div className="relative">
      <div
        id="qr-reader"
        className="w-full overflow-hidden rounded-xl"
        style={{ minHeight: 280 }}
      />
      {isStarting && (
        <div className="absolute inset-0 flex items-center justify-center rounded-xl bg-gray-100">
          <p className="text-sm text-gray-500">Kamera başlatılıyor...</p>
        </div>
      )}
    </div>
  )
}
