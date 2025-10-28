import { useEffect, useMemo, useState } from 'react';
import { Boxes, Layers, Plus, Search, AlertTriangle, Tag } from 'lucide-react';
import { maritimeService } from '../../services/maritime.service';
import type { MaterialItem, MaterialCategory } from '../../types/maritime.types';

type TabType = 'items' | 'low' | 'categories';

export function MaterialPage() {
  const [activeTab, setActiveTab] = useState<TabType>('items');
  const [items, setItems] = useState<MaterialItem[]>([]);
  const [lowStock, setLowStock] = useState<MaterialItem[]>([]);
  const [categories, setCategories] = useState<MaterialCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState<number | 'all'>('all');

  useEffect(() => {
    loadData();
  }, [activeTab]);

  const loadData = async () => {
    try {
      setLoading(true);
      if (activeTab === 'low') {
        const [ls, cats] = await Promise.all([
          maritimeService.material.getLowStock(),
          maritimeService.material.getCategories(true)
        ]);
        setLowStock(ls);
        setCategories(cats);
      } else if (activeTab === 'categories') {
        const cats = await maritimeService.material.getCategories(false);
        setCategories(cats);
      } else {
        const [its, cats] = await Promise.all([
          maritimeService.material.getItems({ onlyActive: true }),
          maritimeService.material.getCategories(true)
        ]);
        setItems(its);
        setCategories(cats);
      }
    } catch (e) {
      console.error('Failed to load material data:', e);
    } finally {
      setLoading(false);
    }
  };

  const filteredItems = useMemo(() => {
    let data = [...items];
    if (categoryId !== 'all') data = data.filter(x => x.categoryId === categoryId);
    if (search) {
      const q = search.toLowerCase();
      data = data.filter(x =>
        x.itemCode.toLowerCase().includes(q) ||
        x.name.toLowerCase().includes(q) ||
        (x.partNumber && x.partNumber.toLowerCase().includes(q)) ||
        (x.barcode && x.barcode.toLowerCase().includes(q))
      );
    }
    return data;
  }, [items, search, categoryId]);

  return (
    <div className="h-full w-full overflow-y-auto">
      <div className="p-6 space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Material Management</h1>
            <p className="text-sm text-gray-600 mt-1">Inventory items, categories, and low stock alerts</p>
          </div>
          <div className="flex gap-2">
            <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
              <Plus className="w-5 h-5" /> Add Item
            </button>
            <button className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50">
              <Layers className="w-5 h-5" /> Add Category
            </button>
          </div>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <StatCard icon={<Boxes className="w-6 h-6 text-blue-600" />} label="Total Items" value={items.length} />
          <StatCard icon={<AlertTriangle className="w-6 h-6 text-red-600" />} label="Low Stock" value={lowStock.length} />
          <StatCard icon={<Layers className="w-6 h-6 text-green-600" />} label="Active Categories" value={categories.filter(c => c.isActive).length} />
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
                className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              >
                <option value="all">All Categories</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
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
                  <CategoryList categories={categories} />
                ) : activeTab === 'low' ? (
                  <ItemList items={lowStock} highlightLow />
                ) : (
                  <ItemList items={filteredItems} />
                )}
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

function ItemList({ items, highlightLow = false }: { items: MaterialItem[]; highlightLow?: boolean }) {
  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          <tr>
            <Th>Code</Th>
            <Th>Name</Th>
            <Th>Unit</Th>
            <Th className="text-right">On Hand</Th>
            <Th className="text-right">Min</Th>
            <Th>Location</Th>
            <Th>Manufacturer</Th>
            <Th>Part No.</Th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {items.map(it => {
            const low = it.minStock != null && it.onHandQuantity < (it.minStock ?? 0);
            return (
              <tr key={it.id} className={highlightLow && low ? 'bg-red-50' : ''}>
                <Td><span className="font-mono">{it.itemCode}</span></Td>
                <Td>
                  <div className="flex items-center gap-2">
                    <span>{it.name}</span>
                    {low && <span className="px-2 py-0.5 text-xs rounded-full bg-red-100 text-red-700">LOW</span>}
                    {it.serialTracked && <span className="px-2 py-0.5 text-xs rounded-full bg-purple-100 text-purple-700">SN</span>}
                    {it.batchTracked && <span className="px-2 py-0.5 text-xs rounded-full bg-yellow-100 text-yellow-700">BATCH</span>}
                  </div>
                </Td>
                <Td>{it.unit}</Td>
                <Td className="text-right">{it.onHandQuantity?.toFixed(3)}</Td>
                <Td className="text-right">{it.minStock != null ? Number(it.minStock).toFixed(3) : '-'}</Td>
                <Td>{it.location || '-'}</Td>
                <Td>{it.manufacturer || '-'}</Td>
                <Td>{it.partNumber || '-'}</Td>
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

function CategoryList({ categories }: { categories: MaterialCategory[] }) {
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
            <span className={`px-2 py-1 text-xs rounded-full ${root.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'}`}>
              {root.isActive ? 'Active' : 'Inactive'}
            </span>
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
                      <span className={`px-2 py-1 text-xs rounded-full ${sc.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'}`}>
                        {sc.isActive ? 'Active' : 'Inactive'}
                      </span>
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

function StatCard({ icon, label, value }: { icon: React.ReactNode; label: string; value: number }) {
  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-gray-600">{label}</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{value}</p>
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