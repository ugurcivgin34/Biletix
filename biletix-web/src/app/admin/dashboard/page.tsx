import { ShieldCheck } from "lucide-react"
import { EmptyState } from "@/components/common/EmptyState"

export default function AdminDashboardPage() {
  return (
    <EmptyState
      icon={ShieldCheck}
      title="Admin dashboard"
      description="Sistem istatistikleri, kullanıcılar ve venue yönetimi burada yer alacak."
    />
  )
}

