"use client"

import { useToast } from "@/hooks/use-toast"
import { cn } from "@/lib/utils"

export function Toaster() {
  const { toasts } = useToast()

  return (
    <div className="fixed bottom-4 right-4 z-[100] flex w-[calc(100%-2rem)] max-w-sm flex-col gap-2">
      {toasts.map((toast) => (
        <div
          key={toast.id}
          className={cn(
            "rounded-lg border bg-white p-4 text-sm shadow-lg",
            toast.variant === "destructive" && "border-red-200 bg-red-50 text-red-900"
          )}
        >
          {toast.title && <div className="font-semibold">{toast.title}</div>}
          {toast.description && <div className="mt-1 text-gray-600">{toast.description}</div>}
        </div>
      ))}
    </div>
  )
}
