"use client"

import { useQuery } from "@tanstack/react-query"
import { Calendar, MapPin, Users } from "lucide-react"
import { Card, CardContent } from "@/components/ui/card"
import { adminApi } from "@/lib/api/admin"
import { eventsApi } from "@/lib/api/events"

export default function AdminDashboardPage() {
  const { data: usersData } = useQuery({
    queryKey: ["admin", "users"],
    queryFn: () => adminApi.getUsers({ pageSize: 1 }),
  })

  const { data: eventsData } = useQuery({
    queryKey: ["admin", "events"],
    queryFn: () => eventsApi.getAll({ pageSize: 1 }),
  })

  const { data: venuesData } = useQuery({
    queryKey: ["admin", "venues"],
    queryFn: () => adminApi.getVenues({ pageSize: 1 }),
  })

  const stats = [
    {
      icon: Users,
      label: "Toplam Kullanıcı",
      value: usersData?.totalCount ?? "—",
      color: "text-blue-500",
      bg: "bg-blue-50",
    },
    {
      icon: Calendar,
      label: "Toplam Etkinlik",
      value: eventsData?.totalCount ?? "—",
      color: "text-green-500",
      bg: "bg-green-50",
    },
    {
      icon: MapPin,
      label: "Toplam Mekan",
      value: venuesData?.totalCount ?? "—",
      color: "text-purple-500",
      bg: "bg-purple-50",
    },
  ]

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold">Admin Dashboard</h1>
        <p className="mt-1 text-sm text-gray-500">Sistem genel bakışı</p>
      </div>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        {stats.map(({ icon: Icon, label, value, color, bg }) => (
          <Card key={label}>
            <CardContent className="p-6">
              <div className="flex items-center gap-4">
                <div className={`rounded-xl p-3 ${bg}`}>
                  <Icon size={24} className={color} />
                </div>
                <div>
                  <p className="text-3xl font-bold">{value}</p>
                  <p className="text-sm text-gray-500">{label}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}

