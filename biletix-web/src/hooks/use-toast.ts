"use client"

import * as React from "react"

type ToastVariant = "default" | "destructive"

export interface Toast {
  id: string
  title?: string
  description?: string
  variant?: ToastVariant
}

type ToastInput = Omit<Toast, "id">
type Listener = (toasts: Toast[]) => void

let memoryState: Toast[] = []
const listeners: Listener[] = []

function emit() {
  listeners.forEach((listener) => listener(memoryState))
}

export function toast(input: ToastInput) {
  const id = crypto.randomUUID()
  memoryState = [{ id, ...input }, ...memoryState].slice(0, 3)
  emit()
  window.setTimeout(() => {
    memoryState = memoryState.filter((item) => item.id !== id)
    emit()
  }, 4000)
}

export function useToast() {
  const [toasts, setToasts] = React.useState<Toast[]>(memoryState)

  React.useEffect(() => {
    listeners.push(setToasts)
    return () => {
      const index = listeners.indexOf(setToasts)
      if (index > -1) listeners.splice(index, 1)
    }
  }, [])

  return { toast, toasts }
}

