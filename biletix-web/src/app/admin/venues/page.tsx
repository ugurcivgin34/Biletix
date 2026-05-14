"use client"

import { FormEvent, useState } from "react"
import { useQuery, useQueryClient } from "@tanstack/react-query"
import { MapPin, Pencil, Plus, Trash2 } from "lucide-react"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { LoadingSpinner } from "@/components/common/LoadingSpinner"
import { adminApi, type AdminVenue, type VenuePayload } from "@/lib/api/admin"
import { useToast } from "@/hooks/use-toast"

const emptyForm: VenuePayload = { name: "", city: "", address: "", capacity: 0 }

function getErrorMessage(error: unknown, fallback: string) {
  if (
    typeof error === "object" &&
    error !== null &&
    "response" in error &&
    typeof error.response === "object" &&
    error.response !== null &&
    "data" in error.response
  ) {
    const data = error.response.data as { detail?: string }
    return data.detail ?? fallback
  }

  return fallback
}

export default function AdminVenuesPage() {
  const queryClient = useQueryClient()
  const { toast } = useToast()
  const [showForm, setShowForm] = useState(false)
  const [editId, setEditId] = useState<string | null>(null)
  const [form, setForm] = useState<VenuePayload>(emptyForm)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const { data, isLoading } = useQuery({
    queryKey: ["admin", "venues"],
    queryFn: () => adminApi.getVenues({ pageSize: 100 }),
  })

  const venues: AdminVenue[] = data?.items ?? []

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setIsSubmitting(true)
    try {
      if (editId) {
        await adminApi.updateVenue(editId, form)
        toast({ title: "Mekan güncellendi" })
      } else {
        await adminApi.createVenue(form)
        toast({ title: "Mekan oluşturuldu" })
      }
      queryClient.invalidateQueries({ queryKey: ["admin", "venues"] })
      setShowForm(false)
      setEditId(null)
      setForm(emptyForm)
    } catch (error) {
      toast({
        title: "Hata",
        description: getErrorMessage(error, "İşlem başarısız"),
        variant: "destructive",
      })
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleEdit = (venue: AdminVenue) => {
    setForm({
      name: venue.name,
      city: venue.city,
      address: venue.address,
      capacity: venue.capacity,
    })
    setEditId(venue.id)
    setShowForm(true)
  }

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`"${name}" mekanını silmek istediğinize emin misiniz?`)) return

    try {
      await adminApi.deleteVenue(id)
      toast({ title: "Mekan silindi" })
      queryClient.invalidateQueries({ queryKey: ["admin", "venues"] })
    } catch (error) {
      toast({
        title: "Hata",
        description: getErrorMessage(error, "Silinemedi"),
        variant: "destructive",
      })
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <h1 className="text-2xl font-bold">Mekan Yönetimi</h1>
        <Button
          onClick={() => {
            setShowForm(true)
            setEditId(null)
            setForm(emptyForm)
          }}
          className="gap-2 bg-red-500 hover:bg-red-600"
        >
          <Plus size={16} />
          Yeni Mekan
        </Button>
      </div>

      {showForm && (
        <Card className="border-red-200">
          <CardContent className="p-5">
            <h2 className="mb-4 font-semibold">{editId ? "Mekan Düzenle" : "Yeni Mekan"}</h2>
            <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div className="space-y-1">
                <Label>Mekan Adı</Label>
                <Input
                  placeholder="Volkswagen Arena"
                  value={form.name}
                  onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
                  required
                />
              </div>
              <div className="space-y-1">
                <Label>Şehir</Label>
                <Input
                  placeholder="Istanbul"
                  value={form.city}
                  onChange={(event) => setForm((current) => ({ ...current, city: event.target.value }))}
                  required
                />
              </div>
              <div className="space-y-1 sm:col-span-2">
                <Label>Adres</Label>
                <Input
                  placeholder="Mahalle, İlçe, Şehir"
                  value={form.address}
                  onChange={(event) => setForm((current) => ({ ...current, address: event.target.value }))}
                  required
                />
              </div>
              <div className="space-y-1">
                <Label>Kapasite</Label>
                <Input
                  type="number"
                  placeholder="22000"
                  value={form.capacity || ""}
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      capacity: Number(event.target.value),
                    }))
                  }
                  required
                />
              </div>
              <div className="flex items-end gap-2">
                <Button type="submit" disabled={isSubmitting} className="bg-red-500 hover:bg-red-600">
                  {isSubmitting ? "Kaydediliyor..." : "Kaydet"}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => {
                    setShowForm(false)
                    setEditId(null)
                  }}
                >
                  İptal
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      {isLoading ? (
        <div className="py-12">
          <LoadingSpinner size="lg" />
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          {venues.map((venue) => (
            <Card key={venue.id}>
              <CardContent className="p-4">
                <div className="flex items-start justify-between gap-4">
                  <div className="flex min-w-0 gap-3">
                    <div className="h-fit rounded-lg bg-purple-50 p-2">
                      <MapPin size={16} className="text-purple-500" />
                    </div>
                    <div className="min-w-0">
                      <p className="font-semibold">{venue.name}</p>
                      <p className="text-sm text-gray-500">{venue.city}</p>
                      <p className="mt-1 text-xs text-gray-400">{venue.address}</p>
                      <p className="mt-1 text-xs text-gray-500">
                        Kapasite: {venue.capacity.toLocaleString("tr-TR")}
                      </p>
                    </div>
                  </div>
                  <div className="flex flex-shrink-0 gap-1">
                    <button
                      type="button"
                      aria-label={`${venue.name} düzenle`}
                      onClick={() => handleEdit(venue)}
                      className="rounded-lg p-1.5 transition-colors hover:bg-gray-100"
                    >
                      <Pencil size={14} className="text-gray-400" />
                    </button>
                    <button
                      type="button"
                      aria-label={`${venue.name} sil`}
                      onClick={() => handleDelete(venue.id, venue.name)}
                      className="rounded-lg p-1.5 transition-colors hover:bg-red-50"
                    >
                      <Trash2 size={14} className="text-red-400" />
                    </button>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
