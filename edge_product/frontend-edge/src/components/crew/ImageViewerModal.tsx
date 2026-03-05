import { X, Upload } from 'lucide-react'
import { useState, useEffect } from 'react'
import { toast } from 'react-toastify'
import { maritimeService } from '../../services/maritime.service'

type ImageViewerModalProps = {
  isOpen: boolean
  imageUrl: string | null
  documentId?: string
  targetTable?: string
  onClose: () => void
  onFileChanged?: () => void
  customUploadHandler?: (documentId: string, formData: FormData) => Promise<any>
}

export default function ImageViewerModal({ isOpen, imageUrl, documentId, targetTable, onClose, onFileChanged, customUploadHandler }: ImageViewerModalProps) {
  const [uploading, setUploading] = useState(false)
  const [currentImageUrl, setCurrentImageUrl] = useState(imageUrl)
  const [previewFile, setPreviewFile] = useState<File | null>(null)
  const [previewUrl, setPreviewUrl] = useState<string | null>(null)
  
  // Update currentImageUrl when imageUrl prop changes
  useEffect(() => {
    setCurrentImageUrl(imageUrl)
    setPreviewFile(null)
    setPreviewUrl(null)
  }, [imageUrl])
  
  if (!isOpen || !imageUrl) return null

  const handleSelectImage = () => {
    const input = document.createElement('input')
    input.type = 'file'
    input.accept = 'image/*'
    
    input.onchange = (e) => {
      const file = (e.target as HTMLInputElement).files?.[0]
      if (!file) return

      // Create preview URL
      const url = URL.createObjectURL(file)
      setPreviewFile(file)
      setPreviewUrl(url)
    }

    input.click()
  }

  const handleConfirmChange = async () => {
    if (!previewFile || !documentId) return
    if (!customUploadHandler && !targetTable) return

    try {
      setUploading(true)
      const formData = new FormData()
      formData.append('file', previewFile)
      if (targetTable) {
        formData.append('targetTable', targetTable)
      }

      let result: any
      if (customUploadHandler) {
        result = await customUploadHandler(documentId, formData)
      } else {
        result = await maritimeService.crew.updateDocumentFile(documentId, formData)
      }
      
      // Update local image URL with cache busting
      const newUrl = result.fileUrl || result.documentFilePath
      if (newUrl) {
        setCurrentImageUrl(`${newUrl}?t=${Date.now()}`)
      }
      
      // Clear preview
      setPreviewFile(null)
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl)
      }
      setPreviewUrl(null)
      
      // Notify parent to refresh data
      if (onFileChanged) {
        onFileChanged()
      }
      
      toast.success('Image changed successfully!')
    } catch (error: any) {
      console.error('❌ Failed to change image:', error)
      toast.error(error.message || 'Failed to change image')
    } finally {
      setUploading(false)
    }
  }

  const handleCancelChange = () => {
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl)
    }
    setPreviewFile(null)
    setPreviewUrl(null)
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
      <div className="bg-white rounded-lg shadow-xl max-w-4xl max-h-[90vh] w-full mx-4 flex flex-col">
        <div className="flex items-center justify-between p-4 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900">
            {previewFile ? 'Preview New Image' : 'Document Image'}
          </h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>
        
        <div className="flex-1 overflow-auto p-4 flex items-center justify-center bg-gray-50">
          {uploading ? (
            <div className="flex flex-col items-center gap-4">
              <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent"></div>
              <p className="text-gray-600 font-medium">Uploading new image...</p>
            </div>
          ) : (
            <img 
              src={previewUrl || (currentImageUrl || imageUrl)?.startsWith('http') ? (previewUrl || currentImageUrl || imageUrl) : (previewUrl || currentImageUrl || imageUrl)}
              alt="Document" 
              className="max-w-full max-h-full object-contain"
              onError={(e) => {
                const target = e.target as HTMLImageElement
                target.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0iI2VlZSIvPjx0ZXh0IHg9IjUwJSIgeT0iNTAlIiBmb250LWZhbWlseT0iQXJpYWwiIGZvbnQtc2l6ZT0iMTQiIGZpbGw9IiM5OTkiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGR5PSIuM2VtIj5JbWFnZSBub3QgZm91bmQ8L3RleHQ+PC9zdmc+'
              }}
            />
          )}
        </div>
        
        <div className="p-4 border-t border-gray-200 flex justify-between items-center">
          <div className="flex items-center gap-2">
            {documentId && (targetTable || customUploadHandler) && !previewFile && (
              <button
                onClick={handleSelectImage}
                disabled={uploading}
                className="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded transition-colors flex items-center gap-2"
              >
                <Upload className="w-4 h-4" />
                Change Image
              </button>
            )}
            {previewFile && (
              <>
                <button
                  onClick={handleConfirmChange}
                  disabled={uploading}
                  className="px-4 py-2 bg-green-600 hover:bg-green-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded transition-colors flex items-center gap-2"
                >
                  ✓ Confirm
                </button>
                <button
                  onClick={handleCancelChange}
                  disabled={uploading}
                  className="px-4 py-2 bg-red-600 hover:bg-red-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded transition-colors flex items-center gap-2"
                >
                  ✕ Cancel
                </button>
              </>
            )}
          </div>
          <button
            onClick={onClose}
            disabled={uploading}
            className="px-4 py-2 bg-gray-600 hover:bg-gray-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded transition-colors"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  )
}
