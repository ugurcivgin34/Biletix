"use client"

import { useState } from "react"
import Link from "next/link"
import { Menu, Ticket, X } from "lucide-react"
import { Button } from "@/components/ui/button"
import { useLogout } from "@/lib/hooks/useAuth"
import { useAuthStore } from "@/lib/stores/authStore"

export function Header() {
  const { user, isAuthenticated } = useAuthStore()
  const logout = useLogout()
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)

  const closeMobileMenu = () => setMobileMenuOpen(false)
  const panelHref = user?.role === "Admin" ? "/admin/dashboard" : "/organizer/dashboard"

  return (
    <header className="sticky top-0 z-50 border-b bg-white">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        <Link href="/" className="flex items-center gap-2 text-xl font-bold" onClick={closeMobileMenu}>
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
            <Link href={panelHref} className="transition-colors hover:text-red-500">
              Panel
            </Link>
          ) : null}
        </nav>

        <div className="flex items-center gap-3">
          {isAuthenticated ? (
            <div className="flex items-center gap-3">
              <span className="hidden text-sm text-gray-600 sm:inline">{user?.firstName}</span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  closeMobileMenu()
                  void logout()
                }}
                className="hidden sm:inline-flex"
              >
                Çıkış
              </Button>
            </div>
          ) : (
            <div className="hidden items-center gap-2 sm:flex">
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

          <button
            type="button"
            aria-label={mobileMenuOpen ? "Menüyü kapat" : "Menüyü aç"}
            onClick={() => setMobileMenuOpen((open) => !open)}
            className="rounded-lg p-2 text-gray-600 transition-colors hover:bg-gray-100 md:hidden"
          >
            {mobileMenuOpen ? <X size={22} /> : <Menu size={22} />}
          </button>
        </div>
      </div>

      {mobileMenuOpen && (
        <div className="border-t bg-white px-4 py-3 shadow-lg md:hidden">
          <nav className="flex flex-col gap-1 text-sm">
            <Link href="/events" onClick={closeMobileMenu} className="rounded-lg px-3 py-2 hover:bg-gray-50">
              Etkinlikler
            </Link>
            {isAuthenticated && (
              <Link href="/my-tickets" onClick={closeMobileMenu} className="rounded-lg px-3 py-2 hover:bg-gray-50">
                Biletlerim
              </Link>
            )}
            {(user?.role === "Organizer" || user?.role === "Admin") && (
              <Link href={panelHref} onClick={closeMobileMenu} className="rounded-lg px-3 py-2 hover:bg-gray-50">
                Panel
              </Link>
            )}
          </nav>

          <div className="mt-3 border-t pt-3">
            {isAuthenticated ? (
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  closeMobileMenu()
                  void logout()
                }}
                className="w-full"
              >
                Çıkış
              </Button>
            ) : (
              <div className="grid grid-cols-2 gap-2">
                <Link href="/login" onClick={closeMobileMenu}>
                  <Button variant="outline" size="sm" className="w-full">
                    Giriş
                  </Button>
                </Link>
                <Link href="/register" onClick={closeMobileMenu}>
                  <Button size="sm" className="w-full bg-red-500 hover:bg-red-600">
                    Kayıt Ol
                  </Button>
                </Link>
              </div>
            )}
          </div>
        </div>
      )}
    </header>
  )
}

