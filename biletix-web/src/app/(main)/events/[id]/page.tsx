import type { Metadata } from "next"
import { EventDetailClient } from "./EventDetailClient"

interface Props {
  params: { id: string }
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  try {
    const apiUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5157"
    const response = await fetch(`${apiUrl}/api/events/${params.id}`, {
      next: { revalidate: 60 },
    })

    if (!response.ok) return { title: "Etkinlik" }

    const event = await response.json()
    const description = event.description?.substring(0, 160)

    return {
      title: event.title,
      description,
      openGraph: {
        title: event.title,
        description,
      },
    }
  } catch {
    return { title: "Etkinlik" }
  }
}

export default function EventDetailPage({ params }: Props) {
  return <EventDetailClient id={params.id} />
}
