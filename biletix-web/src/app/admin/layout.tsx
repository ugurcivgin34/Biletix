"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { LayoutDashboard, MapPin, Shield, Users } from "lucide-react"
import { Header } from "@/components/layout/Header"
import { useAuthStore } from "@/lib/stores/authStore"

const navItems = [
  { href: "/admin/dashboard", icon: LayoutDashboard, label: "Dashboard" },
  { href: "/admin/users", icon: Users, label: "Kullanıcılar" },
  { href: "/admin/venues", icon: MapPin, label: "Mekanlar" },
]

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const { user, isAuthenticated } = useAuthStore()
  const router = useRouter()
  const pathname = usePathname()
  const [hasHydrated, setHasHydrated] = useState(false)

  useEffect(() => {
    setHasHydrated(useAuthStore.persist.hasHydrated())
    const unsubscribe = useAuthStore.persist.onFinishHydration(() => setHasHydrated(true))

    return unsubscribe
  }, [])

  useEffect(() => {
    if (!hasHydrated) return

    if (!isAuthenticated) {
      router.push("/login")
      return
    }

    if (user?.role !== "Admin") {
      router.push("/")
    }
  }, [hasHydrated, isAuthenticated, router, user])

  if (!hasHydrated) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex-1 p-6" />
      </div>
    )
  }

  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <div className="flex flex-1 pb-16 md:pb-0">
        <aside className="hidden w-56 border-r bg-gray-50 px-3 pt-6 md:block">
          <div className="mb-4 flex items-center gap-2 px-3">
            <Shield size={16} className="text-red-500" />
            <span className="text-sm font-semibold text-gray-700">Admin Panel</span>
          </div>
          <nav className="space-y-1">
            {navItems.map(({ href, icon: Icon, label }) => (
              <Link
                key={href}
                href={href}
                className={`flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition-colors ${
                  pathname === href
                    ? "bg-red-50 font-medium text-red-600"
                    : "text-gray-600 hover:bg-gray-100"
                }`}
              >
                <Icon size={16} />
                {label}
              </Link>
            ))}
          </nav>
        </aside>
        <main className="flex-1 overflow-auto p-6">{children}</main>
      </div>

      <nav className="fixed inset-x-0 bottom-0 z-40 grid grid-cols-3 border-t bg-white md:hidden">
        {navItems.map(({ href, icon: Icon, label }) => (
          <Link
            key={href}
            href={href}
            className={`flex flex-col items-center gap-1 px-2 py-2 text-[11px] transition-colors ${
              pathname === href ? "text-red-600" : "text-gray-500"
            }`}
          >
            <Icon size={18} />
            <span className="truncate">{label}</span>
          </Link>
        ))}
      </nav>
    </div>
  )
}

