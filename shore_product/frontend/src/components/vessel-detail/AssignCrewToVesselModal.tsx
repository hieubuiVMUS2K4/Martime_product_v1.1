import React, { useState, useEffect, useMemo } from 'react';
import { X, Ship, Users, Check } from 'lucide-react';
import { crewApi } from '../../services/crew.service';
import type { CrewMember } from '../../types/crew.types';
import '../../pages/MasterDataManagement/Crew/AssignShipModal.css';

interface Props {
  vesselId: string;
  vesselName: string;
  onClose: () => void;
  /** Gọi sau khi gán thành công để tab cha tải lại danh sách */
  onAssigned: () => void;
}

const COLORS = ['#0a7068','#7c3aed','#059669','#d97706','#dc2626','#0891b2','#4f46e5','#15803d','#b45309','#9333ea'];
function hashColor(id: string) {
  let h = 0; for (let i = 0; i < id.length; i++) h = (h * 31 + id.charCodeAt(i)) | 0;
  return COLORS[Math.abs(h) % COLORS.length];
}
function initials(name: string) {
  const p = name.split(' ').filter(Boolean);
  return p.length >= 2 ? (p[0][0] + p[p.length - 1][0]).toUpperCase() : name.substring(0, 2).toUpperCase();
}

/**
 * Modal gán thuyền viên lên tàu — dùng trong trang chi tiết tàu.
 * Tàu đích đã biết sẵn (vesselId), nên modal liệt kê thuyền viên đang ở bờ
 * (chưa gán lên tàu nào) và cho chọn 1 hoặc nhiều người để gán cùng lúc.
 */
export const AssignCrewToVesselModal: React.FC<Props> = ({
  vesselId, vesselName, onClose, onAssigned,
}) => {
  const [pool, setPool] = useState<CrewMember[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    const fn = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); };
    document.addEventListener('keydown', fn);
    document.body.style.overflow = 'hidden';
    return () => { document.removeEventListener('keydown', fn); document.body.style.overflow = ''; };
  }, [onClose]);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      try {
        const res = await crewApi.getAll({ poolOnly: true, pageSize: 500 });
        if (!cancelled) setPool(res.data ?? []);
      } catch (err) {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Không tải được danh sách thuyền viên ở bờ');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, []);

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return pool;
    return pool.filter(c =>
      c.fullName.toLowerCase().includes(q) ||
      (c.crewId || '').toLowerCase().includes(q) ||
      (c.rankName || '').toLowerCase().includes(q)
    );
  }, [pool, search]);

  const toggle = (id: string) => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  };

  const toggleAll = () => {
    setSelectedIds(prev =>
      prev.size === filtered.length && filtered.length > 0
        ? new Set()
        : new Set(filtered.map(c => c.id))
    );
  };

  const selectedCrew = useMemo(
    () => pool.filter(c => selectedIds.has(c.id)),
    [pool, selectedIds]
  );

  const handleConfirm = async () => {
    if (selectedIds.size === 0) return;
    setSaving(true);
    setError(null);
    try {
      await Promise.all([...selectedIds].map(id => crewApi.assignToVessel(id, vesselId)));
      onAssigned();
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Không thể gán thuyền viên lên tàu');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="asm-overlay" onClick={onClose}>
      <div className="asm-dialog" onClick={e => e.stopPropagation()}>
        <div className="asm-header">
          <div className="asm-hicon asm-hicon--assign"><Ship size={18} /></div>
          <div className="asm-htitle">
            <h2>Gán thuyền viên lên tàu</h2>
            <span>{vesselName}</span>
          </div>
          <button className="asm-x" onClick={onClose}><X size={18} /></button>
        </div>

        <div className="asm-body">
          {selectedCrew.length > 0 && (
            <>
              <label className="asm-label"><Users size={14} /> Đã chọn ({selectedCrew.length})</label>
              <div className="asm-chips">
                {selectedCrew.map(c => (
                  <div key={c.id} className="asm-chip">
                    <span className="asm-chip-av" style={{ background: hashColor(c.id) }}>
                      {initials(c.fullName)}
                    </span>
                    <span className="asm-chip-name">{c.fullName}</span>
                    <span className="asm-chip-rank">{c.rankName || c.crewId}</span>
                  </div>
                ))}
              </div>
            </>
          )}

          <div className="asm-pick">
            <label className="asm-label">
              <Users size={14} /> Thuyền viên đang ở bờ
              {!loading && filtered.length > 0 && (
                <button
                  type="button"
                  onClick={toggleAll}
                  style={{
                    marginLeft: 'auto', background: 'none', border: 'none', cursor: 'pointer',
                    color: '#0a7068', fontSize: 12, fontWeight: 600,
                  }}
                >
                  {selectedIds.size === filtered.length ? 'Bỏ chọn tất cả' : 'Chọn tất cả'}
                </button>
              )}
            </label>

            {loading ? (
              <div className="asm-loading">Đang tải...</div>
            ) : (
              <>
                <input
                  className="asm-search"
                  placeholder="Tìm theo tên, mã thuyền viên hoặc chức danh..."
                  value={search}
                  onChange={e => setSearch(e.target.value)}
                />
                <div className="asm-vlist">
                  {filtered.map(c => {
                    const on = selectedIds.has(c.id);
                    return (
                      <button
                        key={c.id} type="button"
                        className={`asm-vopt${on ? ' asm-vopt--on' : ''}`}
                        onClick={() => toggle(c.id)}
                      >
                        <span
                          className="asm-chip-av"
                          style={{ background: hashColor(c.id), flexShrink: 0 }}
                        >
                          {initials(c.fullName)}
                        </span>
                        <span className="asm-vopt-name">{c.fullName}</span>
                        <span className="asm-vopt-imo">{c.rankName || c.crewId}</span>
                        {on && <Check size={16} className="asm-vopt-check" />}
                      </button>
                    );
                  })}
                  {filtered.length === 0 && (
                    <div className="asm-empty">
                      {pool.length === 0
                        ? 'Không có thuyền viên nào đang ở bờ'
                        : 'Không tìm thấy thuyền viên phù hợp'}
                    </div>
                  )}
                </div>
              </>
            )}
          </div>

          {error && (
            <div className="asm-warn" style={{ marginTop: 10 }}>
              <p style={{ color: '#b91c1c' }}>{error}</p>
            </div>
          )}
        </div>

        <div className="asm-footer">
          <button className="asm-fbtn asm-fbtn--ghost" onClick={onClose} disabled={saving}>Hủy</button>
          <button
            className="asm-fbtn asm-fbtn--assign"
            onClick={handleConfirm}
            disabled={saving || selectedIds.size === 0}
          >
            {saving ? <span className="asm-spin" /> : <Ship size={16} />}
            {saving
              ? 'Đang xử lý...'
              : `Gán ${selectedIds.size > 0 ? selectedIds.size + ' thuyền viên ' : ''}lên tàu`}
          </button>
        </div>
      </div>
    </div>
  );
};
