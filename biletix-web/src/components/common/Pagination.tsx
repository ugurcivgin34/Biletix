interface Props {
  currentPage: number
  totalPages: number
  onPageChange: (page: number) => void
}

export function Pagination({ currentPage, totalPages, onPageChange }: Props) {
  const pages = Array.from({ length: Math.min(totalPages, 5) }, (_, index) => {
    if (totalPages <= 5) return index + 1
    if (currentPage <= 3) return index + 1
    if (currentPage >= totalPages - 2) return totalPages - 4 + index
    return currentPage - 2 + index
  })

  return (
    <div className="flex items-center justify-center gap-2 py-4">
      <button
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        className="rounded-lg border px-3 py-2 text-sm transition-colors hover:bg-gray-50 disabled:opacity-50"
      >
        ← Önceki
      </button>

      {pages.map((page) => (
        <button
          key={page}
          onClick={() => onPageChange(page)}
          className={`h-10 w-10 rounded-lg text-sm font-medium transition-colors ${
            currentPage === page ? "bg-red-500 text-white" : "border hover:bg-gray-50"
          }`}
        >
          {page}
        </button>
      ))}

      <button
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        className="rounded-lg border px-3 py-2 text-sm transition-colors hover:bg-gray-50 disabled:opacity-50"
      >
        Sonraki →
      </button>
    </div>
  )
}
