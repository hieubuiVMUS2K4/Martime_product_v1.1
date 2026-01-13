import { useEffect, useMemo, useState } from 'react';
import { Boxes, Layers, Search, AlertTriangle, Tag, Edit2, Trash2, TrendingUp, FileSpreadsheet } from 'lucide-react';
import { materialService } from '../../services/materialService';
import { receiptService } from '../../services/receiptService';
import type { MaterialItem, MaterialCategory } from '../../types/maritime.types';
import type { CreateMaterialItemDto, UpdateMaterialItemDto, CreateMaterialCategoryDto, UpdateMaterialCategoryDto, StockAdjustmentDto } from '../../services/materialService';
import type { MaterialReceiptListDto } from '../../services/receiptService';
import { ItemFormModal } from './ItemFormModal';
import { CategoryFormModal } from './CategoryFormModal';
import { StockAdjustmentModal } from './StockAdjustmentModal';
import { ImportReceiptModal } from './ImportReceiptModal';
import { ReceiptDetailModal } from './ReceiptDetailModal';
import { useTranslationSafe } from '@/contexts/I18nContext';

type TabType = 'items' | 'low' | 'categories' | 'receipts';

export function MaterialPage() {
  const { t } = useTranslationSafe();
  const [activeTab, setActiveTab] = useState<TabType>('items');
  const [items, setItems] = useState<MaterialItem[]>([]);
  const [lowStock, setLowStock] = useState<MaterialItem[]>([]);
  const [categories, setCategories] = useState<MaterialCategory[]>([]);
  const [receipts, setReceipts] = useState<MaterialReceiptListDto[]>([]);
  const [totalReceipts, setTotalReceipts] = useState(0);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [categorySearch, setCategorySearch] = useState('');
  const [categoryId, setCategoryId] = useState<number | 'all'>('all');
  const [filterUnit, setFilterUnit] = useState<string>('all');

  // Modal states
  const [itemModalOpen, setItemModalOpen] = useState(false);
  const [categoryModalOpen, setCategoryModalOpen] = useState(false);
  const [stockAdjustmentModalOpen, setStockAdjustmentModalOpen] = useState(false);
  const [importReceiptModalOpen, setImportReceiptModalOpen] = useState(false);
  const [receiptDetailModalOpen, setReceiptDetailModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<MaterialItem | null>(null);
  const [editingCategory, setEditingCategory] = useState<MaterialCategory | null>(null);
  const [adjustingItem, setAdjustingItem] = useState<MaterialItem | null>(null);
  const [selectedReceiptId, setSelectedReceiptId] = useState<number | null>(null);

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 10;

  // State filter/sort cho bảng
  const [sortType, setSortType] = useState<{ col: string; dir: 'asc'|'desc' } | null>(null);
  const [sortMenu, setSortMenu] = useState<string | null>(null); // col name or null
  
  // Category sorting state
  const [categorySortType, setCategorySortType] = useState<{ col: string; dir: 'asc'|'desc' } | null>(null);
  const [categorySortMenu, setCategorySortMenu] = useState<string | null>(null);

  // Receipt sorting state - default: muộn nhất trước (latest first)
  const [receiptSortType, setReceiptSortType] = useState<{ col: string; dir: 'asc'|'desc' } | null>({ col: 'date', dir: 'desc' });
  const [receiptSortMenu, setReceiptSortMenu] = useState<string | null>(null);

  useEffect(() => {
    setCurrentPage(1); // Reset page when tab changes
  }, [activeTab]);

  useEffect(() => {
    loadData();
  }, [activeTab, currentPage]);

  const loadData = async () => {
    try {
      setLoading(true);
      if (activeTab === 'low') {
        const [ls, cats] = await Promise.all([
          materialService.getLowStockItems(),
          materialService.getCategories(true)
        ]);
        setLowStock(ls);
        setCategories(cats);
      } else if (activeTab === 'categories') {
        const cats = await materialService.getCategories(false);
        setCategories(cats);
      } else if (activeTab === 'receipts') {
        const response = await receiptService.getReceipts({
          page: currentPage,
          pageSize: itemsPerPage
        });
        setReceipts(response.data);
        setTotalReceipts(response.totalRecords);
      } else {
        // Load items, categories, and lowStock for stats
        const [its, cats, ls] = await Promise.all([
          materialService.getItems({ onlyActive: true }),
          materialService.getCategories(true),
          materialService.getLowStockItems()
        ]);
        setItems(its);
        setCategories(cats);
        setLowStock(ls);
      }
    } catch (e) {
      console.error('Failed to load material data:', e);
    } finally {
      setLoading(false);
    }
  };

  // Item handlers
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
    if (!confirm(`Are you sure you want to delete "${item.name}"?`)) return;
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

  // Category handlers
  const handleCreateCategory = async (data: CreateMaterialCategoryDto) => {
    await materialService.createCategory(data);
    await loadData();
  };

  const handleUpdateCategory = async (data: UpdateMaterialCategoryDto) => {
    if (!editingCategory) return;
    await materialService.updateCategory(editingCategory.id, data);
    setEditingCategory(null);
    await loadData();
  };

  const handleDeleteCategory = async (category: MaterialCategory) => {
    if (!confirm(`Are you sure you want to delete category "${category.name}"?`)) return;
    try {
      await materialService.deleteCategory(category.id);
      await loadData();
    } catch (error: any) {
      alert(error.message || 'Failed to delete category');
    }
  };

  const filteredItems = useMemo(() => {
    let data = [...items];
    if (categoryId !== 'all') data = data.filter(x => x.categoryId === categoryId);
    if (filterUnit !== 'all') data = data.filter(x => x.unit === filterUnit);
    if (search) {
      const q = search.toLowerCase();
      data = data.filter(x =>
        x.itemCode.toLowerCase().includes(q) ||
        x.name.toLowerCase().includes(q) ||
        (x.partNumber && x.partNumber.toLowerCase().includes(q)) ||
        (x.barcode && x.barcode.toLowerCase().includes(q)) ||
        (x.manufacturer && x.manufacturer.toLowerCase().includes(q))
      );
    }
    return data;
  }, [items, search, categoryId, filterUnit]);

  // Sorting for items
  const sortedItems = useMemo(() => {
    if (!sortType) return filteredItems;
    const sorted = [...filteredItems];
    switch (sortType.col) {
      case 'name':
        sorted.sort((a, b) => {
          return sortType.dir === 'asc'
            ? a.name.localeCompare(b.name)
            : b.name.localeCompare(a.name);
        });
        break;
      case 'category':
        sorted.sort((a, b) => {
          const aCat = categories.find(c => c.id === a.categoryId)?.name || '';
          const bCat = categories.find(c => c.id === b.categoryId)?.name || '';
          return sortType.dir === 'asc'
            ? aCat.localeCompare(bCat)
            : bCat.localeCompare(aCat);
        });
        break;
      case 'stock':
        sorted.sort((a, b) => {
          return sortType.dir === 'asc' 
            ? a.onHandQuantity - b.onHandQuantity 
            : b.onHandQuantity - a.onHandQuantity;
        });
        break;
      case 'unitCost':
        sorted.sort((a, b) => {
          const aCost = a.unitCost || 0;
          const bCost = b.unitCost || 0;
          return sortType.dir === 'asc' ? aCost - bCost : bCost - aCost;
        });
        break;
      default:
        break;
    }
    return sorted;
  }, [filteredItems, sortType, categories]);

  const filteredCategories = useMemo(() => {
    if (!categorySearch) return categories;
    const q = categorySearch.toLowerCase();
    return categories.filter(cat =>
      cat.name.toLowerCase().includes(q) ||
      cat.categoryCode.toLowerCase().includes(q) ||
      (cat.description && cat.description.toLowerCase().includes(q))
    );
  }, [categories, categorySearch]);

  // Sorting for categories
  const sortedCategories = useMemo(() => {
    if (!categorySortType) return filteredCategories;
    const sorted = [...filteredCategories];
    switch (categorySortType.col) {
      case 'name':
        sorted.sort((a, b) => {
          return categorySortType.dir === 'asc'
            ? a.name.localeCompare(b.name)
            : b.name.localeCompare(a.name);
        });
        break;
      case 'code':
        sorted.sort((a, b) => {
          return categorySortType.dir === 'asc'
            ? a.categoryCode.localeCompare(b.categoryCode)
            : b.categoryCode.localeCompare(a.categoryCode);
        });
        break;
      case 'status':
        sorted.sort((a, b) => {
          const aActive = a.isActive ? 1 : 0;
          const bActive = b.isActive ? 1 : 0;
          return categorySortType.dir === 'asc' ? aActive - bActive : bActive - aActive;
        });
        break;
      default:
        break;
    }
    return sorted;
  }, [filteredCategories, categorySortType]);

  // Sorting for receipts
  const sortedReceipts = useMemo(() => {
    if (!receiptSortType) return receipts;
    const sorted = [...receipts];
    switch (receiptSortType.col) {
      case 'date':
        sorted.sort((a, b) => {
          const dateA = new Date(a.receiptDate).getTime();
          const dateB = new Date(b.receiptDate).getTime();
          return receiptSortType.dir === 'asc' ? dateA - dateB : dateB - dateA;
        });
        break;
      case 'amount':
        sorted.sort((a, b) => {
          const amountA = a.totalAmount || 0;
          const amountB = b.totalAmount || 0;
          return receiptSortType.dir === 'asc' ? amountA - amountB : amountB - amountA;
        });
        break;
      case 'items':
        sorted.sort((a, b) => {
          return receiptSortType.dir === 'asc' ? a.itemCount - b.itemCount : b.itemCount - a.itemCount;
        });
        break;
      default:
        break;
    }
    return sorted;
  }, [receipts, receiptSortType]);

  // Pagination for items
  const paginatedItems = useMemo(() => {
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    return sortedItems.slice(startIndex, endIndex);
  }, [sortedItems, currentPage, itemsPerPage]);

  const totalPages = Math.ceil(sortedItems.length / itemsPerPage);

  // Reset to page 1 when filter changes
  useEffect(() => {
    setCurrentPage(1);
  }, [search, categoryId, filterUnit]);

  // Get unique units
  const uniqueUnits = useMemo(() => {
    return [...new Set(items.map(item => item.unit))].sort();
  }, [items]);

  return (
    <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
      <div className="p-6 space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">{t('materials.title')}</h1>
            <p className="text-sm text-gray-600 mt-1">{t('materials.subtitle')}</p>
          </div>
          <div className="flex gap-2">
            <button 
              onClick={() => {
                setImportReceiptModalOpen(true);
              }}
              className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
            >
              <FileSpreadsheet className="w-5 h-5" /> {t('materials.importReceipt')}
            </button>
            <button 
              onClick={() => {
                setEditingCategory(null);
                setCategoryModalOpen(true);
              }}
              className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              <Layers className="w-5 h-5" /> {t('materials.addCategory')}
            </button>
          </div>
        </div>

        {/* Tabs */}
        <div className="bg-white rounded-lg shadow">
          <div className="border-b border-gray-200">
            <nav className="flex -mb-px">
              <TabButton active={activeTab === 'items'} onClick={() => setActiveTab('items')} icon={<Boxes className="w-5 h-5" />} label={t('materials.tabs.items')} />
              <TabButton active={activeTab === 'low'} onClick={() => setActiveTab('low')} icon={<AlertTriangle className="w-5 h-5" />} label={t('materials.tabs.lowStock')} />
              <TabButton active={activeTab === 'categories'} onClick={() => setActiveTab('categories')} icon={<Layers className="w-5 h-5" />} label={t('materials.tabs.categories')} />
              <TabButton active={activeTab === 'receipts'} onClick={() => setActiveTab('receipts')} icon={<FileSpreadsheet className="w-5 h-5" />} label={t('materials.tabs.receipts')} />
            </nav>
          </div>

          {/* Filters (Items tab) */}
          {activeTab === 'items' && (
            <div className="p-4 border-b border-gray-200 flex items-center gap-4">
              <div className="relative flex-1">
                <Search className="w-4 h-4 text-gray-400 absolute left-3 top-3" />
                <input
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  placeholder={t('materials.searchPlaceholder')}
                  className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <select
                value={categoryId}
                onChange={(e) => setCategoryId(e.target.value === 'all' ? 'all' : Number(e.target.value))}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 min-w-[180px]"
              >
                <option value="all">{t('materials.allCategories')}</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
              <select
                value={filterUnit}
                onChange={(e) => setFilterUnit(e.target.value)}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 min-w-[150px]"
              >
                <option value="all">{t('materials.allUnits')}</option>
                {uniqueUnits.map(unit => <option key={unit} value={unit}>{unit}</option>)}
              </select>
            </div>
          )}

          {/* Filters (Categories tab) */}
          {activeTab === 'categories' && (
            <div className="p-4 border-b border-gray-200">
              <div className="relative">
                <Search className="w-4 h-4 text-gray-400 absolute left-3 top-3" />
                <input
                  value={categorySearch}
                  onChange={(e) => setCategorySearch(e.target.value)}
                  placeholder={t('materials.categorySearchPlaceholder')}
                  className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>
          )}

          {/* Content */}
          <div className="p-6">
            {loading ? (
              <div className="text-center py-12">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                <p className="text-gray-600 mt-4">{t('materials.loading')}</p>
              </div>
            ) : (
              <>
                {activeTab === 'categories' ? (
                  <CategoryList 
                    categories={sortedCategories}
                    onEdit={(cat) => {
                      setEditingCategory(cat);
                      setCategoryModalOpen(true);
                    }}
                    onDelete={handleDeleteCategory}
                    sortType={categorySortType}
                    setSortType={setCategorySortType}
                    sortMenu={categorySortMenu}
                    setSortMenu={setCategorySortMenu}
                  />
                ) : activeTab === 'receipts' ? (
                  <ReceiptList 
                    receipts={sortedReceipts}
                    currentPage={currentPage}
                    itemsPerPage={itemsPerPage}
                    totalReceipts={totalReceipts}
                    setCurrentPage={setCurrentPage}
                    onReceiptClick={(receiptId) => {
                      setSelectedReceiptId(receiptId);
                      setReceiptDetailModalOpen(true);
                    }}
                    onImportClick={() => setImportReceiptModalOpen(true)}
                    sortType={receiptSortType}
                    setSortType={setReceiptSortType}
                    sortMenu={receiptSortMenu}
                    setSortMenu={setReceiptSortMenu}
                  />
                ) : activeTab === 'low' ? (
                  <ItemList 
                    items={lowStock} 
                    categories={categories} 
                    highlightLow
                    onEdit={(item) => {
                      setEditingItem(item);
                      setItemModalOpen(true);
                    }}
                    onDelete={handleDeleteItem}
                    onAdjustStock={(item) => {
                      setAdjustingItem(item);
                      setStockAdjustmentModalOpen(true);
                    }}
                  />
                ) : (
                  <div>
                    {/* Info and Pagination */}
                    <div className="flex items-center justify-between mb-4">
                      {/* Left - Display info */}
                      <div className="text-sm text-gray-600">
                        {t('materials.showingItems', { start: sortedItems.length === 0 ? 0 : (currentPage - 1) * itemsPerPage + 1, end: Math.min(currentPage * itemsPerPage, sortedItems.length), total: sortedItems.length })}
                      </div>

                      {/* Right - Pagination */}
                      {totalPages > 1 && (
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                            disabled={currentPage === 1}
                            className="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-30 disabled:cursor-not-allowed"
                          >
                            {t('materials.previous')}
                          </button>
                          
                          <span className="text-sm text-gray-600 px-2">
                            {t('materials.pageOf', { current: currentPage, total: totalPages })}
                          </span>

                          <button
                            onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                            disabled={currentPage === totalPages}
                            className="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-30 disabled:cursor-not-allowed"
                          >
                            {t('materials.next')}
                          </button>
                        </div>
                      )}
                    </div>

                    <ItemList 
                      items={paginatedItems} 
                      categories={categories}
                      onEdit={(item) => {
                        setEditingItem(item);
                        setItemModalOpen(true);
                      }}
                      onDelete={handleDeleteItem}
                      onAdjustStock={(item) => {
                        setAdjustingItem(item);
                        setStockAdjustmentModalOpen(true);
                      }}
                      currentPage={currentPage}
                      itemsPerPage={itemsPerPage}
                      sortType={sortType}
                      setSortType={setSortType}
                      sortMenu={sortMenu}
                      setSortMenu={setSortMenu}
                    />
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      </div>

      {/* Modals */}
      <ItemFormModal
        isOpen={itemModalOpen}
        onClose={() => {
          setItemModalOpen(false);
          setEditingItem(null);
        }}
        onSubmit={editingItem ? handleUpdateItem : handleCreateItem}
        item={editingItem}
        categories={categories}
        title={editingItem ? t('materials.editItem') : t('materials.addItem')}
      />

      <CategoryFormModal
        isOpen={categoryModalOpen}
        onClose={() => {
          setCategoryModalOpen(false);
          setEditingCategory(null);
        }}
        onSubmit={editingCategory ? handleUpdateCategory : handleCreateCategory}
        category={editingCategory}
        categories={categories}
        title={editingCategory ? t('materials.editCategory') : t('materials.addCategory')}
      />

      <StockAdjustmentModal
        isOpen={stockAdjustmentModalOpen}
        onClose={() => {
          setStockAdjustmentModalOpen(false);
          setAdjustingItem(null);
        }}
        onSubmit={handleStockAdjustment}
        item={adjustingItem}
      />

      <ImportReceiptModal
        isOpen={importReceiptModalOpen}
        onClose={() => setImportReceiptModalOpen(false)}
        onSuccess={() => {
          loadData();
          alert('Import successful!');
        }}
      />

      <ReceiptDetailModal
        isOpen={receiptDetailModalOpen}
        onClose={() => {
          setReceiptDetailModalOpen(false);
          setSelectedReceiptId(null);
        }}
        receiptId={selectedReceiptId || 0}
      />
    </div>
  );
}

function ItemList({ items, highlightLow = false, categories, onEdit, onDelete, onAdjustStock, currentPage, itemsPerPage, sortType, setSortType, sortMenu, setSortMenu }: { 
  items: MaterialItem[]; 
  highlightLow?: boolean; 
  categories: MaterialCategory[];
  onEdit: (item: MaterialItem) => void;
  onDelete: (item: MaterialItem) => void;
  onAdjustStock: (item: MaterialItem) => void;
  currentPage?: number;
  itemsPerPage?: number;
  sortType?: { col: string; dir: 'asc'|'desc' } | null;
  setSortType?: (sortType: { col: string; dir: 'asc'|'desc' } | null) => void;
  sortMenu?: string | null;
  setSortMenu?: (sortMenu: string | null) => void;
}) {
  const { t } = useTranslationSafe()
  const getCategoryName = (catId: number) => {
    const cat = categories.find(c => c.id === catId);
    return cat?.name || 'Unknown';
  };

  // SortDropdown component
  function SortDropdown({ col, options, sortType, setSortType, sortMenu, setSortMenu }: {
    col: string;
    options: Array<{ label: string; dir: 'asc'|'desc' }>;
    sortType: any;
    setSortType: any;
    sortMenu: any;
    setSortMenu: any;
  }) {
    return (
      <div className="absolute top-1/2 right-0 -translate-y-1/2" style={{zIndex:2}}>
        <button
          className="text-gray-400 hover:text-blue-600 text-base p-1"
          onClick={e => { e.stopPropagation(); setSortMenu(sortMenu === col ? null : col) }}
          style={{lineHeight:0}}
        >
          ▼
        </button>
        {sortMenu === col && (
          <div className="absolute right-0 mt-6 w-40 bg-white border border-gray-200 rounded shadow-lg z-20">
            {options.map(opt => (
              <button
                key={opt.label}
                className={`block w-full text-left px-3 py-2 text-sm hover:bg-blue-50 ${sortType?.col === col && sortType?.dir === opt.dir ? 'text-blue-600 font-bold' : 'text-gray-700'}`}
                onClick={e => { e.stopPropagation(); setSortType({col,dir:opt.dir}); setSortMenu(null) }}
              >
                {opt.label}
              </button>
            ))}
          </div>
        )}
      </div>
    )
  }

  return (
    <div className="overflow-x-auto border border-gray-200 rounded-lg">
      <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
        <thead className="bg-gray-50">
          <tr>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative" style={{width: '5%'}}>STT</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative" style={{position:'relative', width: '22%'}}>
              Tên vật tư
              {setSortType && setSortMenu && (
                <SortDropdown col="name" options={[{label:'Sắp xếp từ A-Z',dir:'asc'},{label:'Sắp xếp từ Z-A',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative" style={{position:'relative', width: '18%'}}>
              Danh mục
              {setSortType && setSortMenu && (
                <SortDropdown col="category" options={[{label:'Sắp xếp từ A-Z',dir:'asc'},{label:'Sắp xếp từ Z-A',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300" style={{width: '6%'}}>Đơn vị</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative" style={{position:'relative', width: '8%'}}>
              Tồn kho
              {setSortType && setSortMenu && (
                <SortDropdown col="stock" options={[{label:'Sắp xếp tăng dần',dir:'asc'},{label:'Sắp xếp giảm dần',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300" style={{width: '12%'}}>Min / Max</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300" style={{width: '11%'}}>Mã linh kiện</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative" style={{position:'relative', width: '10%'}}>
              Đơn giá
              {setSortType && setSortMenu && (
                <SortDropdown col="unitCost" options={[{label:'Sắp xếp tăng dần',dir:'asc'},{label:'Sắp xếp giảm dần',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300" style={{width: '11%'}}>Trạng thái</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '7%'}}>Thao tác</th>
          </tr>
        </thead>
        <tbody className="bg-white">
          {items.map((it, index) => {
            const low = it.minStock != null && it.onHandQuantity < (it.minStock ?? 0);
            const over = it.maxStock != null && it.onHandQuantity > (it.maxStock ?? 0);
            const totalValue = it.unitCost ? (it.unitCost * it.onHandQuantity) : null;
            const globalIndex = currentPage && itemsPerPage ? (currentPage - 1) * itemsPerPage + index + 1 : index + 1;
            
            return (
              <tr 
                key={it.id} 
                className={`cursor-pointer hover:bg-gray-50 transition-colors border-b border-gray-200 ${highlightLow && low ? 'bg-red-50' : ''}`}
                onClick={(e) => {
                  // Don't trigger if clicking action buttons
                  if ((e.target as HTMLElement).closest('button')) return;
                  onEdit(it);
                }}
              >
                <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-900 text-center border-r border-gray-300" style={{width: '5%'}}>{globalIndex}</td>
                <td className="px-4 py-3 border-r border-gray-300 overflow-hidden" style={{width: '22%'}}>
                  <div className="text-sm font-medium text-gray-900 truncate">{it.name}</div>
                  {it.specification && (
                    <div className="text-xs text-gray-500 mt-1 truncate">{it.specification}</div>
                  )}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300 overflow-hidden" style={{width: '18%'}}>
                  <div className="flex justify-center">
                    <span className="text-xs px-2 py-1 rounded-full bg-blue-100 text-blue-700 truncate max-w-full inline-block">
                      {getCategoryName(it.categoryId)}
                    </span>
                  </div>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300 overflow-hidden" style={{width: '6%'}}>
                  <span className="text-xs truncate">{it.unit}</span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300 overflow-hidden" style={{width: '8%'}}>
                  <span className={`text-sm font-medium ${low ? 'text-red-600' : over ? 'text-orange-600' : 'text-gray-900'}`}>
                    {it.onHandQuantity.toFixed(2)}
                  </span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300 overflow-hidden" style={{width: '12%'}}>
                  <span className="text-xs text-gray-600 truncate">
                    {it.minStock != null ? it.minStock.toFixed(2) : '-'} / {it.maxStock != null ? it.maxStock.toFixed(2) : '-'}
                  </span>
                </td>
                <td className="px-4 py-3 border-r border-gray-300 overflow-hidden" style={{width: '11%'}}>
                  <div className="text-xs font-mono text-gray-700 truncate">{it.partNumber || '-'}</div>
                  {it.barcode && <div className="text-xs text-gray-400 mt-0.5 truncate">🔖 {it.barcode}</div>}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300 overflow-hidden" style={{width: '10%'}}>
                  <div className="flex flex-col items-center">
                    {it.unitCost ? (
                      <>
                        <span className="text-xs font-medium text-gray-900 truncate">{it.unitCost.toFixed(2)} {it.currency || 'USD'}</span>
                        {totalValue && <span className="text-xs text-gray-500 truncate">= {totalValue.toFixed(2)}</span>}
                      </>
                    ) : (
                      <span className="text-xs text-gray-400">-</span>
                    )}
                  </div>
                </td>
                <td className="px-4 py-3 text-center border-r border-gray-300 overflow-hidden" style={{width: '11%'}}>
                  <div className="flex flex-wrap gap-1 justify-center">
                    {low && <span className="px-1 py-0.5 text-xs rounded-full bg-red-100 text-red-700 whitespace-nowrap">LOW</span>}
                    {over && <span className="px-1 py-0.5 text-xs rounded-full bg-orange-100 text-orange-700 whitespace-nowrap">OVER</span>}
                    {it.serialTracked && <span className="px-1 py-0.5 text-xs rounded-full bg-purple-100 text-purple-700 whitespace-nowrap">SN</span>}
                    {it.batchTracked && <span className="px-1 py-0.5 text-xs rounded-full bg-yellow-100 text-yellow-700 whitespace-nowrap">BATCH</span>}
                    {it.expiryRequired && <span className="px-1 py-0.5 text-xs rounded-full bg-blue-100 text-blue-700 whitespace-nowrap">EXP</span>}
                    {!it.isActive && <span className="px-1 py-0.5 text-xs rounded-full bg-gray-100 text-gray-600 whitespace-nowrap">INACTIVE</span>}
                  </div>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center" style={{width: '8%'}}>
                  <div className="flex items-center justify-center gap-1.5">
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        onAdjustStock(it);
                      }}
                      className="p-1.5 text-green-600 hover:bg-green-50 rounded transition-colors"
                      title={t('materials.adjustStock')}
                    >
                      <TrendingUp className="w-4 h-4" />
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        onDelete(it);
                      }}
                      className="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors"
                      title={t('common.delete')}
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>

      {items.length === 0 && (
        <div className="text-center py-12">
          <Boxes className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-500">{t('materials.noItemsFound')}</p>
        </div>
      )}
    </div>
  );
}

function ReceiptList({ receipts, currentPage, itemsPerPage, totalReceipts, setCurrentPage, onReceiptClick, onImportClick, sortType, setSortType, sortMenu, setSortMenu }: {
  receipts: MaterialReceiptListDto[];
  currentPage: number;
  itemsPerPage: number;
  totalReceipts: number;
  setCurrentPage: (page: number) => void;
  onReceiptClick: (receiptId: number) => void;
  onImportClick: () => void;
  sortType?: { col: string; dir: 'asc'|'desc' } | null;
  setSortType?: (sortType: { col: string; dir: 'asc'|'desc' } | null) => void;
  sortMenu?: string | null;
  setSortMenu?: (sortMenu: string | null) => void;
}) {
  // SortDropdown component
  function SortDropdown({ col, options, sortType, setSortType, sortMenu, setSortMenu }: {
    col: string;
    options: Array<{ label: string; dir: 'asc'|'desc' }>;
    sortType: any;
    setSortType: any;
    sortMenu: any;
    setSortMenu: any;
  }) {
    return (
      <div className="absolute top-1/2 right-2 -translate-y-1/2" style={{zIndex:10}}>
        <button
          className="text-gray-400 hover:text-blue-600 text-base p-1"
          onClick={e => { e.stopPropagation(); setSortMenu(sortMenu === col ? null : col) }}
          style={{lineHeight:0}}
        >
          ▼
        </button>
        {sortMenu === col && (
          <div className="absolute right-0 top-full mt-1 w-48 bg-white border border-gray-200 rounded shadow-lg" style={{zIndex:50}}>
            {options.map(opt => (
              <button
                key={opt.label}
                className={`block w-full text-left px-3 py-2 text-sm hover:bg-blue-50 ${sortType?.col === col && sortType?.dir === opt.dir ? 'text-blue-600 font-bold' : 'text-gray-700'}`}
                onClick={e => { e.stopPropagation(); setSortType({col,dir:opt.dir}); setSortMenu(null) }}
              >
                {opt.label}
              </button>
            ))}
          </div>
        )}
      </div>
    )
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-4">
        <h3 className="text-lg font-semibold text-gray-900">Import Receipts History</h3>
        <button 
          onClick={onImportClick}
          className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
        >
          <FileSpreadsheet className="w-4 h-4" />
          Import New Receipt
        </button>
      </div>
      
      {receipts.length > 0 ? (
        <>
          <div className="overflow-x-auto bg-white rounded-lg border border-gray-200">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase border-r border-gray-300" style={{width: '8%'}}>STT</th>
                  <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase border-r border-gray-300" style={{width: '18%'}}>Receipt Code</th>
                  <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase border-r border-gray-300 relative" style={{position:'relative', width: '18%'}}>
                    Receipt Date
                    {setSortType && setSortMenu && (
                      <SortDropdown col="date" options={[{label:'Sớm nhất trước',dir:'asc'},{label:'Muộn nhất trước',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
                    )}
                  </th>
                  <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase border-r border-gray-300 relative" style={{position:'relative', width: '20%'}}>
                    Total Amount
                    {setSortType && setSortMenu && (
                      <SortDropdown col="amount" options={[{label:'Từ nhỏ đến lớn',dir:'asc'},{label:'Từ lớn đến nhỏ',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
                    )}
                  </th>
                  <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase border-r border-gray-300 relative" style={{position:'relative', width: '18%'}}>
                    Items
                    {setSortType && setSortMenu && (
                      <SortDropdown col="items" options={[{label:'Từ ít đến nhiều',dir:'asc'},{label:'Từ nhiều đến ít',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
                    )}
                  </th>
                  <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase" style={{width: '18%'}}>Status</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {receipts.map((receipt: MaterialReceiptListDto, idx: number) => (
                  <tr 
                    key={receipt.receiptCode} 
                    className="hover:bg-gray-50 cursor-pointer"
                    onClick={() => onReceiptClick(receipt.id)}
                  >
                    <td className="px-4 py-3 text-center border-r border-gray-300">{(currentPage - 1) * itemsPerPage + idx + 1}</td>
                    <td className="px-4 py-3 text-center border-r border-gray-300">
                      <span className="font-medium text-blue-600">{receipt.receiptCode}</span>
                    </td>
                    <td className="px-4 py-3 text-center border-r border-gray-300">
                      {new Date(receipt.receiptDate).toLocaleDateString('vi-VN')}
                    </td>
                    <td className="px-4 py-3 text-center border-r border-gray-300">
                      <span className="font-semibold text-gray-900">
                        {receipt.totalAmount ? receipt.totalAmount.toLocaleString('vi-VN') : '0'} {receipt.currency}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-center border-r border-gray-300">
                      <span className="px-2 py-1 bg-gray-100 text-gray-700 rounded-full text-sm">
                        {receipt.itemCount} items
                      </span>
                    </td>
                    <td className="px-4 py-3 text-center">
                      <span className={`px-2 py-1 rounded-full text-xs ${
                        receipt.status === 'Completed' ? 'bg-green-100 text-green-700' : 
                        receipt.status === 'Pending' ? 'bg-yellow-100 text-yellow-700' : 
                        'bg-gray-100 text-gray-700'
                      }`}>
                        {receipt.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          
          {/* Pagination */}
          <div className="flex justify-between items-center mt-4">
            <p className="text-sm text-gray-600">
              Showing {(currentPage - 1) * itemsPerPage + 1} - {Math.min(currentPage * itemsPerPage, totalReceipts)} of {totalReceipts} receipts
            </p>
            <div className="flex gap-2">
              <button
                onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
                disabled={currentPage === 1}
                className="px-3 py-1 border rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Previous
              </button>
              <span className="px-3 py-1 border rounded bg-blue-50 text-blue-600 font-medium">
                {currentPage}
              </span>
              <button
                onClick={() => setCurrentPage(currentPage + 1)}
                disabled={currentPage * itemsPerPage >= totalReceipts}
                className="px-3 py-1 border rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          </div>
        </>
      ) : (
        <div className="text-center py-12 bg-white rounded-lg border border-gray-200">
          <FileSpreadsheet className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-500 mb-4">No receipts found. Import your first receipt to get started.</p>
          <button 
            onClick={onImportClick}
            className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
          >
            Import New Receipt
          </button>
        </div>
      )}
    </div>
  );
}

function CategoryList({ categories, onEdit, onDelete, sortType, setSortType, sortMenu, setSortMenu }: { 
  categories: MaterialCategory[];
  onEdit: (category: MaterialCategory) => void;
  onDelete: (category: MaterialCategory) => void;
  sortType?: { col: string; dir: 'asc'|'desc' } | null;
  setSortType?: (sortType: { col: string; dir: 'asc'|'desc' } | null) => void;
  sortMenu?: string | null;
  setSortMenu?: (sortMenu: string | null) => void;
}) {
  // SortDropdown component for categories
  function SortDropdown({ col, options, sortType, setSortType, sortMenu, setSortMenu }: {
    col: string;
    options: Array<{ label: string; dir: 'asc'|'desc' }>;
    sortType: any;
    setSortType: any;
    sortMenu: any;
    setSortMenu: any;
  }) {
    return (
      <div className="absolute top-1/2 right-0 -translate-y-1/2" style={{zIndex:2}}>
        <button
          className="text-gray-400 hover:text-blue-600 text-base p-1"
          onClick={e => { e.stopPropagation(); setSortMenu(sortMenu === col ? null : col) }}
          style={{lineHeight:0}}
        >
          ▼
        </button>
        {sortMenu === col && (
          <div className="absolute right-0 mt-6 w-40 bg-white border border-gray-200 rounded shadow-lg z-20">
            {options.map(opt => (
              <button
                key={opt.label}
                className={`block w-full text-left px-3 py-2 text-sm hover:bg-blue-50 ${sortType?.col === col && sortType?.dir === opt.dir ? 'text-blue-600 font-bold' : 'text-gray-700'}`}
                onClick={e => { e.stopPropagation(); setSortType({col,dir:opt.dir}); setSortMenu(null) }}
              >
                {opt.label}
              </button>
            ))}
          </div>
        )}
      </div>
    )
  }

  return (
    <div className="overflow-x-auto border border-gray-200 rounded-lg">
      <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
        <thead className="bg-gray-50">
          <tr>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative" style={{width: '8%'}}>STT</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative" style={{position:'relative', width: '25%'}}>
              Tên danh mục
              {setSortType && setSortMenu && (
                <SortDropdown col="name" options={[{label:'Sắp xếp từ A-Z',dir:'asc'},{label:'Sắp xếp từ Z-A',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative" style={{position:'relative', width: '15%'}}>
              Mã danh mục
              {setSortType && setSortMenu && (
                <SortDropdown col="code" options={[{label:'Sắp xếp từ A-Z',dir:'asc'},{label:'Sắp xếp từ Z-A',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300" style={{width: '25%'}}>Mô tả</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300" style={{width: '15%'}}>Danh mục cha</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative" style={{position:'relative', width: '12%'}}>
              Trạng thái
              {setSortType && setSortMenu && (
                <SortDropdown col="status" options={[{label:'Hoạt động trước',dir:'desc'},{label:'Không hoạt động trước',dir:'asc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '10%'}}>Thao tác</th>
          </tr>
        </thead>
        <tbody className="bg-white">
          {categories.map((cat, index) => {
            const parentCat = cat.parentCategoryId ? categories.find(c => c.id === cat.parentCategoryId) : null;
            return (
              <tr 
                key={cat.id} 
                className="cursor-pointer hover:bg-gray-50 transition-colors border-b border-gray-200"
                onClick={(e) => {
                  // Don't trigger if clicking action buttons
                  if ((e.target as HTMLElement).closest('button')) return;
                  onEdit(cat);
                }}
              >
                <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-900 text-center border-r border-gray-300" style={{width: '8%'}}>{index + 1}</td>
                <td className="px-4 py-3 border-r border-gray-300" style={{width: '25%'}}>
                  <div className="flex items-center gap-2">
                    <Tag className="w-4 h-4 text-blue-600 flex-shrink-0" />
                    <div className="text-sm font-medium text-gray-900 truncate">{cat.name}</div>
                  </div>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300" style={{width: '15%'}}>
                  <span className="text-xs font-mono text-gray-700">{cat.categoryCode}</span>
                </td>
                <td className="px-4 py-3 border-r border-gray-300" style={{width: '25%'}}>
                  <div className="text-xs text-gray-600 line-clamp-2">
                    {cat.description || '-'}
                  </div>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300" style={{width: '15%'}}>
                  {parentCat ? (
                    <span className="text-xs px-2 py-1 rounded-full bg-gray-100 text-gray-700 truncate">
                      {parentCat.name}
                    </span>
                  ) : (
                    <span className="text-xs text-gray-400">Root</span>
                  )}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300" style={{width: '12%'}}>
                  <span className={`text-xs px-2 py-1 rounded-full ${cat.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'}`}>
                    {cat.isActive ? 'Hoạt động' : 'Không hoạt động'}
                  </span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center" style={{width: '10%'}}>
                  <div className="flex items-center justify-center gap-1.5">
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        onEdit(cat);
                      }}
                      className="p-1.5 text-blue-600 hover:bg-blue-50 rounded transition-colors"
                      title="Sửa"
                    >
                      <Edit2 className="w-4 h-4" />
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        onDelete(cat);
                      }}
                      className="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors"
                      title="Xóa"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>

      {categories.length === 0 && (
        <div className="text-center py-12">
          <Layers className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-500">No categories found</p>
        </div>
      )}
    </div>
  );
}

function TabButton({ active, onClick, icon, label }: { active: boolean; onClick: () => void; icon: React.ReactNode; label: string }) {
  return (
    <button
      onClick={onClick}
      className={`flex items-center gap-2 px-6 py-3 border-b-2 font-medium text-sm transition-colors ${
        active ? 'border-blue-600 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
      }`}
    >
      {icon}{label}
    </button>
  );
}