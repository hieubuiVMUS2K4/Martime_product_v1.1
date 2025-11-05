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

  // Pagination for items
  const paginatedItems = useMemo(() => {
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    return filteredItems.slice(startIndex, endIndex);
  }, [filteredItems, currentPage, itemsPerPage]);

  const totalPages = Math.ceil(filteredItems.length / itemsPerPage);

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
                    categories={categories}
                    onEdit={(cat) => {
                      setEditingCategory(cat);
                      setCategoryModalOpen(true);
                    }}
                    onDelete={handleDeleteCategory}
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
                        Hiển thị {filteredItems.length === 0 ? 0 : (currentPage - 1) * itemsPerPage + 1} - {Math.min(currentPage * itemsPerPage, filteredItems.length)} trong tổng số {filteredItems.length} vật tư
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

function ItemList({ items, highlightLow = false, categories, onEdit, onDelete, onAdjustStock, currentPage, itemsPerPage }: { 
  items: MaterialItem[]; 
  highlightLow?: boolean; 
  categories: MaterialCategory[];
  onEdit: (item: MaterialItem) => void;
  onDelete: (item: MaterialItem) => void;
  onAdjustStock: (item: MaterialItem) => void;
  currentPage?: number;
  itemsPerPage?: number;
}) {
  const getCategoryName = (catId: number) => {
    const cat = categories.find(c => c.id === catId);
    return cat?.name || 'Unknown';
  };

  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          <tr>
            <Th className="text-center">STT</Th>
            <Th>Name</Th>
            <Th className="text-center">Category</Th>
            <Th className="text-center">Unit</Th>
            <Th className="text-center">On Hand</Th>
            <Th className="text-center">Min / Max</Th>
            <Th className="text-center">Location</Th>
            <Th className="text-center">Part No.</Th>
            <Th className="text-right">Unit Cost</Th>
            <Th className="text-center">Status</Th>
            <Th className="text-center">Actions</Th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {items.map((it, index) => {
            const low = it.minStock != null && it.onHandQuantity < (it.minStock ?? 0);
            const over = it.maxStock != null && it.onHandQuantity > (it.maxStock ?? 0);
            const totalValue = it.unitCost ? (it.unitCost * it.onHandQuantity) : null;
            const globalIndex = currentPage && itemsPerPage ? (currentPage - 1) * itemsPerPage + index + 1 : index + 1;
            
            return (
              <tr 
                key={it.id} 
                className={`cursor-pointer hover:bg-blue-50 transition-colors ${highlightLow && low ? 'bg-red-50' : ''}`}
                onClick={(e) => {
                  // Don't trigger if clicking action buttons
                  if ((e.target as HTMLElement).closest('button')) return;
                  onEdit(it);
                }}
              >
                <Td className="text-center"><span className="text-sm text-gray-900">{globalIndex}</span></Td>
                <Td>
                  <div className="flex flex-col">
                    <span className="font-medium">{it.name}</span>
                    {it.specification && <span className="text-xs text-gray-500">{it.specification}</span>}
                  </div>
                </Td>
                <Td className="text-center">
                  <span className="text-xs text-gray-600">{getCategoryName(it.categoryId)}</span>
                </Td>
                <Td className="text-center"><span className="text-xs">{it.unit}</span></Td>
                <Td className="text-center">
                  <span className={`font-medium ${low ? 'text-red-600' : over ? 'text-orange-600' : ''}`}>
                    {it.onHandQuantity.toFixed(2)}
                  </span>
                </Td>
                <Td className="text-center">
                  <span className="text-xs">
                    {it.minStock != null ? it.minStock.toFixed(2) : '-'} / {it.maxStock != null ? it.maxStock.toFixed(2) : '-'}
                  </span>
                </Td>
                <Td className="text-center"><span className="text-xs">{it.location || '-'}</span></Td>
                <Td className="text-center">
                  <span className="text-xs font-mono">{it.partNumber || '-'}</span>
                  {it.barcode && <div className="text-xs text-gray-400">🔖 {it.barcode}</div>}
                </Td>
                <Td className="text-right">
                  <div className="flex flex-col items-end">
                    {it.unitCost ? (
                      <>
                        <span className="text-xs font-medium">{it.unitCost.toFixed(2)} {it.currency || 'USD'}</span>
                        {totalValue && <span className="text-xs text-gray-500">= {totalValue.toFixed(2)}</span>}
                      </>
                    ) : (
                      <span className="text-xs text-gray-400">-</span>
                    )}
                  </div>
                </Td>
                <Td className="text-center">
                  <div className="flex flex-wrap gap-1 justify-center">
                    {low && <span className="px-2 py-0.5 text-xs rounded-full bg-red-100 text-red-700">LOW</span>}
                    {over && <span className="px-2 py-0.5 text-xs rounded-full bg-orange-100 text-orange-700">OVER</span>}
                    {it.serialTracked && <span className="px-2 py-0.5 text-xs rounded-full bg-purple-100 text-purple-700">SN</span>}
                    {it.batchTracked && <span className="px-2 py-0.5 text-xs rounded-full bg-yellow-100 text-yellow-700">BATCH</span>}
                    {it.expiryRequired && <span className="px-2 py-0.5 text-xs rounded-full bg-blue-100 text-blue-700">EXP</span>}
                    {!it.isActive && <span className="px-2 py-0.5 text-xs rounded-full bg-gray-100 text-gray-600">INACTIVE</span>}
                  </div>
                </Td>
                <Td className="text-center">
                  <div className="flex items-center justify-center gap-2">
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        onAdjustStock(it);
                      }}
                      className="p-1.5 text-green-600 hover:bg-green-50 rounded transition-colors"
                      title="Adjust Stock"
                    >
                      <TrendingUp className="w-4 h-4" />
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        onDelete(it);
                      }}
                      className="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors"
                      title="Delete"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </Td>
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

function CategoryList({ categories, onEdit, onDelete }: { 
  categories: MaterialCategory[];
  onEdit: (category: MaterialCategory) => void;
  onDelete: (category: MaterialCategory) => void;
}) {
  const roots = categories.filter(c => !c.parentCategoryId);
  const children = (pid: number) => categories.filter(c => c.parentCategoryId === pid);

  return (
    <div className="space-y-3">
      {roots.map(root => (
        <div key={root.id} className="border border-gray-200 rounded-lg">
          <div className="p-4 flex items-center justify-between bg-gray-50">
            <div className="flex items-center gap-2">
              <Tag className="w-4 h-4 text-blue-600" />
              <div>
                <p className="font-semibold text-gray-900">{root.name}</p>
                <p className="text-xs text-gray-500">{root.categoryCode}</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <span className={`px-2 py-1 text-xs rounded-full ${root.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'}`}>
                {root.isActive ? 'Active' : 'Inactive'}
              </span>
              <div className="flex items-center gap-1">
                <button
                  onClick={() => onEdit(root)}
                  className="p-1.5 text-blue-600 hover:bg-blue-50 rounded transition-colors"
                  title="Edit"
                >
                  <Edit2 className="w-4 h-4" />
                </button>
                <button
                  onClick={() => onDelete(root)}
                  className="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors"
                  title="Delete"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>
          {children(root.id).length > 0 && (
            <div className="p-4">
              <div className="grid md:grid-cols-2 gap-3">
                {children(root.id).map(sc => (
                  <div key={sc.id} className="border border-gray-200 rounded p-3">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-medium">{sc.name}</p>
                        <p className="text-xs text-gray-500">{sc.categoryCode}</p>
                      </div>
                      <div className="flex items-center gap-2">
                        <span className={`px-2 py-1 text-xs rounded-full ${sc.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'}`}>
                          {sc.isActive ? 'Active' : 'Inactive'}
                        </span>
                        <div className="flex items-center gap-1">
                          <button
                            onClick={() => onEdit(sc)}
                            className="p-1 text-blue-600 hover:bg-blue-50 rounded transition-colors"
                            title="Edit"
                          >
                            <Edit2 className="w-3.5 h-3.5" />
                          </button>
                          <button
                            onClick={() => onDelete(sc)}
                            className="p-1 text-red-600 hover:bg-red-50 rounded transition-colors"
                            title="Delete"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        </div>
                      </div>
                    </div>
                    {sc.description && <p className="text-sm text-gray-600 mt-2">{sc.description}</p>}
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      ))}

      {roots.length === 0 && (
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

function Th({ children, className = '' }: { children: React.ReactNode; className?: string }) {
  return <th className={`px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider ${className}`}>{children}</th>;
}
function Td({ children, className = '' }: { children: React.ReactNode; className?: string }) {
  return <td className={`px-6 py-4 whitespace-nowrap text-sm text-gray-900 ${className}`}>{children}</td>;
}