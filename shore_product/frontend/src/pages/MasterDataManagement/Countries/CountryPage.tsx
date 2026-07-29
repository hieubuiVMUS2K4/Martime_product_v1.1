import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { Plus, Pencil, Trash2, Loader2, Globe, X, ChevronLeft, ChevronRight } from 'lucide-react';
import { countryApi, type CountryPayload } from '../../../services/crew.service';
import { useToast } from '../../../components/common/Toast';
import { useConfirmDialog } from '../../../components/common/ConfirmDialog';
import type { Country } from '../../../types/crew.types';
import '../Crew/CrewListPage.css';
import '../CertificateTypes/CertificateFormModal.css';

const emptyForm: CountryPayload = { countryCode: '', countryName: '', flagImageUrl: '' };

/* ═══════════════════════════════════════════════════════════════ */
export const CountryPage: React.FC = () => {
  const toast = useToast();
  const { confirm } = useConfirmDialog();

  const [countries, setCountries] = useState<Country[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  /* filters */
  const [searchCode, setSearchCode] = useState('');
  const [searchName, setSearchName] = useState('');

  /* form modal */
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Country | null>(null);
  const [form, setForm] = useState<CountryPayload>(emptyForm);

  /* context menu */
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; country: Country } | null>(null);
  const [selectedRowId, setSelectedRowId] = useState<number | null>(null);

  /* ── data ── */
  const fetchCountries = useCallback(async () => {
    setLoading(true);
    try { setCountries(await countryApi.getAll()); }
    catch { toast.error('Không thể tải danh sách quốc gia'); }
    finally { setLoading(false); }
  }, [toast]);

  useEffect(() => { fetchCountries(); }, [fetchCountries]);

  const [page, setPage] = useState(1);

  const filtered = useMemo(() => countries.filter(c => {
    if (searchCode && !c.countryCode.toLowerCase().includes(searchCode.toLowerCase())) return false;
    if (searchName && !c.countryName.toLowerCase().includes(searchName.toLowerCase())) return false;
    return true;
  }), [countries, searchCode, searchName]);

  /* ── phân trang phía client, cùng cỡ trang với CrewListPage ── */
  const PAGE_SIZE = 15;
  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  useEffect(() => { if (page > totalPages) setPage(totalPages); }, [page, totalPages]);

  /* ── context menu ── */
  const handleContextMenu = useCallback((e: React.MouseEvent, country: Country) => {
    e.preventDefault();
    setContextMenu({ x: e.clientX, y: e.clientY, country });
    setSelectedRowId(country.id);
  }, []);
  const closeContextMenu = useCallback(() => { setContextMenu(null); setSelectedRowId(null); }, []);
  useEffect(() => {
    const h = () => closeContextMenu();
    window.addEventListener('click', h);
    return () => window.removeEventListener('click', h);
  }, [closeContextMenu]);

  /* ── form actions ── */
  const openCreate = useCallback(() => { setEditing(null); setForm(emptyForm); setShowForm(true); }, []);
  const openEdit = useCallback((c: Country) => {
    setEditing(c);
    setForm({ countryCode: c.countryCode, countryName: c.countryName, flagImageUrl: c.flagImageUrl || '' });
    setShowForm(true);
  }, []);

  const handleDelete = useCallback(async (c: Country) => {
    const { confirmed } = await confirm({
      title: 'Xóa quốc gia', message: `Bạn có chắc muốn xóa "${c.countryName}"?`,
      confirmLabel: 'Xóa', cancelLabel: 'Hủy', variant: 'danger',
    });
    if (!confirmed) return;
    try { await countryApi.remove(c.id); toast.success('Đã xóa quốc gia'); fetchCountries(); }
    catch (err) { toast.error('Lỗi xóa', err instanceof Error ? err.message : 'Không thể xóa'); }
  }, [confirm, toast, fetchCountries]);

  const handleSubmit = useCallback(async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.countryCode.trim() || !form.countryName.trim()) { toast.error('Vui lòng nhập mã và tên quốc gia'); return; }
    setSaving(true);
    try {
      if (editing) { await countryApi.update(editing.id, form); toast.success('Đã cập nhật quốc gia'); }
      else { await countryApi.create(form); toast.success('Đã tạo quốc gia mới'); }
      setShowForm(false); fetchCountries();
    } catch (err) { toast.error('Lỗi lưu', err instanceof Error ? err.message : 'Không thể lưu'); }
    finally { setSaving(false); }
  }, [editing, form, toast, fetchCountries]);

  /* ── loading ── */
  if (loading) {
    return <div className="cl-loading"><Loader2 size={28} className="spin" /><p>Đang tải danh sách quốc gia...</p></div>;
  }

  /* ═══════════════════════ RENDER ═══════════════════════ */
  return (
    <div className="cl-page" style={{ padding: 0, minHeight: 'auto' }}>
      {/* Header */}
      <div className="cl-header">
        <div className="cl-header-left">
          <Globe size={16} className="cl-header-icon" />
          <h1 className="cl-title">Quốc gia</h1>
          <span className="cl-count-badge">{countries.length}</span>
        </div>
        <div className="cl-header-right">
          <button className="cl-btn cl-btn--primary" onClick={openCreate}><Plus size={13} /> Thêm quốc gia</button>
        </div>
      </div>

      {/* Table */}
      <div className="cl-table-card">
        <table className="cl-table">
          <thead>
            <tr className="cl-tr-labels">
              <th style={{ width: 44, textAlign: 'center' }}>STT</th>
              <th style={{ width: '16%' }}>Mã quốc gia</th>
              <th>Tên quốc gia</th>
              <th style={{ width: '12%', textAlign: 'center' }}>Cờ</th>
              <th style={{ width: '12%', textAlign: 'center' }}>Trạng thái</th>
            </tr>
            <tr className="cl-tr-filters">
              <th></th>
              <th><div className="cl-search-wrap"><input className="cl-cf" placeholder="Tìm mã" value={searchCode} onChange={e => setSearchCode(e.target.value)} /></div></th>
              <th><div className="cl-search-wrap"><input className="cl-cf" placeholder="Tìm tên" value={searchName} onChange={e => setSearchName(e.target.value)} /></div></th>
              <th></th><th></th>
            </tr>
          </thead>
          <tbody>
            {filtered.length === 0 ? (
              <tr><td colSpan={5} className="cl-empty">
                <Globe size={24} />
                <p>{countries.length === 0 ? 'Chưa có quốc gia nào' : 'Không tìm thấy quốc gia phù hợp'}</p>
              </td></tr>
            ) : paged.map((c, idx) => (
              <tr key={c.id}
                className={`cl-tr${idx % 2 === 1 ? ' cl-tr--alt' : ''}${selectedRowId === c.id ? ' cl-tr--selected' : ''}`}
                onContextMenu={e => handleContextMenu(e, c)}
              >
                <td style={{ textAlign: 'center' }}>{idx + 1}</td>
                <td><span className="cl-code">{c.countryCode}</span></td>
                <td>{c.countryName}</td>
                <td style={{ textAlign: 'center' }}>
                  {c.flagImageUrl ? <img src={c.flagImageUrl} alt={c.countryCode} style={{ height: 16, width: 24, objectFit: 'cover', borderRadius: 2, verticalAlign: 'middle' }} /> : '—'}
                </td>
                <td style={{ textAlign: 'center' }}>
                  <span className="cl-status-badge cl-status-badge--on"><span className="cl-status-badge__dot" /> Hoạt động</span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Chân trang — cùng khuôn với CrewListPage */}
      <div className="cl-footer">
        <span className="cl-footer-info">
          Hiển thị {paged.length} / {filtered.length} quốc gia
        </span>
        {totalPages > 1 && (
          <div className="cl-pagi-btns">
            <button className="cl-pagi-btn" disabled={page <= 1} onClick={() => setPage(p => Math.max(1, p - 1))}><ChevronLeft size={14} /></button>
            {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => {
              let p: number;
              if (totalPages <= 7) p = i + 1;
              else if (page <= 4) p = i + 1;
              else if (page >= totalPages - 3) p = totalPages - 6 + i;
              else p = page - 3 + i;
              return <button key={p} className={`cl-pagi-btn${p === page ? ' cl-pagi-btn--cur' : ''}`} onClick={() => setPage(p)}>{p}</button>;
            })}
            <button className="cl-pagi-btn" disabled={page >= totalPages} onClick={() => setPage(p => Math.min(totalPages, p + 1))}><ChevronRight size={14} /></button>
          </div>
        )}
      </div>

      {/* Context Menu */}
      {contextMenu && (
        <div className="cl-context-menu" style={{ left: contextMenu.x, top: contextMenu.y }} onClick={e => e.stopPropagation()}>
          <button className="cl-ctx-item" onClick={() => { openEdit(contextMenu.country); closeContextMenu(); }}>
            <Pencil size={13} /> Chỉnh sửa
          </button>
          <div className="cl-ctx-divider" />
          <button className="cl-ctx-item cl-ctx-item--danger" onClick={() => { handleDelete(contextMenu.country); closeContextMenu(); }}>
            <Trash2 size={13} /> Xóa quốc gia
          </button>
        </div>
      )}

      {/* Form Modal */}
      {showForm && (
        <div className="modal-backdrop" onClick={() => setShowForm(false)}>
          <div className="cert-form-modal" onClick={e => e.stopPropagation()} style={{ maxWidth: 480 }}>
            <div className="cfm-header">
              <span className="cfm-title">{editing ? 'Chỉnh sửa quốc gia' : 'Thêm quốc gia mới'}</span>
              <button className="cfm-close" onClick={() => setShowForm(false)}><X size={18} /></button>
            </div>
            <form onSubmit={handleSubmit}>
              <div className="cfm-body">
                <div className="cfm-grid">
                  <div className="cfm-field cfm-field--required">
                    <label>Mã quốc gia</label>
                    <input value={form.countryCode} onChange={e => setForm(f => ({ ...f, countryCode: e.target.value }))} placeholder="VD: VN" style={{ fontFamily: 'monospace' }} />
                  </div>
                  <div className="cfm-field cfm-field--required">
                    <label>Tên quốc gia</label>
                    <input value={form.countryName} onChange={e => setForm(f => ({ ...f, countryName: e.target.value }))} placeholder="VD: Việt Nam" />
                  </div>
                  <div className="cfm-field cfm-field--full">
                    <label>URL ảnh cờ</label>
                    <input value={form.flagImageUrl} onChange={e => setForm(f => ({ ...f, flagImageUrl: e.target.value }))} placeholder="https://..." />
                  </div>
                </div>
              </div>
              <div className="cfm-footer">
                <button type="button" className="cl-btn cl-btn--ghost" onClick={() => setShowForm(false)}>Hủy</button>
                <button type="submit" className="cl-btn cl-btn--primary" disabled={saving}>
                  {saving ? <><Loader2 size={14} className="spin" /> Đang lưu...</> : editing ? 'Cập nhật' : 'Tạo mới'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
