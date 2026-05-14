"use client"

import { useState, type FormEvent } from "react"
import { useRouter } from "next/navigation"
import { Search } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"

interface Props {
  defaultValue?: string
  placeholder?: string
}

export function SearchBar({
  defaultValue = "",
  placeholder = "Etkinlik, sanatçı veya mekan ara...",
}: Props) {
  const [query, setQuery] = useState(defaultValue)
  const router = useRouter()

  const handleSearch = (event: FormEvent) => {
    event.preventDefault()
    const params = new URLSearchParams()
    if (query.trim()) params.set("q", query.trim())
    router.push(`/events${params.toString() ? `?${params.toString()}` : ""}`)
  }

  return (
    <form noValidate onSubmit={handleSearch} className="flex w-full max-w-2xl flex-col gap-2 sm:flex-row">
      <div className="relative flex-1">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={18} />
        <Input
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          placeholder={placeholder}
          className="h-12 pl-10 text-base"
        />
      </div>
      <Button type="submit" className="h-12 w-full bg-red-500 px-6 hover:bg-red-600 sm:w-auto">
        Ara
      </Button>
    </form>
  )
}

