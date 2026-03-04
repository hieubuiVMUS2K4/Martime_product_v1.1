import React from 'react'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { PaginationInfo } from '@/types/maritime.types'

interface PaginationProps {
  pagination: PaginationInfo
  onPageChange: (page: number) => void
  showInfo?: boolean
  className?: string
}

export const Pagination: React.FC<PaginationProps> = ({
  pagination,
  onPageChange,
  showInfo = true,
  className = ''
}) => {
  const { currentPage, totalPages, totalCount, pageSize, hasNextPage, hasPreviousPage } = pagination

  const startIndex = (currentPage - 1) * pageSize + 1
  const endIndex = Math.min(currentPage * pageSize, totalCount)

  // Generate page numbers to display
  const getPageNumbers = () => {
    const pages: (number | string)[] = []
    const maxPagesToShow = 5
    
    if (totalPages <= maxPagesToShow) {
      // Show all pages if total is small
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i)
      }
    } else {
      // Always show first page
      pages.push(1)
      
      // Calculate range around current page
      let start = Math.max(2, currentPage - 1)
      let end = Math.min(totalPages - 1, currentPage + 1)
      
      // Add ellipsis after first page if needed
      if (start > 2) {
        pages.push('...')
      }
      
      // Add pages around current page
      for (let i = start; i <= end; i++) {
        pages.push(i)
      }
      
      // Add ellipsis before last page if needed
      if (end < totalPages - 1) {
        pages.push('...')
      }
      
      // Always show last page
      if (totalPages > 1) {
        pages.push(totalPages)
      }
    }
    
    return pages
  }

  const pageNumbers = getPageNumbers()

  return (
    <div className={`flex items-center justify-between ${className}`}>
      {/* Info Section */}
      {showInfo && (
        <div className="text-sm text-gray-600">
          Showing <span className="font-medium">{startIndex}</span> - <span className="font-medium">{endIndex}</span> of{' '}
          <span className="font-medium">{totalCount}</span> items
        </div>
      )}

      {/* Pagination Controls */}
      <div className="flex items-center gap-2">
        {/* Previous Button */}
        <button
          onClick={() => onPageChange(currentPage - 1)}
          disabled={!hasPreviousPage}
          className="flex items-center gap-1 px-3 py-1.5 text-sm font-medium border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:bg-white transition-colors"
          title="Previous page"
        >
          <ChevronLeft className="w-4 h-4" />
          <span className="hidden sm:inline">Previous</span>
        </button>

        {/* Page Numbers */}
        <div className="flex items-center gap-1">
          {pageNumbers.map((page, index) => {
            if (page === '...') {
              return (
                <span key={`ellipsis-${index}`} className="px-2 py-1.5 text-gray-500">
                  ...
                </span>
              )
            }

            const pageNum = page as number
            const isActive = pageNum === currentPage

            return (
              <button
                key={pageNum}
                onClick={() => onPageChange(pageNum)}
                className={`px-3 py-1.5 text-sm font-medium rounded-lg transition-colors ${
                  isActive
                    ? 'bg-blue-600 text-white hover:bg-blue-700'
                    : 'border border-gray-300 text-gray-700 hover:bg-gray-50'
                }`}
              >
                {pageNum}
              </button>
            )
          })}
        </div>

        {/* Next Button */}
        <button
          onClick={() => onPageChange(currentPage + 1)}
          disabled={!hasNextPage}
          className="flex items-center gap-1 px-3 py-1.5 text-sm font-medium border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:bg-white transition-colors"
          title="Next page"
        >
          <span className="hidden sm:inline">Next</span>
          <ChevronRight className="w-4 h-4" />
        </button>
      </div>
    </div>
  )
}

// Compact version for smaller spaces
export const CompactPagination: React.FC<PaginationProps> = ({
  pagination,
  onPageChange,
  className = ''
}) => {
  const { currentPage, totalPages, hasNextPage, hasPreviousPage } = pagination

  return (
    <div className={`flex items-center gap-2 ${className}`}>
      <button
        onClick={() => onPageChange(currentPage - 1)}
        disabled={!hasPreviousPage}
        className="p-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
        title="Previous page"
      >
        <ChevronLeft className="w-4 h-4" />
      </button>

      <span className="text-sm text-gray-600 min-w-[80px] text-center">
        Page {currentPage} / {totalPages}
      </span>

      <button
        onClick={() => onPageChange(currentPage + 1)}
        disabled={!hasNextPage}
        className="p-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
        title="Next page"
      >
        <ChevronRight className="w-4 h-4" />
      </button>
    </div>
  )
}
