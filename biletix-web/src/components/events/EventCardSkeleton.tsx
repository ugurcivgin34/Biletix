export function EventCardSkeleton() {
  return (
    <div className="animate-pulse overflow-hidden rounded-xl border bg-white">
      <div className="h-48 bg-gray-200" />
      <div className="space-y-3 p-4">
        <div className="h-4 w-3/4 rounded bg-gray-200" />
        <div className="h-3 w-1/2 rounded bg-gray-200" />
        <div className="h-3 w-2/3 rounded bg-gray-200" />
        <div className="mt-4 h-4 w-1/3 rounded bg-gray-200" />
      </div>
    </div>
  )
}
