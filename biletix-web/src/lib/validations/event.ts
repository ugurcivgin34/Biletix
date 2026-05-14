import { z } from "zod"

export const createEventSchema = z.object({
  title: z.string().min(3, "Başlık en az 3 karakter olmalı").max(300),
  description: z.string().min(10, "Açıklama en az 10 karakter olmalı").max(2000),
  startDate: z.string().min(1, "Başlangıç tarihi gerekli"),
  endDate: z.string().min(1, "Bitiş tarihi gerekli"),
  venueId: z.string().uuid("Mekan seçiniz"),
  performerName: z.string().min(2, "Sanatçı adı gerekli"),
  ticketTypes: z
    .array(
      z.object({
        name: z.string().min(1, "Bilet tipi adı gerekli"),
        price: z.number().min(0, "Fiyat 0 veya üzeri olmalı"),
        totalCapacity: z.number().min(1, "Kapasite en az 1 olmalı"),
      })
    )
    .min(1, "En az bir bilet tipi ekleyin"),
})

export const eventSchema = createEventSchema
export type CreateEventFormData = z.infer<typeof createEventSchema>
export type EventFormValues = z.infer<typeof eventSchema>

