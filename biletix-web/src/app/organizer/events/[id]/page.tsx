import { Settings } from "lucide-react"
import { EmptyState } from "@/components/common/EmptyState"

export default function OrganizerEventDetailPage({ params }: { params: { id: string } }) {
  return (
    <EmptyState
      icon={Settings}
      title="Etkinlik ayarları"
      description={`Yönetilen etkinlik ID: ${params.id}`}
    />
  )
}

