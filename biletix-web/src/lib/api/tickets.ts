import { apiClient } from "./client"

export interface ValidateTicketResponse {
  isValid: boolean
  message: string
  bookingId?: string
  eventId?: string
  userId?: string
  attendeeFirstName?: string
  attendeeLastName?: string
  eventTitle?: string
  eventStartDate?: string
  alreadyScanned: boolean
  firstScannedAt?: string
}

export const ticketsApi = {
  validate: async (qrToken: string, scannedBy: string): Promise<ValidateTicketResponse> => {
    const { data } = await apiClient.post<ValidateTicketResponse>("/api/tickets/validate", {
      qrToken,
      scannedBy,
    })
    return data
  },

  getScanHistory: async (eventId: string, params = {}) => {
    const { data } = await apiClient.get(`/api/tickets/scans/${eventId}`, { params })
    return data
  },
}
