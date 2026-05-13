import Link from "next/link"
import { CalendarDays } from "lucide-react"
import { Button } from "@/components/ui/button"
import { EmptyState } from "@/components/common/EmptyState"

export default function OrganizerEventsPage() {
  return (
    <EmptyState
      icon={CalendarDays}
      title="Etkinlik yönetimi"
      description="Oluşturduğunuz etkinlikler burada listelenecek."
      action={
        <Link href="/organizer/events/new">
          <Button>Yeni Etkinlik</Button>
        </Link>
      }
    />
  )
}

