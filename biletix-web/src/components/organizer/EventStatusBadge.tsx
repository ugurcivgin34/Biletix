import { Badge } from "@/components/ui/badge"

const config = {
  Draft: { label: "Taslak", className: "bg-gray-100 text-gray-600" },
  Published: { label: "Yayında", className: "bg-green-100 text-green-700" },
  Cancelled: { label: "İptal", className: "bg-red-100 text-red-700" },
  Completed: { label: "Tamamlandı", className: "bg-blue-100 text-blue-700" },
}

export function EventStatusBadge({ status }: { status: string }) {
  const current = config[status as keyof typeof config] ?? config.Draft

  return (
    <Badge variant="outline" className={`border-transparent ${current.className}`}>
      {current.label}
    </Badge>
  )
}
