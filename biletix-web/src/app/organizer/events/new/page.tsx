import { CalendarPlus } from "lucide-react"
import { EmptyState } from "@/components/common/EmptyState"

export default function NewOrganizerEventPage() {
  return (
    <EmptyState
      icon={CalendarPlus}
      title="Yeni etkinlik"
      description="Etkinlik oluşturma formu burada yer alacak."
    />
  )
}

