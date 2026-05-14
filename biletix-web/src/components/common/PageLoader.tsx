export function PageLoader() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
      <div className="h-10 w-10 animate-spin rounded-full border-2 border-red-500 border-t-transparent" />
      <p className="text-sm text-gray-400">Yükleniyor...</p>
    </div>
  )
}
