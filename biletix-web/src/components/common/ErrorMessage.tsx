import { AlertCircle } from "lucide-react"
import { Button } from "@/components/ui/button"

interface Props {
  title?: string
  message?: string
  onRetry?: () => void
}

export function ErrorMessage({
  title = "Bir hata oluştu",
  message = "Lütfen sayfayı yenileyin veya tekrar deneyin.",
  onRetry,
}: Props) {
  return (
    <div className="flex min-h-[40vh] flex-col items-center justify-center gap-4 text-center">
      <div className="rounded-full bg-red-50 p-4">
        <AlertCircle className="text-red-400" size={32} />
      </div>
      <div>
        <h3 className="font-semibold text-gray-900">{title}</h3>
        <p className="mt-1 text-sm text-gray-500">{message}</p>
      </div>
      {onRetry && (
        <Button variant="outline" size="sm" onClick={onRetry}>
          Tekrar Dene
        </Button>
      )}
    </div>
  )
}
