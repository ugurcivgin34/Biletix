import { useCallback, useEffect, useState } from "react"
import axios from "axios"
import { apiClient } from "@/lib/api/client"
import { useAuthStore } from "@/lib/stores/authStore"

export interface QueueStatus {
  position: number
  totalInQueue: number
  canProceed: boolean
  estimatedWaitSeconds: number
  isInQueue: boolean
}

function getQueueErrorMessage(error: unknown) {
  if (axios.isAxiosError<{ detail?: string; error?: string }>(error)) {
    return error.response?.data?.detail || error.response?.data?.error || "Kuyruğa katılınamadı"
  }

  return "Kuyruğa katılınamadı"
}

export function useJoinQueue(eventId: string) {
  const [status, setStatus] = useState<QueueStatus | null>(null)
  const [isJoining, setIsJoining] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const joinQueue = useCallback(async () => {
    setIsJoining(true)
    setError(null)

    try {
      const { data } = await apiClient.post<QueueStatus>(`/api/queue/${eventId}/join`)
      setStatus(data)
      return data
    } catch (queueError) {
      setError(getQueueErrorMessage(queueError))
      return null
    } finally {
      setIsJoining(false)
    }
  }, [eventId])

  const leaveQueue = useCallback(async () => {
    try {
      await apiClient.delete(`/api/queue/${eventId}/leave`)
      setStatus(null)
    } catch {
      // Leaving the queue is best-effort; the server will also expire stale entries over time.
    }
  }, [eventId])

  return { status, setStatus, isJoining, error, joinQueue, leaveQueue }
}

export function useQueueStream(
  eventId: string,
  enabled: boolean,
  onProceed: () => void,
  onUpdate: (status: QueueStatus) => void
) {
  const { accessToken } = useAuthStore()

  useEffect(() => {
    if (!enabled || !accessToken) return

    let cancelled = false
    let retryTimer: ReturnType<typeof setTimeout> | null = null
    const controller = new AbortController()

    const connect = async () => {
      try {
        const apiUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5157"
        const response = await fetch(`${apiUrl}/api/queue/${eventId}/stream`, {
          headers: { Authorization: `Bearer ${accessToken}` },
          signal: controller.signal,
        })

        if (!response.ok || !response.body) return

        const reader = response.body.getReader()
        const decoder = new TextDecoder()
        let buffer = ""

        while (!cancelled) {
          const { done, value } = await reader.read()
          if (done) break

          buffer += decoder.decode(value, { stream: true })
          const lines = buffer.split("\n")
          buffer = lines.pop() ?? ""

          for (const line of lines) {
            if (line.startsWith("data: ")) {
              try {
                const data = JSON.parse(line.slice(6)) as QueueStatus | string
                if (typeof data !== "string") {
                  onUpdate(data)
                  if (data.canProceed) {
                    onProceed()
                    return
                  }
                }
              } catch {
                // Ignore malformed SSE payloads and keep the stream alive.
              }
            }

            if (line.startsWith("event: proceed")) {
              onProceed()
              return
            }
          }
        }
      } catch (streamError) {
        if (
          streamError instanceof Error &&
          streamError.name !== "AbortError" &&
          !cancelled
        ) {
          retryTimer = setTimeout(connect, 3000)
        }
      }
    }

    connect()

    return () => {
      cancelled = true
      if (retryTimer) clearTimeout(retryTimer)
      controller.abort()
    }
  }, [eventId, enabled, accessToken, onProceed, onUpdate])
}
