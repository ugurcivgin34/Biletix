import { AlertTriangle, Calendar, CheckCircle, User, XCircle } from "lucide-react"
import { type ValidateTicketResponse } from "@/lib/api/tickets"
import { formatDate } from "@/lib/utils/format"

interface Props {
  result: ValidateTicketResponse
  onReset: () => void
}

export function ScanResult({ result, onReset }: Props) {
  const isAlreadyScanned = result.alreadyScanned && !result.isValid

  const config = result.isValid
    ? {
        bg: "bg-green-50 border-green-200",
        icon: <CheckCircle className="text-green-500" size={48} />,
        title: "Giriş Onaylandı",
        titleColor: "text-green-700",
      }
    : isAlreadyScanned
      ? {
          bg: "bg-yellow-50 border-yellow-200",
          icon: <AlertTriangle className="text-yellow-500" size={48} />,
          title: "Bilet Zaten Kullanıldı",
          titleColor: "text-yellow-700",
        }
      : {
          bg: "bg-red-50 border-red-200",
          icon: <XCircle className="text-red-500" size={48} />,
          title: "Geçersiz Bilet",
          titleColor: "text-red-700",
        }

  return (
    <div className={`space-y-4 rounded-2xl border-2 p-6 ${config.bg}`}>
      <div className="space-y-3 text-center">
        <div className="flex justify-center">{config.icon}</div>
        <h2 className={`text-xl font-bold ${config.titleColor}`}>{config.title}</h2>
        <p className="text-sm text-gray-600">{result.message}</p>
      </div>

      {(result.attendeeFirstName || result.eventTitle) && (
        <div className="space-y-2 rounded-xl bg-white p-4">
          {result.attendeeFirstName && (
            <div className="flex items-center gap-2 text-sm">
              <User size={14} className="text-gray-400" />
              <span className="font-medium">
                {result.attendeeFirstName} {result.attendeeLastName}
              </span>
            </div>
          )}
          {result.eventTitle && (
            <div className="flex items-center gap-2 text-sm">
              <Calendar size={14} className="text-gray-400" />
              <span className="text-gray-600">{result.eventTitle}</span>
            </div>
          )}
          {result.firstScannedAt && isAlreadyScanned && (
            <p className="mt-2 text-xs text-yellow-600">
              İlk giriş: {formatDate(result.firstScannedAt)}
            </p>
          )}
        </div>
      )}

      <button
        type="button"
        onClick={onReset}
        className="w-full rounded-xl border bg-white py-3 text-sm font-medium transition-colors hover:bg-gray-50"
      >
        Yeni Tarama
      </button>
    </div>
  )
}
