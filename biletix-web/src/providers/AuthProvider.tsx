"use client"

import { useEffect } from "react"
import { clearAccessTokenCookie, setAccessTokenCookie, useAuthStore } from "@/lib/stores/authStore"

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const accessToken = useAuthStore((state) => state.accessToken)
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)

  useEffect(() => {
    if (isAuthenticated && accessToken) {
      setAccessTokenCookie(accessToken)
      return
    }

    clearAccessTokenCookie()
  }, [accessToken, isAuthenticated])

  return <>{children}</>
}

