import axios from "axios"

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5157"

function getStoredToken() {
  if (typeof window === "undefined") return null

  const directToken = localStorage.getItem("accessToken")
  if (directToken) return directToken

  const persisted = localStorage.getItem("biletix-auth")
  if (!persisted) return null

  try {
    return JSON.parse(persisted).state?.accessToken ?? null
  } catch {
    return null
  }
}

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: { "Content-Type": "application/json" },
})

apiClient.interceptors.request.use((config) => {
  const token = getStoredToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (typeof window !== "undefined" && error.response?.status === 401) {
      localStorage.removeItem("accessToken")
      localStorage.removeItem("refreshToken")
      localStorage.removeItem("biletix-auth")
      window.location.href = "/login"
    }
    return Promise.reject(error)
  }
)

