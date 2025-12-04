import { useEffect, useMemo, useState } from 'react';
import { Wrench, Layers, Plus, Search, Tag, Edit2, Trash2, FileSpreadsheet } from 'lucide-react';
import { equipmentService } from '../../services/equipmentService';
import { equipmentReceiptService } from '../../services/equipmentReceiptService';
import type { EquipmentItem, EquipmentCategory } from '../../services/equipmentService';
import type { CreateEquipmentItemDto, UpdateEquipmentItemDto, CreateEquipmentCategoryDto, UpdateEquipmentCategoryDto } from '../../services/equipmentService';
import type { EquipmentReceiptListDto } from '../../services/equipmentReceiptService';
import { ItemFormModal } from './ItemFormModal';
import { CategoryFormModal } from './CategoryFormModal';
import { ImportEquipmentReceiptModal } from './ImportEquipmentReceiptModal';
import { EquipmentReceiptDetailModal } from './EquipmentReceiptDetailModal';

type TabType = 'items' | 'categories' | 'receipts';

export function EquipmentPage() {
  const [activeTab, setActiveTab] = useState<TabType>('items');
  const [items, setItems] = useState<EquipmentItem[]>([]);
  const [categories, setCategories] = useState<EquipmentCategory[]>([]);
  const [receipts, setReceipts] = useState<EquipmentReceiptListDto[]>([]);
  const [totalReceipts, setTotalReceipts] = useState(0);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [categorySearch, setCategorySearch] = useState('');
  const [categoryId, setCategoryId] = useState<number | 'all'>('all');
  const [filterStatus, setFilterStatus] = useState<string>('all');

  // Modal states
  const [itemModalOpen, setItemModalOpen] = useState(false);
  const [categoryModalOpen, setCategoryModalOpen] = useState(false);
  const [importModalOpen, setImportModalOpen] = useState(false);
  const [receiptDetailModalOpen, setReceiptDetailModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<EquipmentItem | null>(null);
  const [editingCategory, setEditingCategory] = useState<EquipmentCategory | null>(null);
  const [selectedReceiptId, setSelectedReceiptId] = useState<number | null>(null);

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 10;

  // Sorting state
  const [sortType, setSortType] = useState<{ col: string; dir: 'asc'|'desc' } | null>(null);
  const [sortMenu, setSortMenu] = useState<string | null>(null);
  
  // Category sorting state
  const [categorySortType, setCategorySortType] = useState<{ col: string; dir: 'asc'|'desc' } | null>(null);
  const [categorySortMenu, setCategorySortMenu] = useState<string | null>(null);

  useEffect(() => {
    setCurrentPage(1);
  }, [activeTab]);

  useEffect(() => {
    loadData();
  }, [activeTab, currentPage]);

  const loadData = async () => {
    try {
      setLoading(true);
      if (activeTab === 'receipts') {
        const response = await equipmentReceiptService.getReceipts({
          page: currentPage,
          pageSize: itemsPerPage
        });
        setReceipts(response.data);
        setTotalReceipts(response.totalRecords);
      } else if (activeTab === 'categories') {
        const cats = await equipmentService.getCategories(false);
        console.log('Loaded categories:', cats);
        setCategories(cats);
      } else {
        // Load items and categories
        const cats = await equipmentService.getCategories(true);
        const response = await equipmentService.getItemsPaginated({ isActive: true, page: 1, pageSize: 1000 });
        console.log('API response:', response);
        console.log('Items data:', response.data);
        setItems(response.data || []);
        setCategories(cats);
      }
    } catch (e) {
      console.error('Failed to load equipment data:', e);
      setItems([]);
      setCategories([]);
    } finally {
      setLoading(false);
    }
  };

  // Item handlers
  const handleCreateItem = async (data: CreateEquipmentItemDto | UpdateEquipmentItemDto) => {
    await equipmentService.createItem(data as CreateEquipmentItemDto);
    await loadData();
  };

  const handleUpdateItem = async (data: CreateEquipmentItemDto | UpdateEquipmentItemDto) => {
    if (!editingItem) return;
    await equipmentService.updateItem(editingItem.id, data as UpdateEquipmentItemDto);
    setEditingItem(null);
    await loadData();
  };

  const handleDeleteItem = async (item: EquipmentItem) => {
    if (!confirm(`Are you sure you want to delete "${item.name}"?`)) return;
    try {
      await equipmentService.deleteItem(item.id);
      await loadData();
    } catch (error: any) {
      alert(error.message || 'Failed to delete item');
    }
  };

  // Category handlers
  const handleCreateCategory = async (data: CreateEquipmentCategoryDto | UpdateEquipmentCategoryDto) => {
    await equipmentService.createCategory(data as CreateEquipmentCategoryDto);
    await loadData();
  };

  const handleUpdateCategory = async (data: CreateEquipmentCategoryDto | UpdateEquipmentCategoryDto) => {
    if (!editingCategory) return;
    await equipmentService.updateCategory(editingCategory.id, data as UpdateEquipmentCategoryDto);
    setEditingCategory(null);
    await loadData();
  };

  const handleDeleteCategory = async (category: EquipmentCategory) => {
    if (!confirm(`Are you sure you want to delete category "${category.name}"?`)) return;
    try {
      await equipmentService.deleteCategory(category.id);
      await loadData();
    } catch (error: any) {
      alert(error.message || 'Failed to delete category');
    }
  };

  const filteredItems = useMemo(() => {
    if (!Array.isArray(items)) return [];
    let data = [...items];
    if (categoryId !== 'all') data = data.filter(x => x.categoryId === categoryId);
    if (filterStatus !== 'all') data = data.filter(x => x.status === filterStatus);
    if (search) {
      const q = search.toLowerCase();
      data = data.filter(x =>
        x.equipmentCode.toLowerCase().includes(q) ||
        x.name.toLowerCase().includes(q) ||
        (x.serialNumber && x.serialNumber.toLowerCase().includes(q)) ||
        (x.manufacturer && x.manufacturer.toLowerCase().includes(q)) ||
        (x.location && x.location.toLowerCase().includes(q))
      );
    }
    return data;
  }, [items, search, categoryId, filterStatus]);

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
          return sortType.dir === 'asc'
            ? a.categoryName.localeCompare(b.categoryName)
            : b.categoryName.localeCompare(a.categoryName);
        });
        break;
      case 'status':
        sorted.sort((a, b) => {
          return sortType.dir === 'asc'
            ? a.status.localeCompare(b.status)
            : b.status.localeCompare(a.status);
        });
        break;
      default:
        break;
    }
    return sorted;
  }, [filteredItems, sortType]);

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
  }, [search, categoryId, filterStatus]);

  // Get unique statuses
  const uniqueStatuses = useMemo(() => {
    if (!Array.isArray(items)) return [];
    return [...new Set(items.map(item => item.status))].sort();
  }, [items]);

  return (
    <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
      <div className="p-6 space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Equipment Management</h1>
            <p className="text-sm text-gray-600 mt-1">Manage ship equipment, maintenance schedules, and SOLAS compliance</p>
          </div>
          <div className="flex gap-2">
            <button 
              onClick={() => {
                setEditingItem(null);
                setItemModalOpen(true);
              }}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              <Plus className="w-5 h-5" /> Add Equipment
            </button>
            <button 
              onClick={() => setImportModalOpen(true)}
              className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
            >
              <FileSpreadsheet className="w-5 h-5" /> Import Excel
            </button>
            <button 
              onClick={() => {
                setEditingCategory(null);
                setCategoryModalOpen(true);
              }}
              className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              <Layers className="w-5 h-5" /> Add Category
            </button>
          </div>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <StatCard 
            icon={<Wrench className="w-6 h-6 text-blue-600" />} 
            label="Tổng thiết bị" 
            value={items?.length || 0}
          />
          <StatCard 
            icon={<Layers className="w-6 h-6 text-green-600" />} 
            label="Danh mục" 
            value={categories?.length || 0}
          />
          <StatCard 
            icon={<Tag className="w-6 h-6 text-purple-600" />} 
            label="Đang hoạt động" 
            value={items?.filter(i => i.isActive).length || 0}
          />
        </div>

        {/* Tabs */}
        <div className="bg-white rounded-lg shadow">
          <div className="border-b border-gray-200">
            <nav className="flex -mb-px">
              <TabButton active={activeTab === 'items'} onClick={() => setActiveTab('items')} icon={<Wrench className="w-5 h-5" />} label="Thiết bị" />
              <TabButton active={activeTab === 'categories'} onClick={() => setActiveTab('categories')} icon={<Layers className="w-5 h-5" />} label="Danh mục" />
              <TabButton active={activeTab === 'receipts'} onClick={() => setActiveTab('receipts')} icon={<FileSpreadsheet className="w-5 h-5" />} label="Phiếu nhập" />
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
                  placeholder="Search by code, name, serial number, manufacturer..."
                  className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <select
                value={categoryId}
                onChange={(e) => setCategoryId(e.target.value === 'all' ? 'all' : Number(e.target.value))}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 min-w-[180px]"
              >
                <option value="all">All Categories</option>
                {categories?.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
              <select
                value={filterStatus}
                onChange={(e) => setFilterStatus(e.target.value)}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 min-w-[150px]"
              >
                <option value="all">All Statuses</option>
                {uniqueStatuses?.map(status => <option key={status} value={status}>{status}</option>)}
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
                  placeholder="Search by name, code, or description..."
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
                <p className="text-gray-600 mt-4">Đang tải dữ liệu...</p>
              </div>
            ) : (
              <>
                {activeTab === 'receipts' ? (
                  <div>
                    {receipts.length > 0 ? (
                      <>
                        <div className="overflow-x-auto bg-white rounded-lg border border-gray-200">
                          <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                              <tr>
                                <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase border-r border-gray-300">STT</th>
                                <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase border-r border-gray-300">Mã phiếu nhập</th>
                                <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase border-r border-gray-300">Ngày nhập</th>
                                <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase border-r border-gray-300">Số lượng items</th>
                                <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase border-r border-gray-300">Người tạo</th>
                                <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Trạng thái</th>
                              </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                              {receipts.map((receipt, idx) => (
                                <tr 
                                  key={receipt.id} 
                                  className="hover:bg-gray-50 cursor-pointer"
                                  onClick={() => {
                                    setSelectedReceiptId(receipt.id);
                                    setReceiptDetailModalOpen(true);
                                  }}
                                >
                                  <td className="px-4 py-3 text-center border-r border-gray-300">{(currentPage - 1) * itemsPerPage + idx + 1}</td>
                                  <td className="px-4 py-3 text-center border-r border-gray-300">
                                    <span className="font-medium text-blue-600">{receipt.receiptCode}</span>
                                  </td>
                                  <td className="px-4 py-3 text-center border-r border-gray-300">
                                    {new Date(receipt.receiptDate).toLocaleDateString('vi-VN')}
                                  </td>
                                  <td className="px-4 py-3 text-center border-r border-gray-300">
                                    <span className="px-2 py-1 bg-gray-100 text-gray-700 rounded-full text-sm">
                                      {receipt.itemCount} items
                                    </span>
                                  </td>
                                  <td className="px-4 py-3 text-center border-r border-gray-300">
                                    {receipt.createdBy || '-'}
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
                            Hiển thị {(currentPage - 1) * itemsPerPage + 1} - {Math.min(currentPage * itemsPerPage, totalReceipts)} trong tổng số {totalReceipts} phiếu nhập
                          </p>
                          <div className="flex gap-2">
                            <button
                              onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
                              disabled={currentPage === 1}
                              className="px-3 py-1 border rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                              ← Trước
                            </button>
                            <span className="px-3 py-1 border rounded bg-blue-50 text-blue-600 font-medium">
                              {currentPage}
                            </span>
                            <button
                              onClick={() => setCurrentPage(prev => prev + 1)}
                              disabled={currentPage * itemsPerPage >= totalReceipts}
                              className="px-3 py-1 border rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                              Sau →
                            </button>
                          </div>
                        </div>
                      </>
                    ) : (
                      <div className="text-center py-12 bg-white rounded-lg border border-gray-200">
                        <FileSpreadsheet className="w-12 h-12 text-gray-400 mx-auto mb-4" />
                        <p className="text-gray-500 mb-4">Chưa có phiếu nhập nào. Import phiếu nhập đầu tiên để bắt đầu.</p>
                        <button 
                          onClick={() => setImportModalOpen(true)}
                          className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
                        >
                          Import Excel
                        </button>
                      </div>
                    )}
                  </div>
                ) : activeTab === 'categories' ? (
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
                ) : (
                  <div>
                    {/* Info and Pagination */}
                    <div className="flex items-center justify-between mb-4">
                      {/* Left - Display info */}
                      <div className="text-sm text-gray-600">
                        Hiển thị {sortedItems.length === 0 ? 0 : (currentPage - 1) * itemsPerPage + 1} - {Math.min(currentPage * itemsPerPage, sortedItems.length)} trong tổng số {sortedItems.length} thiết bị
                      </div>

                      {/* Right - Pagination */}
                      {totalPages > 1 && (
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                            disabled={currentPage === 1}
                            className="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-30 disabled:cursor-not-allowed"
                          >
                            ← Trước
                          </button>
                          
                          <span className="text-sm text-gray-600 px-2">
                            Trang {currentPage} / {totalPages}
                          </span>

                          <button
                            onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                            disabled={currentPage === totalPages}
                            className="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-30 disabled:cursor-not-allowed"
                          >
                            Sau →
                          </button>
                        </div>
                      )}
                    </div>

                    <ItemList 
                      items={paginatedItems} 
                      onEdit={(item) => {
                        setEditingItem(item);
                        setItemModalOpen(true);
                      }}
                      onDelete={handleDeleteItem}
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
        title={editingItem ? 'Edit Equipment' : 'Add New Equipment'}
      />

      <CategoryFormModal
        isOpen={categoryModalOpen}
        onClose={() => {
          setCategoryModalOpen(false);
          setEditingCategory(null);
        }}
        onSubmit={editingCategory ? handleUpdateCategory : handleCreateCategory}
        category={editingCategory}
        title={editingCategory ? 'Edit Category' : 'Add New Category'}
      />

      <ImportEquipmentReceiptModal
        isOpen={importModalOpen}
        onClose={() => setImportModalOpen(false)}
        onSuccess={() => {
          setImportModalOpen(false);
          loadData();
        }}
      />

      <EquipmentReceiptDetailModal
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

function ItemList({ items, onEdit, onDelete, currentPage, itemsPerPage, sortType, setSortType, sortMenu, setSortMenu }: { 
  items: EquipmentItem[];
  onEdit: (item: EquipmentItem) => void;
  onDelete: (item: EquipmentItem) => void;
  currentPage?: number;
  itemsPerPage?: number;
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
      <table className="w-full border-collapse">
        <thead className="bg-gray-50">
          <tr>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300">STT</th>
            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative">
              Tên thiết bị
              {setSortType && setSortMenu && (
                <SortDropdown col="name" options={[{label:'A-Z',dir:'asc'},{label:'Z-A',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300">Danh mục</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300">Vị trí</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300">Số lượng</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300">Trạng thái</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300">Serial</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Thao tác</th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {items?.map((it, index) => {
            const globalIndex = currentPage && itemsPerPage ? (currentPage - 1) * itemsPerPage + index + 1 : index + 1;
            
            return (
              <tr 
                key={it.id} 
                className="hover:bg-gray-50 cursor-pointer"
                onClick={(e) => {
                  if ((e.target as HTMLElement).closest('button')) return;
                  onEdit(it);
                }}
              >
                <td className="px-4 py-3 text-sm text-gray-900 text-center border-r border-gray-200">{globalIndex}</td>
                <td className="px-4 py-3 border-r border-gray-200">
                  <div className="text-sm font-medium text-gray-900">{it.name}</div>
                  <div className="text-xs text-gray-500">{it.equipmentCode}</div>
                  {it.manufacturer && <div className="text-xs text-gray-400">{it.manufacturer} {it.model}</div>}
                </td>
                <td className="px-4 py-3 text-center border-r border-gray-200">
                  <span className="inline-flex px-2 py-1 text-xs rounded-full bg-blue-100 text-blue-700">
                    {it.categoryName}
                  </span>
                </td>
                <td className="px-4 py-3 text-sm text-gray-600 text-center border-r border-gray-200">{it.location || '-'}</td>
                <td className="px-4 py-3 text-sm text-gray-900 text-center border-r border-gray-200">{it.quantity}</td>
                <td className="px-4 py-3 text-center border-r border-gray-200">
                  <span className={`inline-flex px-2 py-1 text-xs rounded-full ${
                    it.status.toUpperCase() === 'OPERATIONAL' ? 'bg-green-100 text-green-700' :
                    it.status.toUpperCase() === 'MAINTENANCE' ? 'bg-yellow-100 text-yellow-700' :
                    it.status.toUpperCase() === 'OUT_OF_SERVICE' ? 'bg-red-100 text-red-700' :
                    'bg-gray-100 text-gray-700'
                  }`}>
                    {it.status}
                  </span>
                </td>
                <td className="px-4 py-3 text-sm font-mono text-gray-700 text-center border-r border-gray-200">{it.serialNumber || '-'}</td>
                <td className="px-4 py-3 text-center">
                  <div className="flex items-center justify-center gap-2">
                    <button
                      onClick={(e) => { e.stopPropagation(); onDelete(it); }}
                      className="p-1.5 text-red-600 hover:bg-red-50 rounded"
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

      {(!items || items.length === 0) && (
        <div className="text-center py-12">
          <Wrench className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-500">Không có thiết bị nào</p>
        </div>
      )}
    </div>
  );
}

function CategoryList({ categories, onEdit, onDelete, sortType, setSortType, sortMenu, setSortMenu }: { 
  categories: EquipmentCategory[];
  onEdit: (category: EquipmentCategory) => void;
  onDelete: (category: EquipmentCategory) => void;
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
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300" style={{width: '30%'}}>Mô tả</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300" style={{width: '10%'}}>Số lượng</th>
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
          {categories?.map((cat, index) => {
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
                <td className="px-4 py-3 border-r border-gray-300" style={{width: '30%'}}>
                  <div className="text-xs text-gray-600 line-clamp-2">
                    {cat.description || '-'}
                  </div>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300" style={{width: '10%'}}>
                  <span className="text-sm font-medium text-gray-900">{cat.equipmentCount || 0}</span>
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

      {(!categories || categories.length === 0) && (
        <div className="text-center py-12">
          <Layers className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-500">No categories found</p>
        </div>
      )}
    </div>
  );
}

function StatCard({ icon, label, value, subtitle }: { icon: React.ReactNode; label: string; value: string | number; subtitle?: string }) {
  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-gray-600">{label}</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{value}</p>
          {subtitle && <p className="text-xs text-gray-500 mt-0.5">{subtitle}</p>}
        </div>
        {icon}
      </div>
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
