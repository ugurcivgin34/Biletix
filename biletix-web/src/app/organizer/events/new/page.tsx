"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { zodResolver } from "@hookform/resolvers/zod"
import axios from "axios"
import { ArrowLeft, Loader2 } from "lucide-react"
import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { TicketTypeFields } from "@/components/organizer/TicketTypeFields"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { useToast } from "@/hooks/use-toast"
import { organizerApi } from "@/lib/api/organizer"
import { createEventSchema, type CreateEventFormData } from "@/lib/validations/event"

interface Venue {
  id: string
  name: string
  city: string
  capacity: number
}

function getCreateEventErrorMessage(error: unknown) {
  if (axios.isAxiosError<{ detail?: string; errors?: unknown }>(error)) {
    return error.response?.data?.detail || error.response?.data?.errors || "Etkinlik oluşturulamadı"
  }

  return "Etkinlik oluşturulamadı"
}

export default function NewEventPage() {
  const router = useRouter()
  const { toast } = useToast()
  const [venues, setVenues] = useState<Venue[]>([])
  const [isSubmitting, setIsSubmitting] = useState(false)

  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateEventFormData>({
    resolver: zodResolver(createEventSchema),
    defaultValues: {
      title: "",
      description: "",
      startDate: "",
      endDate: "",
      venueId: "",
      performerName: "",
      ticketTypes: [{ name: "Standart", price: 500, totalCapacity: 1000 }],
    },
  })

  useEffect(() => {
    organizerApi
      .getVenues()
      .then((data) => setVenues(data.items ?? []))
      .catch(() => {})
  }, [])

  const onSubmit = async (formData: CreateEventFormData) => {
    setIsSubmitting(true)

    try {
      let performerId: string

      const performers = await organizerApi.getPerformers()
      const existing = performers.items?.find(
        (performer: { id: string; name: string }) =>
          performer.name.toLowerCase() === formData.performerName.toLowerCase()
      )

      if (existing) {
        performerId = existing.id
      } else {
        const newPerformer = await organizerApi.createPerformer(formData.performerName, "Diğer")
        performerId = newPerformer.id ?? newPerformer
      }

      await organizerApi.createEvent({
        title: formData.title,
        description: formData.description,
        startDate: new Date(formData.startDate).toISOString(),
        endDate: new Date(formData.endDate).toISOString(),
        venueId: formData.venueId,
        performerId,
        ticketTypes: formData.ticketTypes,
      })

      toast({
        title: "Etkinlik oluşturuldu!",
        description: "Etkinliği yayınlamak için dashboard'dan Yayınla butonuna basın.",
      })
      router.push("/organizer/dashboard")
    } catch (error) {
      const message = getCreateEventErrorMessage(error)
      toast({ title: "Hata", description: String(message), variant: "destructive" })
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center gap-3">
        <Link href="/organizer/dashboard">
          <Button variant="ghost" size="sm" className="gap-1.5">
            <ArrowLeft size={16} />
            Geri
          </Button>
        </Link>
        <h1 className="text-2xl font-bold">Yeni Etkinlik</h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Temel Bilgiler</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-1">
              <Label htmlFor="title">Etkinlik Adı</Label>
              <Input
                id="title"
                placeholder="Tarkan İstanbul Konseri 2026"
                {...register("title")}
              />
              {errors.title && <p className="text-xs text-red-500">{errors.title.message}</p>}
            </div>

            <div className="space-y-1">
              <Label htmlFor="description">Açıklama</Label>
              <Textarea
                id="description"
                placeholder="Etkinlik hakkında detaylı bilgi..."
                rows={4}
                {...register("description")}
              />
              {errors.description && (
                <p className="text-xs text-red-500">{errors.description.message}</p>
              )}
            </div>

            <div className="space-y-1">
              <Label htmlFor="performerName">Sanatçı / Performans</Label>
              <Input id="performerName" placeholder="Tarkan" {...register("performerName")} />
              {errors.performerName && (
                <p className="text-xs text-red-500">{errors.performerName.message}</p>
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Tarih ve Mekan</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div className="space-y-1">
                <Label htmlFor="startDate">Başlangıç</Label>
                <Input id="startDate" type="datetime-local" {...register("startDate")} />
                {errors.startDate && (
                  <p className="text-xs text-red-500">{errors.startDate.message}</p>
                )}
              </div>
              <div className="space-y-1">
                <Label htmlFor="endDate">Bitiş</Label>
                <Input id="endDate" type="datetime-local" {...register("endDate")} />
                {errors.endDate && (
                  <p className="text-xs text-red-500">{errors.endDate.message}</p>
                )}
              </div>
            </div>

            <div className="space-y-1">
              <Label htmlFor="venueId">Mekan</Label>
              <select
                id="venueId"
                {...register("venueId")}
                className="h-10 w-full rounded-lg border border-input bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
              >
                <option value="">Mekan seçin...</option>
                {venues.map((venue) => (
                  <option key={venue.id} value={venue.id}>
                    {venue.name} - {venue.city} ({venue.capacity.toLocaleString("tr-TR")} kişi)
                  </option>
                ))}
              </select>
              {errors.venueId && <p className="text-xs text-red-500">{errors.venueId.message}</p>}
              {venues.length === 0 && (
                <p className="text-xs text-orange-500">
                  Mekan bulunamadı. Admin&apos;den mekan eklenmesini isteyin.
                </p>
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Bilet Tipleri</CardTitle>
          </CardHeader>
          <CardContent>
            <TicketTypeFields control={control} register={register} errors={errors} />
          </CardContent>
        </Card>

        <div className="flex gap-3">
          <Link href="/organizer/dashboard" className="flex-1">
            <Button type="button" variant="outline" className="w-full">
              İptal
            </Button>
          </Link>
          <Button
            type="submit"
            disabled={isSubmitting}
            className="flex-1 bg-red-500 hover:bg-red-600"
          >
            {isSubmitting ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Oluşturuluyor...
              </>
            ) : (
              "Etkinlik Oluştur"
            )}
          </Button>
        </div>
      </form>
    </div>
  )
}

