import { create } from "zustand"
import { persist } from "zustand/middleware"
import type { User } from "@/lib/types/auth"

const authCookieMaxAge = 60 * 60 * 24 * 7

export function setAccessTokenCookie(accessToken: string) {
  document.cookie = `accessToken=${accessToken}; path=/; max-age=${authCookieMaxAge}; SameSite=Lax`
}

export function clearAccessTokenCookie() {
  document.cookie = "accessToken=; path=/; max-age=0; SameSite=Lax"
}

interface AuthState {
  user: User | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  setAuth: (user: User, accessToken: string, refreshToken: string) => void
  clearAuth: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      setAuth: (user, accessToken, refreshToken) => {
        if (typeof window !== "undefined") {
          localStorage.setItem("accessToken", accessToken)
          localStorage.setItem("refreshToken", refreshToken)
          setAccessTokenCookie(accessToken)
        }

        set({ user, accessToken, refreshToken, isAuthenticated: true })
      },
      clearAuth: () => {
        if (typeof window !== "undefined") {
          localStorage.removeItem("accessToken")
          localStorage.removeItem("refreshToken")
          clearAccessTokenCookie()
        }

        set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false })
      },
    }),
    { name: "biletix-auth" }
  )
)
