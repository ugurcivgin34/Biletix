import Link from "next/link"
import { CalendarDays, Gauge, MapPin, Users } from "lucide-react"

const links = [
  { href: "/organizer/dashboard", label: "Dashboard", icon: Gauge },
  { href: "/organizer/events", label: "Etkinlikler", icon: CalendarDays },
  { href: "/admin/dashboard", label: "Admin", icon: Users },
  { href: "/events", label: "Keşfet", icon: MapPin },
]

export function Sidebar() {
  return (
    <aside className="hidden w-64 border-r bg-white lg:block">
      <nav className="sticky top-16 space-y-1 p-4">
        {links.map((link) => {
          const Icon = link.icon

          return (
            <Link
              key={link.href}
              href={link.href}
              className="flex items-center gap-3 rounded-md px-3 py-2 text-sm text-gray-600 transition-colors hover:bg-gray-100 hover:text-gray-950"
            >
              <Icon className="h-4 w-4" />
              {link.label}
            </Link>
          )
        })}
      </nav>
    </aside>
  )
}

