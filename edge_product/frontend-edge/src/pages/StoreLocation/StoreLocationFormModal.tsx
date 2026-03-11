import { useState, useEffect } from 'react';
import { X } from 'lucide-react';
import { storeLocationService } from '@/services/store-location.service';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { StoreLocation, CreateStoreLocationDto } from '@/types/pms.types';

interface Props {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  locations: StoreLocation[];
  editItem?: StoreLocation | null;
}

export function StoreLocationFormModal({ isOpen, onClose, onSuccess, locations, editItem }: Props) {
  const { t } = useTranslationSafe();
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const [form, setForm] = useState<CreateStoreLocationDto>({
    locationCode: '',
    name: '',
    description: '',
    parentId: null,
    address: '',
    managerName: '',
    phone: '',
    email: '',
  });

  useEffect(() => {
    if (isOpen) {
      setError('');
      if (editItem) {
        setForm({
          locationCode: editItem.locationCode,
          name: editItem.name,
          description: editItem.description ?? '',
          parentId: editItem.parentId ?? null,
          address: editItem.address ?? '',
          managerName: editItem.managerName ?? '',
          phone: editItem.phone ?? '',
          email: editItem.email ?? '',
        });
      } else {
        setForm({
          locationCode: '',
          name: '',
          description: '',
          parentId: null,
          address: '',
          managerName: '',
          phone: '',
          email: '',
        });
      }
    }
  }, [isOpen, editItem]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.locationCode.trim() || !form.name.trim()) {
      setError(t('storeLocations.form.required'));
      return;
    }
    try {
      setSaving(true);
      setError('');
      if (editItem) {
        await storeLocationService.update(editItem.id, form);
      } else {
        await storeLocationService.create(form);
      }
      onSuccess();
      onClose();
    } catch (err: any) {
      setError(err?.response?.data?.error || 'Save failed');
    } finally {
      setSaving(false);
    }
  };

  if (!isOpen) return null;

  // Exclude current item and descendants from parent options (prevent cycles)
  const getExcludeIds = (id: string): Set<string> => {
    const ids = new Set<string>();
    const stack = [id];
    while (stack.length) {
      const current = stack.pop()!;
      ids.add(current);
      locations.filter(l => l.parentId === current).forEach(c => stack.push(c.id));
    }
    return ids;
  };
  const excludeIds = editItem ? getExcludeIds(editItem.id) : new Set<string>();
  const parentOptions = locations.filter(l => !excludeIds.has(l.id));

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-lg max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between z-10">
          <h2 className="text-lg font-semibold text-gray-900">
            {editItem ? t('storeLocations.form.editTitle') : t('storeLocations.form.addTitle')}
          </h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Body */}
        <form onSubmit={handleSubmit} className="px-6 py-4 space-y-4">
          {error && (
            <div className="bg-red-50 text-red-700 text-sm px-3 py-2 rounded border border-red-200">{error}</div>
          )}

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                {t('storeLocations.form.code')} <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={form.locationCode}
                onChange={e => setForm(f => ({ ...f, locationCode: e.target.value }))}
                className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
                placeholder="KHO-01"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                {t('storeLocations.form.name')} <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={form.name}
                onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('storeLocations.form.parent')}
            </label>
            <select
              value={form.parentId ?? ''}
              onChange={e => setForm(f => ({ ...f, parentId: e.target.value || null }))}
              className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">— {t('storeLocations.form.noParent')} —</option>
              {parentOptions.map(loc => (
                <option key={loc.id} value={loc.id}>{loc.name} ({loc.locationCode})</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('storeLocations.form.description')}
            </label>
            <textarea
              value={form.description ?? ''}
              onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
              rows={2}
              className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('storeLocations.form.address')}
            </label>
            <input
              type="text"
              value={form.address ?? ''}
              onChange={e => setForm(f => ({ ...f, address: e.target.value }))}
              className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                {t('storeLocations.form.manager')}
              </label>
              <input
                type="text"
                value={form.managerName ?? ''}
                onChange={e => setForm(f => ({ ...f, managerName: e.target.value }))}
                className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                {t('storeLocations.form.phone')}
              </label>
              <input
                type="text"
                value={form.phone ?? ''}
                onChange={e => setForm(f => ({ ...f, phone: e.target.value }))}
                className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('storeLocations.form.email')}
            </label>
            <input
              type="email"
              value={form.email ?? ''}
              onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
              className="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          {/* Footer */}
          <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm border border-gray-300 rounded text-gray-700 hover:bg-gray-50"
            >
              {t('common.cancel')}
            </button>
            <button
              type="submit"
              disabled={saving}
              className="px-4 py-2 text-sm bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
            >
              {saving ? t('storeLocations.form.saving') : (editItem ? t('common.save') : t('storeLocations.form.create'))}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
