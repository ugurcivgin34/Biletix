"use client"

import { Mail, Shield, User } from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useMyBookings } from "@/lib/hooks/useBookings"
import { useAuthStore } from "@/lib/stores/authStore"

const roleLabels = {
  Admin: { label: "Admin", className: "bg-red-100 text-red-700" },
  Organizer: { label: "Organizatör", className: "bg-purple-100 text-purple-700" },
  Customer: { label: "Müşteri", className: "bg-blue-100 text-blue-700" },
}

export default function ProfilePage() {
  const { user } = useAuthStore()
  const { data } = useMyBookings()

  const bookings = data?.items ?? []
  const confirmedBookings = bookings.filter((booking) => booking.status === "Confirmed")
  const confirmedCount = confirmedBookings.length
  const totalSpent = confirmedBookings.reduce((sum, booking) => sum + booking.totalAmount, 0)

  if (!user) return null

  const roleConfig = roleLabels[user.role] ?? roleLabels.Customer
  const initials = `${user.firstName[0] ?? ""}${user.lastName?.[0] ?? ""}`.toUpperCase()

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <h1 className="text-2xl font-bold">Profilim</h1>

      <Card>
        <CardContent className="p-6">
          <div className="flex items-start gap-4">
            <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-full bg-red-100">
              <span className="text-2xl font-bold text-red-500">{initials}</span>
            </div>
            <div className="min-w-0 flex-1">
              <div className="mb-1 flex flex-wrap items-center gap-2">
                <h2 className="text-xl font-semibold">{user.fullName}</h2>
                <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${roleConfig.className}`}>
                  {roleConfig.label}
                </span>
              </div>
              <div className="flex items-center gap-1.5 text-sm text-gray-500">
                <Mail size={14} />
                <span className="break-all">{user.email}</span>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-2 gap-4">
        <Card>
          <CardContent className="p-5 text-center">
            <p className="text-3xl font-bold text-red-500">{confirmedCount}</p>
            <p className="mt-1 text-sm text-gray-500">Onaylı Bilet</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-5 text-center">
            <p className="text-3xl font-bold text-red-500">
              {totalSpent > 0
                ? new Intl.NumberFormat("tr-TR", {
                    style: "currency",
                    currency: "TRY",
                    maximumFractionDigits: 0,
                  }).format(totalSpent)
                : "₺0"}
            </p>
            <p className="mt-1 text-sm text-gray-500">Toplam Harcama</p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Hesap Bilgileri</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <div className="flex items-center gap-3 text-sm">
            <User size={16} className="text-gray-400" />
            <span className="w-24 text-gray-500">Ad Soyad</span>
            <span className="font-medium">{user.fullName}</span>
          </div>
          <div className="flex items-center gap-3 text-sm">
            <Mail size={16} className="text-gray-400" />
            <span className="w-24 text-gray-500">E-posta</span>
            <span className="min-w-0 break-all font-medium">{user.email}</span>
          </div>
          <div className="flex items-center gap-3 text-sm">
            <Shield size={16} className="text-gray-400" />
            <span className="w-24 text-gray-500">Rol</span>
            <span className="font-medium">{roleConfig.label}</span>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

