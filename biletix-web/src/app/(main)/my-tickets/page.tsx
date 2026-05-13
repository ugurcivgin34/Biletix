import { TicketCheck } from "lucide-react"
import { EmptyState } from "@/components/common/EmptyState"

export default function MyTicketsPage() {
  return (
    <EmptyState
      icon={TicketCheck}
      title="Biletlerim"
      description="Satın alınan ve bekleyen biletler burada görüntülenecek."
    />
  )
}

