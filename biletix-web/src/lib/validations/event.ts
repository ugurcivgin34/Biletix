import { z } from "zod"

export const eventSchema = z.object({
  title: z.string().min(3, "Etkinlik adı en az 3 karakter olmalı"),
  description: z.string().min(10, "Açıklama en az 10 karakter olmalı"),
  startDate: z.string().min(1, "Başlangıç tarihi zorunlu"),
  endDate: z.string().min(1, "Bitiş tarihi zorunlu"),
  venueId: z.string().uuid("Geçerli bir mekan seçin"),
  performerId: z.string().uuid("Geçerli bir sanatçı seçin"),
})

export type EventFormValues = z.infer<typeof eventSchema>

