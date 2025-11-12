import { useEffect, useMemo, useState } from 'react';
import { Boxes, Layers, Plus, Search, AlertTriangle, Tag, Edit2, Trash2, TrendingUp } from 'lucide-react';
import { materialService } from '../../services/materialService';
import type { MaterialItem, MaterialCategory } from '../../types/maritime.types';
import type { CreateMaterialItemDto, UpdateMaterialItemDto, CreateMaterialCategoryDto, UpdateMaterialCategoryDto, StockAdjustmentDto } from '../../services/materialService';
import { ItemFormModal } from './ItemFormModal';
import { CategoryFormModal } from './CategoryFormModal';
import { StockAdjustmentModal } from './StockAdjustmentModal';

type TabType = 'items' | 'low' | 'categories';

export function MaterialPage() {
  const [activeTab, setActiveTab] = useState<TabType>('items');
  const [items, setItems] = useState<MaterialItem[]>([]);
  const [lowStock, setLowStock] = useState<MaterialItem[]>([]);
  const [categories, setCategories] = useState<MaterialCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [categorySearch, setCategorySearch] = useState('');
  const [categoryId, setCategoryId] = useState<number | 'all'>('all');
  const [filterUnit, setFilterUnit] = useState<string>('all');

  // Modal states
  const [itemModalOpen, setItemModalOpen] = useState(false);
  const [categoryModalOpen, setCategoryModalOpen] = useState(false);
  const [stockAdjustmentModalOpen, setStockAdjustmentModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<MaterialItem | null>(null);
  const [editingCategory, setEditingCategory] = useState<MaterialCategory | null>(null);
  const [adjustingItem, setAdjustingItem] = useState<MaterialItem | null>(null);

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 10;

  // State filter/sort cho bảng
  const [sortType, setSortType] = useState<{ col: string; dir: 'asc'|'desc' } | null>(null);
  const [sortMenu, setSortMenu] = useState<string | null>(null); // col name or null
  
  // Category sorting state
  const [categorySortType, setCategorySortType] = useState<{ col: string; dir: 'asc'|'desc' } | null>(null);
  const [categorySortMenu, setCategorySortMenu] = useState<string | null>(null);

  useEffect(() => {
    loadData();
  }, [activeTab]);

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

  const totalValue = useMemo(() => {
    return items.reduce((sum, item) => {
      if (item.unitCost) {
        return sum + (item.unitCost * item.onHandQuantity);
      }
      return sum;
    }, 0);
  }, [items]);

  return (
    <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
      <div className="p-6 space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Material Management</h1>
            <p className="text-sm text-gray-600 mt-1">Inventory items, categories, and low stock alerts</p>
          </div>
          <div className="flex gap-2">
            <button 
              onClick={() => {
                setEditingItem(null);
                setItemModalOpen(true);
              }}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              <Plus className="w-5 h-5" /> Add Item
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
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <StatCard 
            icon={<Boxes className="w-6 h-6 text-blue-600" />} 
            label="Total Items" 
            value={items.length}
          />
          <StatCard 
            icon={<AlertTriangle className="w-6 h-6 text-red-600" />} 
            label="Low Stock Items" 
            value={lowStock.length}
          />
          <StatCard 
            icon={<Layers className="w-6 h-6 text-green-600" />} 
            label="Active Categories" 
            value={categories.filter(c => c.isActive).length}
          />
          <StatCard 
            icon={<Boxes className="w-6 h-6 text-purple-600" />} 
            label="Total Inventory Value" 
            value={`$${totalValue.toFixed(2)}`}
            subtitle="USD"
          />
        </div>

        {/* Tabs */}
        <div className="bg-white rounded-lg shadow">
          <div className="border-b border-gray-200">
            <nav className="flex -mb-px">
              <TabButton active={activeTab === 'items'} onClick={() => setActiveTab('items')} icon={<Boxes className="w-5 h-5" />} label="Items" />
              <TabButton active={activeTab === 'low'} onClick={() => setActiveTab('low')} icon={<AlertTriangle className="w-5 h-5" />} label="Low Stock" />
              <TabButton active={activeTab === 'categories'} onClick={() => setActiveTab('categories')} icon={<Layers className="w-5 h-5" />} label="Categories" />
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
                  placeholder="Search by code, name, part number, barcode..."
                  className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <select
                value={categoryId}
                onChange={(e) => setCategoryId(e.target.value === 'all' ? 'all' : Number(e.target.value))}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 min-w-[180px]"
              >
                <option value="all">All Categories</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
              <select
                value={filterUnit}
                onChange={(e) => setFilterUnit(e.target.value)}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 min-w-[150px]"
              >
                <option value="all">All Units</option>
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
                <p className="text-gray-600 mt-4">Loading materials...</p>
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
                        Hiển thị {sortedItems.length === 0 ? 0 : (currentPage - 1) * itemsPerPage + 1} - {Math.min(currentPage * itemsPerPage, sortedItems.length)} trong tổng số {sortedItems.length} vật tư
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
        title={editingItem ? 'Edit Material Item' : 'Add New Material Item'}
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
        title={editingCategory ? 'Edit Category' : 'Add New Category'}
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
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative" style={{position:'relative', width: '26%'}}>
              Tên vật tư
              {setSortType && setSortMenu && (
                <SortDropdown col="name" options={[{label:'Sắp xếp từ A-Z',dir:'asc'},{label:'Sắp xếp từ Z-A',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative" style={{position:'relative', width: '12%'}}>
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
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300" style={{width: '12%'}}>Mã linh kiện</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300 relative" style={{position:'relative', width: '10%'}}>
              Đơn giá
              {setSortType && setSortMenu && (
                <SortDropdown col="unitCost" options={[{label:'Sắp xếp tăng dần',dir:'asc'},{label:'Sắp xếp giảm dần',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-300" style={{width: '12%'}}>Trạng thái</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '8%'}}>Thao tác</th>
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
                <td className="px-4 py-3 border-r border-gray-300" style={{width: '26%'}}>
                  <div className="text-sm font-medium text-gray-900 truncate">{it.name}</div>
                  {it.specification && (
                    <div className="text-xs text-gray-500 mt-1 truncate">{it.specification}</div>
                  )}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300" style={{width: '12%'}}>
                  <span className="text-xs px-2 py-1 rounded-full bg-blue-100 text-blue-700 truncate">
                    {getCategoryName(it.categoryId)}
                  </span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300" style={{width: '6%'}}>
                  <span className="text-xs">{it.unit}</span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300" style={{width: '8%'}}>
                  <span className={`text-sm font-medium ${low ? 'text-red-600' : over ? 'text-orange-600' : 'text-gray-900'}`}>
                    {it.onHandQuantity.toFixed(2)}
                  </span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300" style={{width: '12%'}}>
                  <span className="text-xs text-gray-600">
                    {it.minStock != null ? it.minStock.toFixed(2) : '-'} / {it.maxStock != null ? it.maxStock.toFixed(2) : '-'}
                  </span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300" style={{width: '12%'}}>
                  <span className="text-xs font-mono text-gray-700 truncate">{it.partNumber || '-'}</span>
                  {it.barcode && <div className="text-xs text-gray-400 mt-0.5 truncate">🔖 {it.barcode}</div>}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300" style={{width: '10%'}}>
                  <div className="flex flex-col items-center">
                    {it.unitCost ? (
                      <>
                        <span className="text-xs font-medium text-gray-900">{it.unitCost.toFixed(2)} {it.currency || 'USD'}</span>
                        {totalValue && <span className="text-xs text-gray-500">= {totalValue.toFixed(2)}</span>}
                      </>
                    ) : (
                      <span className="text-xs text-gray-400">-</span>
                    )}
                  </div>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center border-r border-gray-300" style={{width: '12%'}}>
                  <div className="flex flex-wrap gap-1 justify-center">
                    {low && <span className="px-1 py-0.5 text-xs rounded-full bg-red-100 text-red-700">LOW</span>}
                    {over && <span className="px-1 py-0.5 text-xs rounded-full bg-orange-100 text-orange-700">OVER</span>}
                    {it.serialTracked && <span className="px-1 py-0.5 text-xs rounded-full bg-purple-100 text-purple-700">SN</span>}
                    {it.batchTracked && <span className="px-1 py-0.5 text-xs rounded-full bg-yellow-100 text-yellow-700">BATCH</span>}
                    {it.expiryRequired && <span className="px-1 py-0.5 text-xs rounded-full bg-blue-100 text-blue-700">EXP</span>}
                    {!it.isActive && <span className="px-1 py-0.5 text-xs rounded-full bg-gray-100 text-gray-600">INACTIVE</span>}
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
                      title="Điều chỉnh tồn kho"
                    >
                      <TrendingUp className="w-4 h-4" />
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        onDelete(it);
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

      {items.length === 0 && (
        <div className="text-center py-12">
          <Boxes className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-500">No items found</p>
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