import { Gauge } from "lucide-react"
import { EmptyState } from "@/components/common/EmptyState"

export default function OrganizerDashboardPage() {
  return (
    <EmptyState
      icon={Gauge}
      title="Organizatör dashboard"
      description="Etkinlik performansı ve satış özeti burada yer alacak."
    />
  )
}

