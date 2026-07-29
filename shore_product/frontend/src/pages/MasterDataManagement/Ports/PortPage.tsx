import React, { useState, useEffect, useCallback } from 'react';
import { Anchor, Plus, Pencil, Search, RefreshCw, Loader2, AlertTriangle, X, Ban } from 'lucide-react';
import { portApi } from '../../../services/crew.service';
import type { Port, PortPayload } from '../../../services/crew.service';

const EMPTY: PortPayload = {
  portCode: '', portName: '', country: '', countryCode: '',
  latitude: undefined, longitude: undefined, timeZone: '', isActive: true,
};

/**
 * Danh mục cảng — bờ làm chủ, phát xuống mọi tàu.
 *
 * Thêm/sửa ở đây sẽ tự đồng bộ tới tất cả các tàu, giống cách làm với chức danh,
 * quốc gia và loại chứng chỉ. Ngừng sử dụng là đặt cờ, KHÔNG xoá cứng — các chuyến đi
 * và mục sổ thuyền viên cũ còn tham chiếu tới cảng.
 */
export const PortPage: React.FC = () => {
  const [ports, setPorts] = useState<Port[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [showInactive, setShowInactive] = useState(false);
  const [page, setPage] = useState(1);
  const PAGE_SIZE = 20;

  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<Port | null>(null);
  const [form, setForm] = useState<PortPayload>(EMPTY);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      setError(null);
      const res = await portApi.search({
        search: search || undefined,
        isActive: showInactive ? undefined : true,
        page,
        pageSize: PAGE_SIZE,
      });
      setPorts(res.data);
      setTotal(res.pagination.totalCount);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Không tải được danh mục cảng');
    } finally {
      setLoading(false);
    }
  }, [search, showInactive, page]);

  // Hoãn 300ms để không bắn request theo từng phím gõ
  useEffect(() => {
    const t = setTimeout(load, 300);
    return () => clearTimeout(t);
  }, [load]);

  // Đổi từ khoá tìm hay bộ lọc thì về trang 1, nếu không sẽ rơi vào trang trống
  useEffect(() => { setPage(1); }, [search, showInactive]);

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  // Ngừng dùng dòng cuối cùng của trang cuối → trang hiện tại thành rỗng, tự lùi lại
  useEffect(() => {
    if (!loading && ports.length === 0 && page > 1) setPage(p => p - 1);
  }, [loading, ports.length, page]);

  const openCreate = () => { setEditing(null); setForm(EMPTY); setFormError(null); setModalOpen(true); };
  const openEdit = (p: Port) => {
    setEditing(p);
    setForm({
      portCode: p.portCode, portName: p.portName,
      country: p.country ?? '', countryCode: p.countryCode ?? '',
      latitude: p.latitude ?? undefined, longitude: p.longitude ?? undefined,
      timeZone: p.timeZone ?? '', isActive: p.isActive,
    });
    setFormError(null);
    setModalOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setFormError(null);
    try {
      if (editing) await portApi.update(editing.id, form);
      else await portApi.create(form);
      setModalOpen(false);
      await load();
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Lưu thất bại');
    } finally {
      setSaving(false);
    }
  };

  const handleDeactivate = async (p: Port) => {
    if (!window.confirm(`Ngừng sử dụng cảng "${p.portName}"?\nCảng vẫn được giữ lại trong dữ liệu cũ.`)) return;
    try { await portApi.deactivate(p.id); await load(); }
    catch (err) { setError(err instanceof Error ? err.message : 'Thao tác thất bại'); }
  };

  return (
    <div className="p-1">
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center gap-2">
          <Anchor size={17} className="text-teal-600" />
          <h2 className="font-bold text-gray-800">Danh mục cảng</h2>
          <span className="px-2 py-0.5 rounded-full bg-teal-50 text-teal-700 text-xs font-bold border border-teal-200">
            {total}
          </span>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={load} className="p-1.5 rounded border border-gray-300 hover:bg-gray-50" title="Làm mới">
            <RefreshCw size={13} />
          </button>
          <button onClick={openCreate}
            className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm rounded bg-teal-600 text-white hover:bg-teal-700">
            <Plus size={13} /> Thêm cảng
          </button>
        </div>
      </div>

      <div className="flex items-center gap-3 mb-3">
        <div className="relative flex-1 max-w-md">
          <Search size={13} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
          <input value={search} onChange={e => setSearch(e.target.value)}
            placeholder="Tìm theo tên cảng, mã UN/LOCODE hoặc quốc gia..."
            className="w-full border border-gray-300 rounded pl-7 pr-3 py-1.5 text-sm" />
        </div>
        <label className="flex items-center gap-1.5 text-sm text-gray-600">
          <input type="checkbox" checked={showInactive} onChange={e => setShowInactive(e.target.checked)} />
          Hiện cả cảng đã ngừng
        </label>
      </div>

      {error && (
        <div className="mb-3 flex items-start gap-2 rounded bg-red-50 border border-red-200 px-3 py-2 text-sm text-red-700">
          <AlertTriangle size={14} className="mt-0.5 shrink-0" /> {error}
        </div>
      )}

      {loading ? (
        <div className="flex items-center justify-center p-12 text-gray-500">
          <Loader2 size={22} className="animate-spin mr-2" /> Đang tải...
        </div>
      ) : ports.length === 0 ? (
        <div className="flex flex-col items-center justify-center p-12 text-gray-400">
          <Anchor size={28} className="mb-2" />
          <p className="text-sm">Chưa có cảng nào</p>
          <p className="text-xs mt-1">Danh mục cảng được đẩy lên từ tàu, hoặc thêm tay tại đây</p>
        </div>
      ) : (
        <div className="overflow-x-auto border border-gray-200 rounded-lg">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 text-xs uppercase text-gray-600">
              <tr>
                <th className="px-4 py-2.5 text-left">Mã</th>
                <th className="px-4 py-2.5 text-left">Tên cảng</th>
                <th className="px-4 py-2.5 text-left">Quốc gia</th>
                <th className="px-4 py-2.5 text-left">Toạ độ</th>
                <th className="px-4 py-2.5 text-left">Múi giờ</th>
                <th className="px-4 py-2.5 text-center">Trạng thái</th>
                <th className="px-4 py-2.5 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 bg-white">
              {ports.map(p => (
                <tr key={p.id} className={`hover:bg-slate-50 ${!p.isActive ? 'opacity-50' : ''}`}>
                  <td className="px-4 py-2.5 font-mono font-semibold text-teal-700">{p.portCode}</td>
                  <td className="px-4 py-2.5 text-gray-800">{p.portName}</td>
                  <td className="px-4 py-2.5 text-gray-600">
                    {p.country ?? '—'}
                    {p.countryCode && <span className="ml-1.5 text-xs text-gray-400 font-mono">{p.countryCode}</span>}
                  </td>
                  <td className="px-4 py-2.5 text-xs text-gray-500 font-mono">
                    {p.latitude != null && p.longitude != null
                      ? `${p.latitude.toFixed(4)}, ${p.longitude.toFixed(4)}` : '—'}
                  </td>
                  <td className="px-4 py-2.5 text-xs text-gray-500">{p.timeZone ?? '—'}</td>
                  <td className="px-4 py-2.5 text-center">
                    {p.isActive
                      ? <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-green-50 text-green-700 border border-green-200">Đang dùng</span>
                      : <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-gray-100 text-gray-500 border border-gray-200">Đã ngừng</span>}
                  </td>
                  <td className="px-4 py-2.5 text-right whitespace-nowrap">
                    <div className="inline-flex gap-1.5">
                      <button onClick={() => openEdit(p)} title="Sửa"
                        className="p-1.5 rounded border border-gray-200 hover:bg-blue-50 hover:text-blue-600">
                        <Pencil size={12} />
                      </button>
                      {p.isActive && (
                        <button onClick={() => handleDeactivate(p)} title="Ngừng sử dụng"
                          className="p-1.5 rounded border border-gray-200 hover:bg-amber-50 hover:text-amber-600">
                          <Ban size={12} />
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {!loading && total > 0 && (
        <div className="flex items-center justify-between mt-3 text-sm">
          <span className="text-gray-500">
            Hiển thị {(page - 1) * PAGE_SIZE + 1}–{Math.min(page * PAGE_SIZE, total)} / {total} cảng
          </span>
          <div className="flex items-center gap-1">
            <button onClick={() => setPage(1)} disabled={page === 1}
              className="px-2 py-1 rounded border border-gray-300 disabled:opacity-40 hover:bg-gray-50">
              «
            </button>
            <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1}
              className="px-2.5 py-1 rounded border border-gray-300 disabled:opacity-40 hover:bg-gray-50">
              Trước
            </button>
            <span className="px-3 text-gray-600">Trang {page} / {totalPages}</span>
            <button onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page >= totalPages}
              className="px-2.5 py-1 rounded border border-gray-300 disabled:opacity-40 hover:bg-gray-50">
              Sau
            </button>
            <button onClick={() => setPage(totalPages)} disabled={page >= totalPages}
              className="px-2 py-1 rounded border border-gray-300 disabled:opacity-40 hover:bg-gray-50">
              »
            </button>
          </div>
        </div>
      )}

      {modalOpen && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4" onClick={() => setModalOpen(false)}>
          <div className="bg-white rounded-lg shadow-xl w-full max-w-lg" onClick={e => e.stopPropagation()}>
            <div className="flex items-center justify-between px-5 py-3.5 border-b border-gray-200">
              <h2 className="font-bold text-gray-800">{editing ? 'Sửa cảng' : 'Thêm cảng mới'}</h2>
              <button onClick={() => setModalOpen(false)} className="p-1 rounded hover:bg-gray-100"><X size={16} /></button>
            </div>

            {formError && (
              <div className="mx-5 mt-3 flex items-start gap-2 rounded bg-red-50 border border-red-200 px-3 py-2 text-sm text-red-700">
                <AlertTriangle size={14} className="mt-0.5 shrink-0" /> {formError}
              </div>
            )}

            <form onSubmit={handleSave} className="px-5 py-4 space-y-3">
              <div className="grid grid-cols-3 gap-3">
                <label className="block">
                  <span className="text-xs font-semibold text-gray-600">Mã UN/LOCODE <span className="text-red-500">*</span></span>
                  <input required maxLength={5} value={form.portCode}
                    onChange={e => setForm({ ...form, portCode: e.target.value.toUpperCase() })}
                    placeholder="VNSGN"
                    className="mt-1 w-full border border-gray-300 rounded px-2.5 py-1.5 text-sm font-mono" />
                </label>
                <label className="block col-span-2">
                  <span className="text-xs font-semibold text-gray-600">Tên cảng <span className="text-red-500">*</span></span>
                  <input required value={form.portName}
                    onChange={e => setForm({ ...form, portName: e.target.value })}
                    placeholder="Ho Chi Minh City (Saigon)"
                    className="mt-1 w-full border border-gray-300 rounded px-2.5 py-1.5 text-sm" />
                </label>
              </div>

              <div className="grid grid-cols-3 gap-3">
                <label className="block col-span-2">
                  <span className="text-xs font-semibold text-gray-600">Quốc gia</span>
                  <input value={form.country ?? ''}
                    onChange={e => setForm({ ...form, country: e.target.value })}
                    placeholder="Vietnam"
                    className="mt-1 w-full border border-gray-300 rounded px-2.5 py-1.5 text-sm" />
                </label>
                <label className="block">
                  <span className="text-xs font-semibold text-gray-600">Mã quốc gia</span>
                  <input maxLength={2} value={form.countryCode ?? ''}
                    onChange={e => setForm({ ...form, countryCode: e.target.value.toUpperCase() })}
                    placeholder="VN"
                    className="mt-1 w-full border border-gray-300 rounded px-2.5 py-1.5 text-sm font-mono" />
                </label>
              </div>

              <div className="grid grid-cols-3 gap-3">
                <label className="block">
                  <span className="text-xs font-semibold text-gray-600">Vĩ độ</span>
                  <input type="number" step="0.0001" value={form.latitude ?? ''}
                    onChange={e => setForm({ ...form, latitude: e.target.value === '' ? undefined : Number(e.target.value) })}
                    className="mt-1 w-full border border-gray-300 rounded px-2.5 py-1.5 text-sm" />
                </label>
                <label className="block">
                  <span className="text-xs font-semibold text-gray-600">Kinh độ</span>
                  <input type="number" step="0.0001" value={form.longitude ?? ''}
                    onChange={e => setForm({ ...form, longitude: e.target.value === '' ? undefined : Number(e.target.value) })}
                    className="mt-1 w-full border border-gray-300 rounded px-2.5 py-1.5 text-sm" />
                </label>
                <label className="block">
                  <span className="text-xs font-semibold text-gray-600">Múi giờ</span>
                  <input value={form.timeZone ?? ''}
                    onChange={e => setForm({ ...form, timeZone: e.target.value })}
                    placeholder="Asia/Ho_Chi_Minh"
                    className="mt-1 w-full border border-gray-300 rounded px-2.5 py-1.5 text-sm" />
                </label>
              </div>

              <label className="flex items-center gap-2 text-sm text-gray-700">
                <input type="checkbox" checked={form.isActive}
                  onChange={e => setForm({ ...form, isActive: e.target.checked })} />
                Đang sử dụng
              </label>

              <div className="rounded bg-blue-50 border border-blue-200 px-3 py-2 text-xs text-blue-800">
                Cảng lưu tại đây sẽ tự động được phát xuống <strong>tất cả các tàu</strong>.
              </div>

              <div className="flex justify-end gap-2 pt-1">
                <button type="button" onClick={() => setModalOpen(false)}
                  className="px-3.5 py-1.5 text-sm rounded border border-gray-300 hover:bg-gray-50">Hủy</button>
                <button type="submit" disabled={saving}
                  className="px-3.5 py-1.5 text-sm rounded bg-teal-600 text-white hover:bg-teal-700 disabled:opacity-60 inline-flex items-center gap-1.5">
                  {saving && <Loader2 size={13} className="animate-spin" />}
                  {editing ? 'Lưu thay đổi' : 'Thêm cảng'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
