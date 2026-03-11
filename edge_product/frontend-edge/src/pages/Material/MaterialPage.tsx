import { useState, useEffect, useMemo, useCallback } from 'react';
import { Plus, Search, Package, Eye, Edit2, Trash2, ChevronsUpDown, Upload, ChevronRight } from 'lucide-react';
import { materialService } from '@/services/materialService';
import type { CreateMaterialItemDto, UpdateMaterialItemDto, StockAdjustmentDto } from '@/services/materialService';
import { ItemFormModal } from './ItemFormModal';
import { CategoryFormModal } from './CategoryFormModal';
import { StockAdjustmentModal } from './StockAdjustmentModal';
import { ImportReceiptModal } from './ImportReceiptModal';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { MaterialItem, MaterialCategory } from '@/types/maritime.types';

const ITEMS_PER_PAGE_OPTIONS = [10, 20, 50];

export function MaterialPage() {
  const { t } = useTranslationSafe();

  const [items, setItems] = useState<MaterialItem[]>([]);
  const [categories, setCategories] = useState<MaterialCategory[]>([]);
  const [loading, setLoading] = useState(true);

  // Filters
  const [searchName, setSearchName] = useState('');
  const [searchCode, setSearchCode] = useState('');
  const [filterCategory, setFilterCategory] = useState<string>('');
  const [filterUnit, setFilterUnit] = useState<string>('');

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(10);

  // Row selection
  const [selectedRows, setSelectedRows] = useState<Set<string>>(new Set());

  // Modals
  const [itemModalOpen, setItemModalOpen] = useState(false);
  const [categoryModalOpen, setCategoryModalOpen] = useState(false);
  const [stockAdjustmentModalOpen, setStockAdjustmentModalOpen] = useState(false);
  const [importReceiptModalOpen, setImportReceiptModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<MaterialItem | null>(null);
  const [editingCategory, setEditingCategory] = useState<MaterialCategory | null>(null);
  const [adjustingItem, setAdjustingItem] = useState<MaterialItem | null>(null);

  useEffect(() => { loadData(); }, []);

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const [its, cats] = await Promise.all([
        materialService.getItems({ onlyActive: true }),
        materialService.getCategories(true),
      ]);
      setItems(its);
      setCategories(cats);
    } catch (e) {
      console.error('Failed to load material data:', e);
    } finally {
      setLoading(false);
    }
  }, []);

  // ---------- Handlers ----------
  const handleCreateItem = async (data: CreateMaterialItemDto) => {
    await materialService.createItem(data);
    await loadData();
  };

  const handleUpdateItem = async (data: UpdateMaterialItemDto) => {
    if (!editingItem) return;
    await materialService.updateItem(editingItem.id, data);
    setEditingItem(null);
    await loadData();
  };

  const handleDeleteItem = async (item: MaterialItem) => {
    if (!confirm(t('materials.page.confirmDelete', { name: item.name }))) return;
    try {
      await materialService.deleteItem(item.id);
      await loadData();
    } catch (error: any) {
      alert(error.message || 'Failed to delete item');
    }
  };

  const handleStockAdjustment = async (data: StockAdjustmentDto) => {
    await materialService.adjustStock(data);
    setAdjustingItem(null);
    await loadData();
  };

  // Category handlers (for modal)
  const handleCreateCategory = async (data: any) => {
    await materialService.createCategory(data);
    await loadData();
  };
  const handleUpdateCategory = async (data: any) => {
    if (!editingCategory) return;
    await materialService.updateCategory(editingCategory.id, data);
    setEditingCategory(null);
    await loadData();
  };

  // ---------- Derived ----------
  const categoryMap = useMemo(() => {
    const m = new Map<number, string>();
    categories.forEach(c => m.set(c.id, c.name));
    return m;
  }, [categories]);

  const uniqueUnits = useMemo(() => [...new Set(items.map(i => i.unit))].sort(), [items]);

  const filteredItems = useMemo(() => {
    let data = [...items];
    if (searchName) {
      const q = searchName.toLowerCase();
      data = data.filter(i => i.name.toLowerCase().includes(q) || i.manufacturer?.toLowerCase().includes(q));
    }
    if (searchCode) {
      const q = searchCode.toLowerCase();
      data = data.filter(i => i.itemCode.toLowerCase().includes(q));
    }
    if (filterCategory) {
      data = data.filter(i => String(i.categoryId) === filterCategory);
    }
    if (filterUnit) {
      data = data.filter(i => i.unit === filterUnit);
    }
    return data;
  }, [items, searchName, searchCode, filterCategory, filterUnit]);

  const totalPages = Math.max(1, Math.ceil(filteredItems.length / itemsPerPage));

  const paginatedItems = useMemo(() => {
    const start = (currentPage - 1) * itemsPerPage;
    return filteredItems.slice(start, start + itemsPerPage);
  }, [filteredItems, currentPage, itemsPerPage]);

  useEffect(() => { setCurrentPage(1); }, [searchName, searchCode, filterCategory, filterUnit]);

  const toggleRow = (id: string) => {
    setSelectedRows(prev => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  };

  const toggleAllRows = () => {
    if (selectedRows.size === paginatedItems.length) setSelectedRows(new Set());
    else setSelectedRows(new Set(paginatedItems.map(i => i.id)));
  };

  const formatDate = (dateStr: string) => {
    try { return new Date(dateStr).toLocaleDateString('vi-VN'); } catch { return dateStr; }
  };

  // ---------- Render ----------
  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">{t('materials.loading')}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="h-full w-full flex flex-col overflow-hidden bg-white">

      {/* ── HEADER ROW ── */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200 bg-white flex-shrink-0">
        <div className="flex items-center gap-2">
          <span className="text-sm font-semibold text-gray-700">
            ≡ {t('materials.page.materialList')}
          </span>
          <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full font-semibold">
            {filteredItems.length}
          </span>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={() => setCategoryModalOpen(true)}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-300 rounded text-gray-600 hover:bg-gray-50"
          >
            {t('materials.page.manageCategories')}
          </button>
          <button
            onClick={() => { setEditingItem(null); setItemModalOpen(true); }}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            <Plus className="w-3.5 h-3.5" />
            {t('materials.page.addNew')}
          </button>
          <button
            onClick={() => setImportReceiptModalOpen(true)}
            className="p-1.5 border border-gray-300 rounded text-gray-500 hover:bg-gray-50"
            title={t('materials.importReceipt')}
          >
            <Upload className="w-3.5 h-3.5" />
          </button>
        </div>
      </div>

      {/* ── TABLE ── */}
      <div className="flex-1 overflow-auto">
        <table className="min-w-full text-sm border-collapse">
          <thead className="sticky top-0 z-10">

            {/* Row 1: Column headers */}
            <tr className="bg-blue-50">
              <th className="w-10 px-2 py-2 text-center text-xs font-semibold text-gray-600 border-b border-r border-gray-200">TT</th>
              <th className="w-10 px-2 py-2 text-center text-xs font-semibold text-gray-600 border-b border-r border-gray-200">
                <input
                  type="checkbox"
                  checked={selectedRows.size === paginatedItems.length && paginatedItems.length > 0}
                  onChange={toggleAllRows}
                  className="rounded text-blue-600"
                />
              </th>
              <th className="w-32 px-3 py-2 text-left border-b border-r border-gray-200">
                <div className="flex items-center justify-between gap-1">
                  <span className="text-xs font-semibold text-gray-600">{t('materials.page.colCode')}</span>
                  <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                </div>
              </th>
              <th className="min-w-[200px] px-3 py-2 text-left border-b border-r border-gray-200">
                <div className="flex items-center justify-between gap-1">
                  <span className="text-xs font-semibold text-gray-600">{t('materials.page.colName')}</span>
                  <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                </div>
              </th>
              <th className="w-40 px-3 py-2 text-left border-b border-r border-gray-200">
                <div className="flex items-center justify-between gap-1">
                  <span className="text-xs font-semibold text-gray-600">{t('materials.page.colCategory')}</span>
                  <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                </div>
              </th>
              <th className="w-24 px-3 py-2 text-left border-b border-r border-gray-200">
                <div className="flex items-center justify-between gap-1">
                  <span className="text-xs font-semibold text-gray-600">{t('materials.page.colUnit')}</span>
                  <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                </div>
              </th>
              <th className="min-w-[180px] px-3 py-2 text-left border-b border-r border-gray-200">
                <div className="flex items-center justify-between gap-1">
                  <span className="text-xs font-semibold text-gray-600">{t('materials.page.colDescription')}</span>
                </div>
              </th>
              <th className="w-28 px-3 py-2 text-left border-b border-r border-gray-200">
                <div className="flex items-center justify-between gap-1">
                  <span className="text-xs font-semibold text-gray-600">{t('materials.page.colUpdatedAt')}</span>
                  <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                </div>
              </th>
              <th className="w-28 px-3 py-2 text-left border-b border-r border-gray-200">
                <span className="text-xs font-semibold text-gray-600">{t('materials.page.colCreatedBy')}</span>
              </th>
              <th className="w-24 px-3 py-2 border-b border-gray-200"></th>
            </tr>

            {/* Row 2: Column search inputs */}
            <tr className="bg-white border-b border-gray-200">
              <th className="border-r border-gray-200"></th>
              <th className="border-r border-gray-200"></th>
              {/* Code search */}
              <th className="px-2 py-1 border-r border-gray-200">
                <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                  <span className="text-gray-400 text-xs select-none">→</span>
                  <input
                    type="text"
                    placeholder={t('common.search')}
                    value={searchCode}
                    onChange={e => setSearchCode(e.target.value)}
                    className="flex-1 text-xs outline-none min-w-0 bg-transparent"
                  />
                  <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                </div>
              </th>
              {/* Name search */}
              <th className="px-2 py-1 border-r border-gray-200">
                <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                  <span className="text-gray-400 text-xs select-none">→</span>
                  <input
                    type="text"
                    placeholder={t('common.search')}
                    value={searchName}
                    onChange={e => setSearchName(e.target.value)}
                    className="flex-1 text-xs outline-none min-w-0 bg-transparent"
                  />
                  <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                </div>
              </th>
              {/* Category filter */}
              <th className="px-2 py-1 border-r border-gray-200">
                <select
                  value={filterCategory}
                  onChange={e => setFilterCategory(e.target.value)}
                  className="w-full py-0.5 text-xs border border-gray-200 rounded outline-none bg-white"
                >
                  <option value="">{t('materials.allCategories')}</option>
                  {categories.map(c => <option key={c.id} value={String(c.id)}>{c.name}</option>)}
                </select>
              </th>
              {/* Unit filter */}
              <th className="px-2 py-1 border-r border-gray-200">
                <select
                  value={filterUnit}
                  onChange={e => setFilterUnit(e.target.value)}
                  className="w-full py-0.5 text-xs border border-gray-200 rounded outline-none bg-white"
                >
                  <option value="">{t('materials.allUnits')}</option>
                  {uniqueUnits.map(u => <option key={u} value={u}>{u}</option>)}
                </select>
              </th>
              {/* Description - no filter */}
              <th className="border-r border-gray-200"></th>
              {/* Date - no filter */}
              <th className="border-r border-gray-200"></th>
              {/* Created by - no filter */}
              <th className="border-r border-gray-200"></th>
              <th className="border-gray-200"></th>
            </tr>

          </thead>
          <tbody className="divide-y divide-gray-100">
            {paginatedItems.length === 0 ? (
              <tr>
                <td colSpan={10} className="px-4 py-12 text-center text-gray-400">
                  <Package className="w-10 h-10 mx-auto mb-2 opacity-40" />
                  <p>{t('materials.noItemsFound')}</p>
                </td>
              </tr>
            ) : (
              paginatedItems.map((item, idx) => {
                const globalIndex = (currentPage - 1) * itemsPerPage + idx + 1;
                const low = item.minStock != null && item.onHandQuantity < (item.minStock ?? 0);
                return (
                  <tr
                    key={item.id}
                    className={`hover:bg-blue-50 ${
                      selectedRows.has(item.id) ? 'bg-blue-50' : idx % 2 === 1 ? 'bg-gray-50/50' : 'bg-white'
                    }`}
                  >
                    {/* TT */}
                    <td className="px-2 py-2 text-center text-xs text-gray-500 border-r border-gray-100">
                      {globalIndex}
                    </td>
                    {/* Checkbox */}
                    <td className="px-2 py-2 text-center border-r border-gray-100">
                      <input
                        type="checkbox"
                        checked={selectedRows.has(item.id)}
                        onChange={() => toggleRow(item.id)}
                        className="rounded text-blue-600"
                      />
                    </td>
                    {/* Mã vật tư */}
                    <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100 font-mono">
                      {item.itemCode}
                    </td>
                    {/* Tên vật tư */}
                    <td className="px-3 py-2 border-r border-gray-100">
                      <button
                        onClick={() => { setEditingItem(item); setItemModalOpen(true); }}
                        className="flex items-center gap-1 text-blue-600 hover:underline font-medium text-xs text-left w-full"
                      >
                        <ChevronRight className="w-3 h-3 flex-shrink-0" />
                        <span className="marquee-cell flex-1 min-w-0">
                          <span className="marquee-text">{item.name}</span>
                        </span>
                      </button>
                      {low && (
                        <span className="ml-4 text-[10px] px-1.5 py-0.5 rounded-full bg-red-100 text-red-700 whitespace-nowrap">LOW</span>
                      )}
                    </td>
                    {/* Loại vật tư */}
                    <td className="px-3 py-2 text-xs border-r border-gray-100">
                      <span className="px-2 py-0.5 rounded-full bg-blue-50 text-blue-700 whitespace-nowrap">
                        {categoryMap.get(item.categoryId) || '—'}
                      </span>
                    </td>
                    {/* Đơn vị tính */}
                    <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100 text-center">
                      {item.unit}
                    </td>
                    {/* Mô tả */}
                    <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100 max-w-[200px]">
                      <div className="marquee-cell">
                        <span className="marquee-text">{item.specification || item.notes || ''}</span>
                      </div>
                    </td>
                    {/* Ngày cập nhật */}
                    <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100 text-center">
                      {formatDate(item.createdAt)}
                    </td>
                    {/* Người tạo */}
                    <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100 text-center">
                      —
                    </td>
                    {/* Actions */}
                    <td className="px-2 py-2">
                      <div className="flex items-center justify-center gap-0.5">
                        <button
                          onClick={() => { setEditingItem(item); setItemModalOpen(true); }}
                          className="p-1 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded"
                          title={t('materials.page.edit')}
                        >
                          <Edit2 className="w-3.5 h-3.5" />
                        </button>
                        <button
                          onClick={() => { setAdjustingItem(item); setStockAdjustmentModalOpen(true); }}
                          className="p-1 text-gray-400 hover:text-green-600 hover:bg-green-50 rounded"
                          title={t('materials.adjustStock')}
                        >
                          <Eye className="w-3.5 h-3.5" />
                        </button>
                        <button
                          onClick={() => handleDeleteItem(item)}
                          className="p-1 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded"
                          title={t('materials.page.delete')}
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* ── PAGINATION ── */}
      <div className="flex items-center justify-between px-4 py-2 border-t border-gray-200 bg-white flex-shrink-0 text-xs text-gray-600">
        {/* Left: per-page selector */}
        <div>
          <select
            value={itemsPerPage}
            onChange={e => { setItemsPerPage(Number(e.target.value)); setCurrentPage(1); }}
            className="border border-gray-300 rounded px-2 py-1 text-xs"
          >
            {ITEMS_PER_PAGE_OPTIONS.map(n => (
              <option key={n} value={n}>{t('pms.assets.perPage', { n })}</option>
            ))}
          </select>
        </div>

        {/* Middle: page buttons */}
        <div className="flex items-center gap-1">
          <span className="mr-2">
            {t('pms.assets.pageInfo', { current: currentPage, total: totalPages, records: filteredItems.length })}
          </span>
          <button
            onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
            disabled={currentPage === 1}
            className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40"
          >‹</button>
          {[...Array(Math.min(5, totalPages))].map((_, i) => {
            let page: number;
            if (totalPages <= 5) page = i + 1;
            else if (currentPage <= 3) page = i + 1;
            else if (currentPage >= totalPages - 2) page = totalPages - 4 + i;
            else page = currentPage - 2 + i;
            return (
              <button
                key={page}
                onClick={() => setCurrentPage(page)}
                className={`w-7 h-7 flex items-center justify-center border rounded text-xs ${
                  currentPage === page
                    ? 'bg-blue-600 text-white border-blue-600'
                    : 'border-gray-300 hover:bg-gray-50'
                }`}
              >
                {page}
              </button>
            );
          })}
          <button
            onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
            disabled={currentPage === totalPages}
            className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40"
          >›</button>
        </div>

        {/* Right: go to page */}
        <div className="flex items-center gap-2">
          <span>{t('pms.assets.goToPage')}</span>
          <input
            type="number"
            min={1}
            max={totalPages}
            value={currentPage}
            onChange={e => {
              const v = Number(e.target.value);
              if (v >= 1 && v <= totalPages) setCurrentPage(v);
            }}
            className="w-12 border border-gray-300 rounded px-1 py-1 text-center text-xs"
          />
        </div>
      </div>

      {/* ── MODALS ── */}
      <ItemFormModal
        isOpen={itemModalOpen}
        onClose={() => { setItemModalOpen(false); setEditingItem(null); }}
        onSubmit={editingItem ? handleUpdateItem : handleCreateItem}
        item={editingItem}
        categories={categories}
        title={editingItem ? t('materials.editItem') : t('materials.addItem')}
      />

      <CategoryFormModal
        isOpen={categoryModalOpen}
        onClose={() => { setCategoryModalOpen(false); setEditingCategory(null); }}
        onSubmit={editingCategory ? handleUpdateCategory : handleCreateCategory}
        category={editingCategory}
        categories={categories}
        title={editingCategory ? t('materials.editCategory') : t('materials.addCategory')}
      />

      <StockAdjustmentModal
        isOpen={stockAdjustmentModalOpen}
        onClose={() => { setStockAdjustmentModalOpen(false); setAdjustingItem(null); }}
        onSubmit={handleStockAdjustment}
        item={adjustingItem}
      />

      <ImportReceiptModal
        isOpen={importReceiptModalOpen}
        onClose={() => setImportReceiptModalOpen(false)}
        onSuccess={() => { loadData(); }}
      />
    </div>
  );
}
