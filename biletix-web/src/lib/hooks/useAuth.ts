"use client"

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { useRouter } from "next/navigation"
import { authApi } from "@/lib/api/auth"
import { useAuthStore } from "@/lib/stores/authStore"
import type { RegisterRequest, User } from "@/lib/types/auth"

function getRedirectPath(role: string) {
  if (typeof window !== "undefined") {
    const from = new URLSearchParams(window.location.search).get("from")
    if (from?.startsWith("/")) return from
  }

  if (role === "Admin") return "/admin/dashboard"
  if (role === "Organizer") return "/organizer/dashboard"
  return "/"
}

export function useMe() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)

  return useQuery({
    queryKey: ["auth", "me"],
    queryFn: authApi.getMe,
    enabled: isAuthenticated,
  })
}

export function useLogin() {
  const { setAuth } = useAuthStore()
  const router = useRouter()

  return useMutation({
    mutationFn: ({ email, password }: { email: string; password: string }) =>
      authApi.login(email, password),
    onSuccess: (data) => {
      const [firstName = "", ...lastNameParts] = data.fullName.split(" ")
      const user: User = {
        userId: data.userId,
        email: data.email,
        fullName: data.fullName,
        firstName,
        lastName: lastNameParts.join(" "),
        role: data.role as User["role"],
      }

      setAuth(user, data.accessToken, data.refreshToken)
      router.push(getRedirectPath(data.role))
    },
  })
}

export function useRegister() {
  const router = useRouter()

  return useMutation({
    mutationFn: (request: RegisterRequest) => authApi.register(request),
    onSuccess: () => {
      router.push("/login?registered=true")
    },
  })
}

export function useLogout() {
  const { clearAuth, refreshToken } = useAuthStore()
  const router = useRouter()
  const queryClient = useQueryClient()

  return async () => {
    try {
      if (refreshToken) await authApi.logout(refreshToken)
    } catch {
      // Local logout should still complete when the server token is already gone.
    } finally {
      clearAuth()
      queryClient.clear()
      router.push("/")
    }
  }
}

