import { UserRound } from "lucide-react"
import { EmptyState } from "@/components/common/EmptyState"

export default function ProfilePage() {
  return (
    <EmptyState
      icon={UserRound}
      title="Profil"
      description="Kullanıcı bilgileri ve hesap ayarları burada yer alacak."
    />
  )
}

