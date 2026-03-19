import React, { useState, useEffect } from 'react';
import { X } from 'lucide-react';
import type { CreateAssignmentRequest } from '../../types/assignment.types';
import type { Rank } from '../../types/crew.types';
import { crewApi } from '../../services/crew.service';
import type { CrewMember } from '../../types/crew.types';
import './AssignmentFormModal.css';

interface Props {
  ranks: Rank[];
  onSubmit: (data: CreateAssignmentRequest) => Promise<void>;
  onClose: () => void;
}

export const AssignmentFormModal: React.FC<Props> = ({ ranks, onSubmit, onClose }) => {
  const [form, setForm] = useState<CreateAssignmentRequest>({
    crewMemberId: '',
    vesselId: '',
    rankId: 0,
  });
  const [crewList, setCrewList] = useState<CrewMember[]>([]);
  const [vessels, setVessels] = useState<{ id: string; name: string }[]>([]);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    crewApi.getAll({ pageSize: 500 }).then(r => setCrewList(r.data)).catch(() => {});
    fetch(`${import.meta.env.VITE_API_BASE_URL || '/api'}/vessels`)
      .then(r => r.json()).then(setVessels).catch(() => {});
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.crewMemberId || !form.vesselId || !form.rankId) return;
    setSaving(true);
    try {
      await onSubmit(form);
    } finally {
      setSaving(false);
    }
  };

  const set = (field: keyof CreateAssignmentRequest, value: unknown) =>
    setForm(prev => ({ ...prev, [field]: value }));

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={e => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Tạo phân công mới</h2>
          <button className="modal-close" onClick={onClose}><X size={18} /></button>
        </div>
        <form onSubmit={handleSubmit} className="modal-body">
          <div className="form-group">
            <label>Thuyền viên *</label>
            <select required value={form.crewMemberId} onChange={e => set('crewMemberId', e.target.value)}>
              <option value="">-- Chọn thuyền viên --</option>
              {crewList.map(c => (
                <option key={c.id} value={c.id}>{c.fullName} ({c.crewId})</option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Tàu *</label>
            <select required value={form.vesselId} onChange={e => set('vesselId', e.target.value)}>
              <option value="">-- Chọn tàu --</option>
              {vessels.map(v => (
                <option key={v.id} value={v.id}>{v.name}</option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Chức danh *</label>
            <select required value={form.rankId} onChange={e => set('rankId', Number(e.target.value))}>
              <option value={0}>-- Chọn chức danh --</option>
              {ranks.map(r => (
                <option key={r.id} value={r.id}>{r.rankName} ({r.rankCode})</option>
              ))}
            </select>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Ngày bắt đầu (dự kiến)</label>
              <input type="date" value={form.plannedStartDate?.split('T')[0] || ''} onChange={e => set('plannedStartDate', e.target.value)} />
            </div>
            <div className="form-group">
              <label>Ngày kết thúc (dự kiến)</label>
              <input type="date" value={form.plannedEndDate?.split('T')[0] || ''} onChange={e => set('plannedEndDate', e.target.value)} />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Cảng lên tàu</label>
              <input type="text" placeholder="Port name" value={form.joinPortName || ''} onChange={e => set('joinPortName', e.target.value)} />
            </div>
            <div className="form-group">
              <label>Cảng rời tàu</label>
              <input type="text" placeholder="Port name" value={form.leavePortName || ''} onChange={e => set('leavePortName', e.target.value)} />
            </div>
          </div>

          <div className="form-group">
            <label>Ghi chú</label>
            <textarea rows={2} value={form.notes || ''} onChange={e => set('notes', e.target.value)} />
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-secondary" onClick={onClose}>Hủy</button>
            <button type="submit" className="btn-primary" disabled={saving}>
              {saving ? 'Đang lưu...' : 'Tạo phân công'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
