import type { MetadataRoute } from "next"

interface SitemapEvent {
  id: string
  createdAt?: string
  updatedAt?: string
}

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const baseUrl = process.env.NEXT_PUBLIC_APP_URL || "http://localhost:3001"

  const staticRoutes: MetadataRoute.Sitemap = [
    { url: baseUrl, lastModified: new Date(), changeFrequency: "daily", priority: 1 },
    { url: `${baseUrl}/events`, lastModified: new Date(), changeFrequency: "hourly", priority: 0.9 },
    { url: `${baseUrl}/login`, lastModified: new Date(), changeFrequency: "monthly", priority: 0.3 },
    { url: `${baseUrl}/register`, lastModified: new Date(), changeFrequency: "monthly", priority: 0.3 },
  ]

  try {
    const apiUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5157"
    const response = await fetch(`${apiUrl}/api/search/events?pageSize=100`, {
      next: { revalidate: 300 },
    })

    if (response.ok) {
      const data = await response.json()
      const eventRoutes: MetadataRoute.Sitemap = ((data.items ?? []) as SitemapEvent[]).map((event) => ({
        url: `${baseUrl}/events/${event.id}`,
        lastModified: new Date(event.updatedAt ?? event.createdAt ?? Date.now()),
        changeFrequency: "daily" as const,
        priority: 0.8,
      }))

      return [...staticRoutes, ...eventRoutes]
    }
  } catch {}

  return staticRoutes
}
