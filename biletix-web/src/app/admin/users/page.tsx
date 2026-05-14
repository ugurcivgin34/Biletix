"use client"

import { useState } from "react"
import { useQuery, useQueryClient } from "@tanstack/react-query"
import { User } from "lucide-react"
import { Card, CardContent } from "@/components/ui/card"
import { LoadingSpinner } from "@/components/common/LoadingSpinner"
import { Pagination } from "@/components/common/Pagination"
import { adminApi, type AdminUser } from "@/lib/api/admin"
import { useToast } from "@/hooks/use-toast"
import { useAuthStore } from "@/lib/stores/authStore"

const roles = ["Customer", "Organizer", "Admin"] as const

const roleColors: Record<string, string> = {
  Admin: "bg-red-100 text-red-700",
  Organizer: "bg-purple-100 text-purple-700",
  Customer: "bg-blue-100 text-blue-700",
}

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

export default function AdminUsersPage() {
  const [page, setPage] = useState(1)
  const [updating, setUpdating] = useState<string | null>(null)
  const { toast } = useToast()
  const queryClient = useQueryClient()
  const { user: currentUser } = useAuthStore()

  const { data, isLoading } = useQuery({
    queryKey: ["admin", "users", page],
    queryFn: () => adminApi.getUsers({ page, pageSize: 20 }),
  })

  const users: AdminUser[] = data?.items ?? []

  const handleRoleChange = async (userId: string, newRole: string) => {
    setUpdating(userId)
    try {
      await adminApi.updateUserRole(userId, newRole)
      toast({ title: "Rol güncellendi" })
      queryClient.invalidateQueries({ queryKey: ["admin", "users"] })
    } catch (error) {
      toast({
        title: "Hata",
        description: getErrorMessage(error, "Rol güncellenemedi"),
        variant: "destructive",
      })
    } finally {
      setUpdating(null)
    }
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Kullanıcı Yönetimi</h1>

      {isLoading ? (
        <div className="py-12">
          <LoadingSpinner size="lg" />
        </div>
      ) : (
        <div className="space-y-3">
          {users.map((user) => (
            <Card key={user.id}>
              <CardContent className="p-4">
                <div className="flex items-center justify-between gap-4">
                  <div className="flex min-w-0 items-center gap-3">
                    <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-full bg-gray-100">
                      <User size={18} className="text-gray-400" />
                    </div>
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium">
                        {user.firstName} {user.lastName}
                      </p>
                      <p className="truncate text-xs text-gray-500">{user.email}</p>
                    </div>
                  </div>

                  <div className="flex flex-shrink-0 items-center gap-3">
                    <span
                      className={`rounded-full px-2 py-0.5 text-xs font-medium ${roleColors[user.role]}`}
                    >
                      {user.role}
                    </span>

                    {user.id !== currentUser?.userId && (
                      <select
                        value={user.role}
                        disabled={updating === user.id}
                        onChange={(event) => handleRoleChange(user.id, event.target.value)}
                        className="rounded-lg border bg-white px-2 py-1.5 text-xs focus:outline-none focus:ring-1 focus:ring-red-500"
                      >
                        {roles.map((role) => (
                          <option key={role} value={role}>
                            {role}
                          </option>
                        ))}
                      </select>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {data && data.totalPages > 1 && (
        <Pagination currentPage={page} totalPages={data.totalPages} onPageChange={setPage} />
      )}
    </div>
  )
}
