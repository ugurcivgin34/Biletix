"use client"

import { useCallback, useEffect, useState } from "react"
import { CheckCircle, Clock, Loader2, Users, X } from "lucide-react"
import { Button } from "@/components/ui/button"
import { type QueueStatus, useQueueStream } from "@/lib/hooks/useQueue"

interface Props {
  eventId: string
  isOpen: boolean
  initialStatus: QueueStatus
  onProceed: () => void
  onClose: () => void
  onLeave: () => void
}

export function WaitingQueueModal({
  eventId,
  isOpen,
  initialStatus,
  onProceed,
  onClose,
  onLeave,
}: Props) {
  const [status, setStatus] = useState<QueueStatus>(initialStatus)
  const [canProceed, setCanProceed] = useState(initialStatus.canProceed)

  useEffect(() => {
    setStatus(initialStatus)
    setCanProceed(initialStatus.canProceed)
  }, [initialStatus])

  const handleProceed = useCallback(() => {
    setCanProceed(true)
    window.setTimeout(onProceed, 1500)
  }, [onProceed])

  const handleUpdate = useCallback((nextStatus: QueueStatus) => {
    setStatus(nextStatus)
  }, [])

  useQueueStream(eventId, isOpen && !canProceed, handleProceed, handleUpdate)

  if (!isOpen) return null

  const minutes = Math.ceil(status.estimatedWaitSeconds / 60)
  const passedCount = Math.max(0, status.totalInQueue - status.position)
  const progress =
    status.totalInQueue > 0 ? Math.min(100, (passedCount / status.totalInQueue) * 100) : 0

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      style={{ background: "rgba(0,0,0,0.6)" }}
    >
      <div className="w-full max-w-sm rounded-2xl bg-white p-8 shadow-2xl">
        {canProceed ? (
          <div className="space-y-4 text-center">
            <div className="flex justify-center">
              <div className="rounded-full bg-green-100 p-4">
                <CheckCircle className="text-green-500" size={48} />
              </div>
            </div>
            <h2 className="text-xl font-bold">Sıranız Geldi!</h2>
            <p className="text-sm text-gray-500">Bilet sayfasına yönlendiriliyorsunuz...</p>
            <div className="flex justify-center">
              <Loader2 className="animate-spin text-red-500" size={24} />
            </div>
          </div>
        ) : (
          <div className="space-y-6">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-bold">Bekleme Sırası</h2>
              <button
                type="button"
                onClick={onClose}
                className="text-gray-400 hover:text-gray-600"
                aria-label="Kuyruk penceresini kapat"
              >
                <X size={20} />
              </button>
            </div>

            <div className="py-4 text-center">
              <div className="mb-2 text-6xl font-bold text-red-500">
                {status.position.toLocaleString("tr-TR")}
              </div>
              <p className="text-sm text-gray-500">sıra numaranız</p>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div className="rounded-xl bg-gray-50 p-3 text-center">
                <div className="mb-1 flex items-center justify-center gap-1 text-xs text-gray-500">
                  <Users size={12} />
                  <span>Sıradaki</span>
                </div>
                <p className="font-bold text-gray-900">
                  {status.totalInQueue.toLocaleString("tr-TR")}
                </p>
              </div>
              <div className="rounded-xl bg-gray-50 p-3 text-center">
                <div className="mb-1 flex items-center justify-center gap-1 text-xs text-gray-500">
                  <Clock size={12} />
                  <span>Bekleme</span>
                </div>
                <p className="font-bold text-gray-900">~{minutes} dk</p>
              </div>
            </div>

            <div>
              <div className="mb-1 flex justify-between text-xs text-gray-400">
                <span>İlerleme</span>
                <span>{passedCount} kişi geçti</span>
              </div>
              <div className="h-2 overflow-hidden rounded-full bg-gray-100">
                <div
                  className="h-full rounded-full bg-red-500 transition-all duration-1000"
                  style={{ width: `${progress}%` }}
                />
              </div>
            </div>

            <p className="text-center text-xs text-gray-400">
              Sıranız geldiğinde otomatik yönlendirileceksiniz
            </p>

            <Button
              variant="ghost"
              size="sm"
              onClick={onLeave}
              className="w-full text-gray-400 hover:text-red-500"
            >
              Sıradan Çık
            </Button>
          </div>
        )}
      </div>
    </div>
  )
}
