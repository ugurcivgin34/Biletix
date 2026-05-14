import { apiClient } from "./client"

export interface AdminUser {
  id: string
  email: string
  firstName: string
  lastName: string
  role: "Admin" | "Organizer" | "Customer"
  isActive: boolean
  createdAt: string
}

export interface AdminVenue {
  id: string
  name: string
  city: string
  address: string
  capacity: number
}

export interface VenuePayload {
  name: string
  city: string
  address: string
  capacity: number
}

export const adminApi = {
  getUsers: async (params: { page?: number; pageSize?: number; role?: string } = {}) => {
    const { data } = await apiClient.get("/api/admin/users", { params })
    return data
  },

  updateUserRole: async (userId: string, role: string) => {
    const { data } = await apiClient.patch(`/api/admin/users/${userId}/role`, { role })
    return data
  },

  getVenues: async (params = {}) => {
    const { data } = await apiClient.get("/api/venues", { params })
    return data
  },

  createVenue: async (payload: VenuePayload) => {
    const { data } = await apiClient.post("/api/venues", payload)
    return data
  },

  updateVenue: async (id: string, payload: VenuePayload) => {
    await apiClient.put(`/api/venues/${id}`, payload)
  },

  deleteVenue: async (id: string) => {
    await apiClient.delete(`/api/venues/${id}`)
  },
}
