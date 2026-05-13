"use client"

import Link from "next/link"
import { Shield, Star, Ticket, Zap } from "lucide-react"
import { EventsGrid } from "@/components/events/EventsGrid"
import { SearchBar } from "@/components/events/SearchBar"
import { useSearchEvents } from "@/lib/hooks/useEvents"

export default function HomePage() {
  const { data, isLoading } = useSearchEvents({ pageSize: 8, sortBy: "startDate" })
  const events = data?.items ?? []

  return (
    <div className="space-y-12">
      <section className="relative -mx-4 overflow-hidden rounded-2xl bg-gray-950 px-4 py-20 text-white">
        <div className="absolute inset-0 bg-[linear-gradient(135deg,rgba(17,24,39,1)_0%,rgba(127,29,29,0.92)_52%,rgba(17,24,39,1)_100%)]" />
        <div className="absolute inset-x-0 bottom-0 h-px bg-red-300/40" />

        <div className="relative mx-auto max-w-3xl space-y-6 text-center">
          <div className="mb-4 flex items-center justify-center gap-3">
            <Ticket className="text-red-300" size={40} />
            <h1 className="text-5xl font-bold">Biletix</h1>
          </div>
          <p className="text-xl text-gray-200">
            Konser, tiyatro, spor ve daha fazlası için
            <br />
            en iyi biletler burada!
          </p>
          <div className="flex justify-center">
            <SearchBar />
          </div>
        </div>
      </section>

      <section className="grid grid-cols-1 gap-6 md:grid-cols-3">
        {[
          { icon: Zap, title: "Hızlı Satın Al", desc: "Saniyeler içinde biletini al, QR kodunu al" },
          { icon: Shield, title: "Güvenli Ödeme", desc: "Stripe ile güvenli ödeme altyapısı" },
          { icon: Star, title: "En İyi Etkinlikler", desc: "Türkiye'nin en popüler etkinlikleri" },
        ].map(({ icon: Icon, title, desc }) => (
          <div key={title} className="flex gap-4 rounded-xl bg-gray-50 p-4">
            <div className="h-fit rounded-lg bg-red-100 p-2">
              <Icon className="text-red-500" size={20} />
            </div>
            <div>
              <h3 className="font-semibold text-gray-900">{title}</h3>
              <p className="mt-1 text-sm text-gray-500">{desc}</p>
            </div>
          </div>
        ))}
      </section>

      <section>
        <div className="mb-6 flex items-center justify-between">
          <h2 className="text-2xl font-bold">Yaklaşan Etkinlikler</h2>
          <Link href="/events" className="text-sm text-red-500 hover:underline">
            Tümünü gör →
          </Link>
        </div>
        <EventsGrid events={events} isLoading={isLoading} />
      </section>
    </div>
  )
}

