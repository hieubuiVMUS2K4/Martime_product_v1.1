import { useState, useRef } from 'react';
import { X, Upload, FileSpreadsheet, AlertCircle, CheckCircle, Loader2 } from 'lucide-react';
// import * as XLSX from 'xlsx'; // TODO: Install xlsx package
import { receiptService, type ImportReceiptDto, type ImportReceiptItemDto, type ReceiptPreviewResponseDto } from '../../services/receiptService';

interface ImportReceiptModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export function ImportReceiptModal({ isOpen, onClose, onSuccess }: ImportReceiptModalProps) {
  const [step, setStep] = useState<'upload' | 'preview' | 'importing'>('upload');
  const [receiptDate, setReceiptDate] = useState(new Date().toISOString().split('T')[0]);
  const [notes, setNotes] = useState('');
  const [createdBy, setCreatedBy] = useState('');
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);
  const [parsedItems, setParsedItems] = useState<ImportReceiptItemDto[]>([]);
  const [preview, setPreview] = useState<ReceiptPreviewResponseDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  if (!isOpen) return null;

  const handleClose = () => {
    setStep('upload');
    setUploadedFile(null);
    setParsedItems([]);
    setPreview(null);
    setError(null);
    setNotes('');
    setCreatedBy('');
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
    onClose();
  };

  const handleFileSelect = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    setError(null);
    setUploadedFile(file);

    try {
      const items = await parseExcelFile(file);
      setParsedItems(items);
      
      if (items.length === 0) {
        setError('No valid items found in Excel file');
      }
    } catch (err: any) {
      setError(err.message || 'Failed to parse Excel file');
      setParsedItems([]);
    }
  };

  const parseExcelFile = (file: File): Promise<ImportReceiptItemDto[]> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onload = () => {
        try {
          // TODO: Install xlsx package first
          throw new Error('XLSX package not installed. Please run: npm install xlsx @types/xlsx');
          
          /*
          const data = e.target?.result;
          if (!data) {
            reject(new Error('Failed to read file'));
            return;
          }

          // Parse Excel file
          const workbook = XLSX.read(data, { type: 'binary' });
          const sheetName = workbook.SheetNames[0];
          const worksheet = workbook.Sheets[sheetName];
          const jsonData: any[] = XLSX.utils.sheet_to_json(worksheet);

          if (jsonData.length === 0) {
            reject(new Error('Excel file is empty'));
            return;
          }
          */
          
          // Temporarily return empty array until XLSX is installed
          resolve([]);
        } catch (error) {
          reject(new Error('Failed to parse Excel file. Please check the format.'));
        }
      };

      reader.onerror = () => reject(new Error('Failed to read file'));
      reader.readAsBinaryString(file);
    });
  };

  const handlePreview = async () => {
    try {
      setError(null);
      setLoading(true);

      if (parsedItems.length === 0) {
        setError('No items to import. Please select an Excel file.');
        setLoading(false);
        return;
      }

      const dto: ImportReceiptDto = {
        receiptDate,
        notes: notes || undefined,
        createdBy: createdBy || undefined,
        importSource: 'Excel',
        importFileName: uploadedFile?.name,
        items: parsedItems
      };

      const previewResult = await receiptService.previewImport(dto);
      setPreview(previewResult);
      setStep('preview');
    } catch (err: any) {
      setError(err.message || 'Failed to preview import');
    } finally {
      setLoading(false);
    }
  };

  const handleImport = async () => {
    if (!preview) return;

    try {
      setError(null);
      setStep('importing');

      const dto: ImportReceiptDto = {
        receiptDate,
        notes: notes || undefined,
        createdBy: createdBy || undefined,
        importSource: 'Excel',
        importFileName: uploadedFile?.name,
        items: parsedItems,
      };

      await receiptService.importReceipt(dto);
      onSuccess();
      handleClose();
    } catch (err: any) {
      setError(err.message || 'Failed to import receipt');
      setStep('preview');
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-5xl max-h-[90vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div className="flex items-center gap-3">
            <FileSpreadsheet className="w-6 h-6 text-blue-600" />
            <div>
              <h2 className="text-xl font-bold text-gray-900">Import Phiếu Nhập Kho</h2>
              <p className="text-sm text-gray-600">Nhập nhiều vật tư cùng lúc từ dữ liệu Excel</p>
            </div>
          </div>
          <button onClick={handleClose} className="p-2 hover:bg-gray-100 rounded-lg">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {step === 'upload' && (
            <div className="space-y-6">
              {/* Receipt Info */}
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Ngày nhập kho <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="date"
                    value={receiptDate}
                    onChange={(e) => setReceiptDate(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Người tạo
                  </label>
                  <input
                    type="text"
                    value={createdBy}
                    onChange={(e) => setCreatedBy(e.target.value)}
                    placeholder="Nhập tên người tạo..."
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Ghi chú
                  </label>
                  <input
                    type="text"
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    placeholder="Ghi chú về phiếu nhập..."
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>

              {/* Instructions */}
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h3 className="font-semibold text-blue-900 mb-2">📋 Hướng dẫn:</h3>
                <ol className="text-sm text-blue-800 space-y-1 list-decimal list-inside">
                  <li>Chuẩn bị file Excel với các cột sau:</li>
                  <li className="ml-6"><code className="bg-blue-100 px-1 rounded">ItemCode, ItemName, Category, Quantity, Unit, UnitCost, Location</code></li>
                  <li className="ml-6">Các cột tùy chọn: <code className="bg-blue-100 px-1 rounded">PartNumber, Barcode, Manufacturer, Specification, MinStock, MaxStock</code></li>
                  <li>Upload file Excel (.xlsx hoặc .xls)</li>
                  <li>Click "Preview" để kiểm tra trước khi import</li>
                </ol>
              </div>

              {/* File Upload Area */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Upload File Excel <span className="text-red-500">*</span>
                </label>
                <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center hover:border-blue-500 transition-colors">
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept=".xlsx,.xls"
                    onChange={handleFileSelect}
                    className="hidden"
                    id="excel-file-input"
                  />
                  <label htmlFor="excel-file-input" className="cursor-pointer">
                    <FileSpreadsheet className="w-12 h-12 text-gray-400 mx-auto mb-4" />
                    {uploadedFile ? (
                      <div>
                        <p className="text-sm font-medium text-gray-900">{uploadedFile.name}</p>
                        <p className="text-xs text-gray-500 mt-1">
                          {parsedItems.length} items đã được phát hiện
                        </p>
                        <button
                          type="button"
                          onClick={(e) => {
                            e.preventDefault();
                            setUploadedFile(null);
                            setParsedItems([]);
                            if (fileInputRef.current) {
                              fileInputRef.current.value = '';
                            }
                          }}
                          className="mt-2 text-sm text-red-600 hover:text-red-800"
                        >
                          Xóa file
                        </button>
                      </div>
                    ) : (
                      <div>
                        <p className="text-sm text-gray-600">
                          Click để chọn file Excel hoặc kéo thả vào đây
                        </p>
                        <p className="text-xs text-gray-500 mt-1">
                          Hỗ trợ định dạng: .xlsx, .xls
                        </p>
                      </div>
                    )}
                  </label>
                </div>
              </div>

              {/* Preview parsed items */}
              {parsedItems.length > 0 && (
                <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                  <p className="text-sm font-semibold text-green-900">
                    ✓ Đã phân tích file thành công: {parsedItems.length} items
                  </p>
                  <p className="text-xs text-green-700 mt-1">
                    Click "Preview" để xem chi tiết trước khi import
                  </p>
                </div>
              )}

              {error && (
                <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
                  <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                  <div>
                    <p className="font-semibold text-red-900">Lỗi</p>
                    <p className="text-sm text-red-800">{error}</p>
                  </div>
                </div>
              )}
            </div>
          )}

          {step === 'preview' && preview && (
            <div className="space-y-6">
              {/* Summary */}
              <div className="grid grid-cols-5 gap-4">
                <div className="bg-blue-50 rounded-lg p-4">
                  <p className="text-xs text-blue-600 font-medium">Tổng items</p>
                  <p className="text-2xl font-bold text-blue-900">{preview.summary.totalItems}</p>
                </div>
                <div className="bg-green-50 rounded-lg p-4">
                  <p className="text-xs text-green-600 font-medium">Tạo mới</p>
                  <p className="text-2xl font-bold text-green-900">{preview.summary.newItems}</p>
                </div>
                <div className="bg-yellow-50 rounded-lg p-4">
                  <p className="text-xs text-yellow-600 font-medium">Cập nhật</p>
                  <p className="text-2xl font-bold text-yellow-900">{preview.summary.existingItems}</p>
                </div>
                <div className="bg-red-50 rounded-lg p-4">
                  <p className="text-xs text-red-600 font-medium">Lỗi</p>
                  <p className="text-2xl font-bold text-red-900">{preview.summary.errorItems}</p>
                </div>
                <div className="bg-purple-50 rounded-lg p-4">
                  <p className="text-xs text-purple-600 font-medium">Tổng giá trị</p>
                  <p className="text-2xl font-bold text-purple-900">${preview.summary.totalAmount.toFixed(2)}</p>
                </div>
              </div>

              {/* Warnings & Errors */}
              {preview.warnings.length > 0 && (
                <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                  <p className="font-semibold text-yellow-900 mb-2">⚠️ Cảnh báo:</p>
                  <ul className="text-sm text-yellow-800 space-y-1">
                    {preview.warnings.map((w, i) => <li key={i}>• {w}</li>)}
                  </ul>
                </div>
              )}

              {preview.errors.length > 0 && (
                <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                  <p className="font-semibold text-red-900 mb-2">❌ Lỗi:</p>
                  <ul className="text-sm text-red-800 space-y-1">
                    {preview.errors.map((e, i) => <li key={i}>• {e}</li>)}
                  </ul>
                </div>
              )}

              {/* Items Preview Table */}
              <div className="border border-gray-200 rounded-lg overflow-hidden">
                <div className="overflow-x-auto max-h-96">
                  <table className="w-full">
                    <thead className="bg-gray-50 sticky top-0">
                      <tr>
                        <th className="px-3 py-2 text-left text-xs font-medium text-gray-500">STT</th>
                        <th className="px-3 py-2 text-left text-xs font-medium text-gray-500">Action</th>
                        <th className="px-3 py-2 text-left text-xs font-medium text-gray-500">Item Code</th>
                        <th className="px-3 py-2 text-left text-xs font-medium text-gray-500">Item Name</th>
                        <th className="px-3 py-2 text-left text-xs font-medium text-gray-500">Category</th>
                        <th className="px-3 py-2 text-right text-xs font-medium text-gray-500">Quantity</th>
                        <th className="px-3 py-2 text-left text-xs font-medium text-gray-500">Unit</th>
                        <th className="px-3 py-2 text-right text-xs font-medium text-gray-500">Unit Cost</th>
                        <th className="px-3 py-2 text-left text-xs font-medium text-gray-500">Stock Change</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                      {preview.items.map((item, idx) => (
                        <tr key={idx} className={`
                          ${item.action === 'ERROR' ? 'bg-red-50' : ''}
                          ${item.action === 'CREATE' ? 'bg-green-50' : ''}
                          ${item.action === 'UPDATE' ? 'bg-yellow-50' : ''}
                        `}>
                          <td className="px-3 py-2 text-sm">{item.lineNumber}</td>
                          <td className="px-3 py-2">
                            <span className={`text-xs px-2 py-1 rounded-full font-medium ${
                              item.action === 'CREATE' ? 'bg-green-100 text-green-700' :
                              item.action === 'UPDATE' ? 'bg-yellow-100 text-yellow-700' :
                              'bg-red-100 text-red-700'
                            }`}>
                              {item.action}
                            </span>
                          </td>
                          <td className="px-3 py-2 text-sm font-mono">{item.itemCode}</td>
                          <td className="px-3 py-2 text-sm">{item.itemName}</td>
                          <td className="px-3 py-2 text-sm text-gray-600">{item.categoryName}</td>
                          <td className="px-3 py-2 text-sm text-right font-medium">{item.quantity}</td>
                          <td className="px-3 py-2 text-sm">{item.unit}</td>
                          <td className="px-3 py-2 text-sm text-right">{item.unitCost?.toFixed(2) || '-'}</td>
                          <td className="px-3 py-2 text-sm">
                            {item.action === 'UPDATE' && (
                              <span className="text-xs text-gray-600">
                                {item.currentStock?.toFixed(2)} → {item.newStock?.toFixed(2)}
                              </span>
                            )}
                            {item.action === 'CREATE' && (
                              <span className="text-xs text-green-600">New: {item.newStock?.toFixed(2)}</span>
                            )}
                            {item.errorMessage && (
                              <span className="text-xs text-red-600">{item.errorMessage}</span>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          )}

          {step === 'importing' && (
            <div className="flex flex-col items-center justify-center py-12">
              <Loader2 className="w-12 h-12 text-blue-600 animate-spin mb-4" />
              <p className="text-lg font-medium text-gray-900">Đang import...</p>
              <p className="text-sm text-gray-600">Vui lòng đợi trong giây lát</p>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between p-6 border-t border-gray-200 bg-gray-50">
          <div className="text-sm text-gray-600">
            {step === 'upload' && 'Bước 1: Nhập dữ liệu'}
            {step === 'preview' && 'Bước 2: Xem trước và xác nhận'}
            {step === 'importing' && 'Đang xử lý...'}
          </div>
          <div className="flex gap-3">
            <button
              onClick={handleClose}
              disabled={step === 'importing'}
              className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
            >
              Hủy
            </button>
            {step === 'upload' && (
              <button
                onClick={handlePreview}
                disabled={!uploadedFile || parsedItems.length === 0 || loading}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
              >
                {loading ? (
                  <>
                    <Loader2 className="w-4 h-4 animate-spin" />
                    Đang xử lý...
                  </>
                ) : (
                  <>
                    <Upload className="w-4 h-4" />
                    Preview
                  </>
                )}
              </button>
            )}
            {step === 'preview' && (
              <>
                <button
                  onClick={() => setStep('upload')}
                  className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
                >
                  ← Quay lại
                </button>
                <button
                  onClick={handleImport}
                  disabled={!preview || preview.summary.errorItems > 0}
                  className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                >
                  <CheckCircle className="w-4 h-4" />
                  Confirm & Import
                </button>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
