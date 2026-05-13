"use client"

import Link from "next/link"
import { Ticket } from "lucide-react"
import { Button } from "@/components/ui/button"
import { useLogout } from "@/lib/hooks/useAuth"
import { useAuthStore } from "@/lib/stores/authStore"

export function Header() {
  const { user, isAuthenticated } = useAuthStore()
  const logout = useLogout()

  return (
    <header className="sticky top-0 z-50 border-b bg-white">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        <Link href="/" className="flex items-center gap-2 text-xl font-bold">
          <Ticket className="text-red-500" size={24} />
          <span>Biletix</span>
        </Link>

        <nav className="hidden items-center gap-6 text-sm md:flex">
          <Link href="/events" className="transition-colors hover:text-red-500">
            Etkinlikler
          </Link>
          {isAuthenticated && (
            <Link href="/my-tickets" className="transition-colors hover:text-red-500">
              Biletlerim
            </Link>
          )}
          {user?.role === "Organizer" || user?.role === "Admin" ? (
            <Link href="/organizer/dashboard" className="transition-colors hover:text-red-500">
              Panel
            </Link>
          ) : null}
        </nav>

        <div className="flex items-center gap-3">
          {isAuthenticated ? (
            <div className="flex items-center gap-3">
              <span className="hidden text-sm text-gray-600 sm:inline">{user?.firstName}</span>
              <Button variant="outline" size="sm" onClick={() => void logout()}>
                Çıkış
              </Button>
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <Link href="/login">
                <Button variant="outline" size="sm">
                  Giriş
                </Button>
              </Link>
              <Link href="/register">
                <Button size="sm" className="bg-red-500 hover:bg-red-600">
                  Kayıt Ol
                </Button>
              </Link>
            </div>
          )}
        </div>
      </div>
    </header>
  )
}

