import React, { useState, useEffect } from 'react';
import { X, Ship, Anchor, Users, Check } from 'lucide-react';
import { useVessels } from '../../hooks/useCrew';
import type { CrewMember } from '../../types/crew.types';
import './AssignShipModal.css';

interface Props {
  crewMembers: CrewMember[];
  mode: 'assign' | 'unassign';
  onClose: () => void;
  onAssign: (crewIds: string[], vesselId: string) => Promise<void>;
  onUnassign: (crewIds: string[]) => Promise<void>;
  saving: boolean;
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

export const AssignShipModal: React.FC<Props> = ({
  crewMembers, mode, onClose, onAssign, onUnassign, saving,
}) => {
  const { vessels, loading } = useVessels();
  const [vesselId, setVesselId] = useState('');
  const [search, setSearch] = useState('');

  useEffect(() => {
    const fn = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); };
    document.addEventListener('keydown', fn);
    document.body.style.overflow = 'hidden';
    return () => { document.removeEventListener('keydown', fn); document.body.style.overflow = ''; };
  }, [onClose]);

  const filtered = vessels.filter(v =>
    v.name.toLowerCase().includes(search.toLowerCase()) || v.imo.includes(search)
  );
  const chosen = vessels.find(v => v.id === vesselId);

  const handleConfirm = async () => {
    const ids = crewMembers.map(c => c.id);
    if (mode === 'assign') { if (!vesselId) return; await onAssign(ids, vesselId); }
    else { await onUnassign(ids); }
  };

  return (
    <div className="asm-overlay" onClick={onClose}>
      <div className="asm-dialog" onClick={e => e.stopPropagation()}>
        <div className="asm-header">
          <div className={`asm-hicon asm-hicon--${mode}`}>
            {mode === 'assign' ? <Ship size={18} /> : <Anchor size={18} />}
          </div>
          <div className="asm-htitle">
            <h2>{mode === 'assign' ? 'Gán thuyền viên lên tàu' : 'Rút thuyền viên về bờ'}</h2>
            <span>{crewMembers.length} thuyền viên được chọn</span>
          </div>
          <button className="asm-x" onClick={onClose}><X size={18} /></button>
        </div>

        <div className="asm-body">
          <label className="asm-label"><Users size={14} /> Danh sách thuyền viên</label>
          <div className="asm-chips">
            {crewMembers.map(c => (
              <div key={c.id} className="asm-chip">
                <span className="asm-chip-av" style={{ background: hashColor(c.id) }}>
                  {initials(c.fullName)}
                </span>
                <span className="asm-chip-name">{c.fullName}</span>
                <span className="asm-chip-rank">{c.rankName || c.crewId}</span>
                {c.vesselName && <span className="asm-chip-tag">{c.vesselName}</span>}
              </div>
            ))}
          </div>

          {mode === 'assign' ? (
            <div className="asm-pick">
              <label className="asm-label"><Ship size={14} /> Chọn tàu đích</label>
              {loading ? <div className="asm-loading">Đang tải...</div> : (
                <>
                  <input
                    className="asm-search"
                    placeholder="Tìm tàu theo tên hoặc IMO..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                  />
                  <div className="asm-vlist">
                    {filtered.map(v => (
                      <button
                        key={v.id} type="button"
                        className={`asm-vopt${vesselId === v.id ? ' asm-vopt--on' : ''}`}
                        onClick={() => setVesselId(v.id)}
                      >
                        <Ship size={15} className="asm-vopt-icon" />
                        <span className="asm-vopt-name">{v.name}</span>
                        <span className="asm-vopt-imo">IMO {v.imo}</span>
                        {vesselId === v.id && <Check size={16} className="asm-vopt-check" />}
                      </button>
                    ))}
                    {filtered.length === 0 && <div className="asm-empty">Không tìm thấy tàu nào</div>}
                  </div>
                </>
              )}
            </div>
          ) : (
            <div className="asm-warn">
              <Anchor size={16} />
              <p>Các thuyền viên sẽ được rút khỏi tàu hiện tại và chuyển về trạng thái <strong>Pool</strong> (ở bờ).</p>
            </div>
          )}
        </div>

        <div className="asm-footer">
          <button className="asm-fbtn asm-fbtn--ghost" onClick={onClose} disabled={saving}>Hủy</button>
          <button
            className={`asm-fbtn asm-fbtn--${mode}`}
            onClick={handleConfirm}
            disabled={saving || (mode === 'assign' && !vesselId)}
          >
            {saving ? <span className="asm-spin" /> : mode === 'assign' ? <Ship size={16} /> : <Anchor size={16} />}
            {saving ? 'Đang xử lý...' : mode === 'assign'
              ? `Gán lên ${chosen?.name || 'tàu'}`
              : 'Xác nhận rút về bờ'}
          </button>
        </div>
      </div>
    </div>
  );
};
