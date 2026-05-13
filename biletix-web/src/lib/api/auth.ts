import { apiClient } from "./client"
import type { LoginResponse, RegisterRequest, User } from "@/lib/types/auth"

export const authApi = {
  login: async (email: string, password: string): Promise<LoginResponse> => {
    const { data } = await apiClient.post("/api/auth/login", { email, password })
    return data
  },

  register: async (request: RegisterRequest): Promise<{ userId: string; email: string }> => {
    const { data } = await apiClient.post("/api/auth/register", request)
    return data
  },

  logout: async (refreshToken: string): Promise<void> => {
    await apiClient.post("/api/auth/logout", { refreshToken })
  },

  refreshToken: async (accessToken: string, refreshToken: string): Promise<LoginResponse> => {
    const { data } = await apiClient.post("/api/auth/refresh", { accessToken, refreshToken })
    return data
  },

  getMe: async (): Promise<User> => {
    const { data } = await apiClient.get("/api/auth/me")
    return data
  },
}

