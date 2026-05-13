"use client"

import { Suspense, useState } from "react"
import { useRouter, useSearchParams } from "next/navigation"
import { SlidersHorizontal, X } from "lucide-react"
import { Pagination } from "@/components/common/Pagination"
import { EventsGrid } from "@/components/events/EventsGrid"
import { SearchBar } from "@/components/events/SearchBar"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { useSearchEvents } from "@/lib/hooks/useEvents"

type SortBy = "startDate" | "price" | "relevance"

function EventsPageContent() {
  const searchParams = useSearchParams()
  const router = useRouter()
  const [showFilters, setShowFilters] = useState(false)

  const [filters, setFilters] = useState({
    q: searchParams.get("q") || "",
    city: searchParams.get("city") || "",
    minPrice: searchParams.get("minPrice") || "",
    maxPrice: searchParams.get("maxPrice") || "",
    sortBy: (searchParams.get("sortBy") || "startDate") as SortBy,
    page: Number(searchParams.get("page")) || 1,
  })

  const { data, isLoading } = useSearchEvents({
    q: filters.q || undefined,
    city: filters.city || undefined,
    minPrice: filters.minPrice ? Number(filters.minPrice) : undefined,
    maxPrice: filters.maxPrice ? Number(filters.maxPrice) : undefined,
    sortBy: filters.sortBy,
    page: filters.page,
    pageSize: 12,
  })

  const events = data?.items ?? []
  const totalPages = data?.totalPages ?? 1
  const hasFilters = Boolean(filters.q || filters.city || filters.minPrice || filters.maxPrice)

  const clearFilters = () => {
    setFilters({ q: "", city: "", minPrice: "", maxPrice: "", sortBy: "startDate", page: 1 })
    router.push("/events")
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Etkinlikler</h1>
        <Button variant="outline" size="sm" onClick={() => setShowFilters(!showFilters)} className="gap-2">
          <SlidersHorizontal size={16} />
          Filtreler
          {hasFilters && (
            <span className="flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-xs text-white">
              !
            </span>
          )}
        </Button>
      </div>

      <SearchBar defaultValue={filters.q} />

      {showFilters && (
        <div className="space-y-4 rounded-xl bg-gray-50 p-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Şehir</label>
              <Input
                placeholder="Istanbul, Ankara..."
                value={filters.city}
                onChange={(event) => setFilters((current) => ({ ...current, city: event.target.value, page: 1 }))}
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Min Fiyat</label>
              <Input
                type="number"
                placeholder="0"
                value={filters.minPrice}
                onChange={(event) => setFilters((current) => ({ ...current, minPrice: event.target.value, page: 1 }))}
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Max Fiyat</label>
              <Input
                type="number"
                placeholder="10000"
                value={filters.maxPrice}
                onChange={(event) => setFilters((current) => ({ ...current, maxPrice: event.target.value, page: 1 }))}
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Sıralama</label>
              <select
                value={filters.sortBy}
                onChange={(event) =>
                  setFilters((current) => ({
                    ...current,
                    sortBy: event.target.value as SortBy,
                    page: 1,
                  }))
                }
                className="h-8 w-full rounded-lg border border-input bg-white px-2.5 py-1 text-sm outline-none transition-colors focus:border-ring focus:ring-2 focus:ring-ring/30"
              >
                <option value="startDate">Tarihe göre</option>
                <option value="price">Fiyata göre</option>
                <option value="relevance">İlgililik</option>
              </select>
            </div>
          </div>
          {hasFilters && (
            <Button variant="ghost" size="sm" onClick={clearFilters} className="gap-1">
              <X size={14} /> Filtreleri temizle
            </Button>
          )}
        </div>
      )}

      {data && (
        <p className="text-sm text-gray-500">
          {data.totalCount} etkinlik bulundu
          {filters.q && (
            <span>
              {" "}
              — <strong>&ldquo;{filters.q}&rdquo;</strong> için
            </span>
          )}
        </p>
      )}

      <EventsGrid events={events} isLoading={isLoading} />

      {totalPages > 1 && (
        <Pagination
          currentPage={filters.page}
          totalPages={totalPages}
          onPageChange={(page) => setFilters((current) => ({ ...current, page }))}
        />
      )}
    </div>
  )
}

export default function EventsPage() {
  return (
    <Suspense>
      <EventsPageContent />
    </Suspense>
  )
}
