import React, { useState, useEffect, useCallback } from 'react';
import { Plus, Trash2, Loader2, ShieldCheck, Search, X, Check, Users, AlertTriangle, CheckCircle2, XCircle } from 'lucide-react';
import { ENV } from '../../config/env';
import '../../pages/VesselManagement/VesselsPage.css';

const BASE = ENV.API_BASE_URL;

interface VesselCertificateTabProps {
  vesselId: string;
  vesselName: string;
}

interface Assignment {
  id: number;
  certificateId: number;
  vesselId: string;
  assignedAt: string;
  certificateCode: string;
  certificateName: string;
  category: string | null;
  isMandatory: boolean;
}

interface AvailableCert {
  id: number;
  certificateCode: string;
  certificateName: string;
  category: string | null;
  isMandatory: boolean;
}

interface CrewCert {
  id: number;
  crewMemberId: string;
  crewMemberName: string;
  certificateCode: string;
  certificateName: string;
  category: string | null;
  certificateNumber: string | null;
  issueDate: string | null;
  expiryDate: string | null;
  issuingAuthority: string | null;
  countryName: string | null;
  documentFilePath: string | null;
  isSynced: boolean;
  status: string;
  daysUntilExpiry: number | null;
}

const CATEGORY_LABELS: Record<string, string> = {
  COMPETENCY: 'Năng lực',
  MEDICAL: 'Y tế',
  PROFICIENCY: 'Thành thạo',
  SAFETY: 'An toàn',
};

const CATEGORY_COLORS: Record<string, { bg: string; color: string }> = {
  COMPETENCY: { bg: '#dbeafe', color: '#1e40af' },
  MEDICAL: { bg: '#d1fae5', color: '#065f46' },
  PROFICIENCY: { bg: '#fef3c7', color: '#92400e' },
  SAFETY: { bg: '#fee2e2', color: '#991b1b' },
};

export function VesselCertificateTab({ vesselId, vesselName }: VesselCertificateTabProps) {
  const [activeTab, setActiveTab] = useState<'assignments' | 'crew'>('assignments');
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [available, setAvailable] = useState<AvailableCert[]>([]);
  const [crewCerts, setCrewCerts] = useState<CrewCert[]>([]);
  const [loading, setLoading] = useState(true);
  const [crewCertsLoading, setCrewCertsLoading] = useState(false);
  const [showPicker, setShowPicker] = useState(false);
  const [pickerSearch, setPickerSearch] = useState('');
  const [selectedToAdd, setSelectedToAdd] = useState<Set<number>>(new Set());
  const [saving, setSaving] = useState(false);
  const [filterCat, setFilterCat] = useState('');
  const [filterName, setFilterName] = useState('');
  const [crewCertSearch, setCrewCertSearch] = useState('');
  const [crewCertCat, setCrewCertCat] = useState('');

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const [aRes, bRes] = await Promise.all([
        fetch(`${BASE}/vessels/${vesselId}/certificates`),
        fetch(`${BASE}/vessels/${vesselId}/certificates/available`),
      ]);
      if (aRes.ok) setAssignments(await aRes.json());
      if (bRes.ok) setAvailable(await bRes.json());
    } catch { /* ignore */ }
    finally { setLoading(false); }
  }, [vesselId]);

  const fetchCrewCerts = useCallback(async () => {
    setCrewCertsLoading(true);
    try {
      const res = await fetch(`${BASE}/vessels/${vesselId}/certificates/crew`);
      if (res.ok) setCrewCerts(await res.json());
    } catch { /* ignore */ }
    finally { setCrewCertsLoading(false); }
  }, [vesselId]);

  useEffect(() => { fetchData(); }, [fetchData]);
  useEffect(() => { if (activeTab === 'crew') fetchCrewCerts(); }, [activeTab, fetchCrewCerts]);

  const handleAdd = async () => {
    if (selectedToAdd.size === 0) return;
    setSaving(true);
    try {
      const res = await fetch(`${BASE}/vessels/${vesselId}/certificates`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ certificateIds: Array.from(selectedToAdd) }),
      });
      if (res.ok) {
        setShowPicker(false);
        setSelectedToAdd(new Set());
        setPickerSearch('');
        await fetchData();
      }
    } catch { /* ignore */ }
    finally { setSaving(false); }
  };

  const handleRemove = async (assignmentId: number) => {
    if (!confirm('Bạn có chắc muốn gỡ chứng chỉ này khỏi tàu?')) return;
    try {
      await fetch(`${BASE}/vessels/${vesselId}/certificates/${assignmentId}`, { method: 'DELETE' });
      await fetchData();
    } catch { /* ignore */ }
  };

  const toggleSelect = (id: number) => {
    setSelectedToAdd(prev => {
      const s = new Set(prev);
      s.has(id) ? s.delete(id) : s.add(id);
      return s;
    });
  };

  const selectAll = (ids: number[]) => {
    setSelectedToAdd(prev => {
      const s = new Set(prev);
      const allSelected = ids.every(id => s.has(id));
      if (allSelected) ids.forEach(id => s.delete(id));
      else ids.forEach(id => s.add(id));
      return s;
    });
  };

  // Filter assignments
  const filteredAssignments = assignments.filter(a =>
    (!filterCat || a.category === filterCat) &&
    (!filterName || a.certificateName.toLowerCase().includes(filterName.toLowerCase()) || a.certificateCode.toLowerCase().includes(filterName.toLowerCase()))
  );

  // Filter picker
  const filteredAvailable = available.filter(c =>
    !pickerSearch || c.certificateName.toLowerCase().includes(pickerSearch.toLowerCase()) || c.certificateCode.toLowerCase().includes(pickerSearch.toLowerCase())
  );

  // Filter crew certs
  const filteredCrewCerts = crewCerts.filter(c =>
    (!crewCertCat || c.category === crewCertCat) &&
    (!crewCertSearch || c.certificateName.toLowerCase().includes(crewCertSearch.toLowerCase())
      || c.certificateCode.toLowerCase().includes(crewCertSearch.toLowerCase())
      || c.crewMemberName.toLowerCase().includes(crewCertSearch.toLowerCase()))
  );

  const getCertStatusStyle = (status: string) => {
    if (status === 'EXPIRED') return { bg: '#fef2f2', color: '#dc2626', label: 'Hết hạn' };
    if (status === 'EXPIRING_SOON') return { bg: '#fffbeb', color: '#d97706', label: 'Sắp hết hạn' };
    return { bg: '#f0fdf4', color: '#16a34a', label: 'Còn hiệu lực' };
  };

  // Group by category for display
  const categories = [...new Set(assignments.map(a => a.category || 'OTHER'))].sort();

  if (loading) {
    return (
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '60px', gap: '10px', color: '#6b7c8f' }}>
        <Loader2 size={22} className="spin" /><span>Đang tải danh sách chứng chỉ...</span>
      </div>
    );
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>

      {/* Tab switcher */}
      <div style={{ display: 'flex', borderBottom: '2px solid #e2eaf2', gap: '0' }}>
        <button
          onClick={() => setActiveTab('assignments')}
          style={{
            padding: '8px 18px', fontSize: '13px', fontWeight: 600, border: 'none', background: 'none', cursor: 'pointer',
            borderBottom: activeTab === 'assignments' ? '2px solid #0054a6' : '2px solid transparent',
            color: activeTab === 'assignments' ? '#0054a6' : '#6b7c8f',
            marginBottom: '-2px', display: 'flex', alignItems: 'center', gap: '6px',
          }}
        >
          <ShieldCheck size={14} /> Loại chứng chỉ yêu cầu
          <span style={{ background: activeTab === 'assignments' ? '#0054a6' : '#e2eaf2', color: activeTab === 'assignments' ? '#fff' : '#6b7c8f', borderRadius: '10px', padding: '0 7px', fontSize: '11px' }}>
            {assignments.length}
          </span>
        </button>
        <button
          onClick={() => setActiveTab('crew')}
          style={{
            padding: '8px 18px', fontSize: '13px', fontWeight: 600, border: 'none', background: 'none', cursor: 'pointer',
            borderBottom: activeTab === 'crew' ? '2px solid #0054a6' : '2px solid transparent',
            color: activeTab === 'crew' ? '#0054a6' : '#6b7c8f',
            marginBottom: '-2px', display: 'flex', alignItems: 'center', gap: '6px',
          }}
        >
          <Users size={14} /> Chứng chỉ thuyền viên
          <span style={{ background: activeTab === 'crew' ? '#0054a6' : '#e2eaf2', color: activeTab === 'crew' ? '#fff' : '#6b7c8f', borderRadius: '10px', padding: '0 7px', fontSize: '11px' }}>
            {crewCerts.length}
          </span>
        </button>
      </div>

      {/* ── TAB 1: Loại chứng chỉ yêu cầu ── */}
      {activeTab === 'assignments' && (
        <>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: '8px' }}>
            <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap', flex: 1 }}>
              <div className="vp-search-wrap" style={{ flex: 1, maxWidth: '280px' }}>
                <input className="vp-cf" placeholder="Tìm theo tên / mã chứng chỉ" value={filterName} onChange={e => setFilterName(e.target.value)} style={{ paddingLeft: '24px' }} />
              </div>
              <select className="vp-cf" value={filterCat} onChange={e => setFilterCat(e.target.value)}
                style={{ width: '160px', padding: '4px 8px', border: '1px solid #C5D9EC', borderRadius: '4px', fontSize: '11.5px', background: '#fff' }}>
                <option value="">Tất cả loại</option>
                {Object.entries(CATEGORY_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
              </select>
            </div>
            <button className="vp-btn vp-btn--primary" onClick={() => setShowPicker(true)}>
              <Plus size={13} /> Gán chứng chỉ
            </button>
          </div>

          <div className="vp-table-card">
            <table className="vp-table">
              <thead>
                <tr className="vp-tr-labels">
                  <th style={{ width: '50px' }}>#</th>
                  <th>Mã</th>
                  <th>Tên chứng chỉ</th>
                  <th>Loại</th>
                  <th>Bắt buộc</th>
                  <th>Ngày gán</th>
                  <th style={{ width: '60px' }}></th>
                </tr>
              </thead>
              <tbody>
                {filteredAssignments.length === 0 ? (
                  <tr><td colSpan={7} className="vp-empty">
                    <ShieldCheck size={24} />
                    <p>{assignments.length === 0 ? 'Chưa có chứng chỉ nào được gán cho tàu này' : 'Không tìm thấy'}</p>
                    {assignments.length === 0 && <button className="vp-btn vp-btn--primary" onClick={() => setShowPicker(true)}><Plus size={13} /> Gán chứng chỉ</button>}
                  </td></tr>
                ) : filteredAssignments.map((a, idx) => {
                  const catCol = CATEGORY_COLORS[a.category || ''] || { bg: '#f1f5f9', color: '#475569' };
                  return (
                    <tr key={a.id} className={`vp-tr${idx % 2 === 1 ? ' vp-tr--alt' : ''}`}>
                      <td style={{ color: '#6b7c8f', fontSize: '11.5px' }}>{idx + 1}</td>
                      <td style={{ fontWeight: 600, fontSize: '12px', fontFamily: 'monospace' }}>{a.certificateCode}</td>
                      <td style={{ fontWeight: 500 }}>{a.certificateName}</td>
                      <td>
                        <span style={{ display: 'inline-block', padding: '1px 8px', borderRadius: '999px', fontSize: '11px', fontWeight: 500, background: catCol.bg, color: catCol.color }}>
                          {CATEGORY_LABELS[a.category || ''] || a.category || '—'}
                        </span>
                      </td>
                      <td>{a.isMandatory ? <span style={{ color: '#dc2626', fontWeight: 600, fontSize: '11.5px' }}>Bắt buộc</span> : <span style={{ color: '#6b7c8f', fontSize: '11.5px' }}>Tùy chọn</span>}</td>
                      <td style={{ color: '#6b7c8f', fontSize: '12px' }}>{new Date(a.assignedAt).toLocaleDateString('vi-VN')}</td>
                      <td>
                        <button className="vp-icon-btn" title="Gỡ chứng chỉ" onClick={() => handleRemove(a.id)}
                          style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#6b7c8f', padding: '4px', borderRadius: '4px' }}
                          onMouseEnter={e => { e.currentTarget.style.color = '#dc2626'; e.currentTarget.style.background = '#fef2f2'; }}
                          onMouseLeave={e => { e.currentTarget.style.color = '#6b7c8f'; e.currentTarget.style.background = 'none'; }}>
                          <Trash2 size={14} />
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
          <div style={{ fontSize: '12px', color: '#6b7c8f' }}>Hiển thị {filteredAssignments.length} / {assignments.length} chứng chỉ</div>
        </>
      )}

      {/* ── TAB 2: Chứng chỉ thuyền viên từ snapshot ── */}
      {activeTab === 'crew' && (
        <>
          <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap', alignItems: 'center' }}>
            <div style={{ position: 'relative', flex: 1, maxWidth: '300px' }}>
              <Search size={13} style={{ position: 'absolute', left: '8px', top: '50%', transform: 'translateY(-50%)', color: '#99aab8', pointerEvents: 'none' }} />
              <input placeholder="Tìm thuyền viên, tên / mã chứng chỉ..." value={crewCertSearch} onChange={e => setCrewCertSearch(e.target.value)}
                style={{ width: '100%', padding: '5px 8px 5px 28px', border: '1px solid #C5D9EC', borderRadius: '4px', fontSize: '12px', outline: 'none' }} />
            </div>
            <select value={crewCertCat} onChange={e => setCrewCertCat(e.target.value)}
              style={{ padding: '5px 8px', border: '1px solid #C5D9EC', borderRadius: '4px', fontSize: '12px', background: '#fff' }}>
              <option value="">Tất cả loại</option>
              {Object.entries(CATEGORY_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
            <button className="vp-btn vp-btn--ghost" onClick={fetchCrewCerts} title="Làm mới" style={{ marginLeft: 'auto' }}>
              <Loader2 size={13} className={crewCertsLoading ? 'spin' : ''} />
            </button>
          </div>

          {crewCertsLoading ? (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '40px', gap: '10px', color: '#6b7c8f' }}>
              <Loader2 size={20} className="spin" /><span>Đang tải chứng chỉ thuyền viên...</span>
            </div>
          ) : (
            <div className="vp-table-card">
              <table className="vp-table">
                <thead>
                  <tr className="vp-tr-labels">
                    <th style={{ width: '40px' }}>#</th>
                    <th>Thuyền viên</th>
                    <th>Mã CC</th>
                    <th>Tên chứng chỉ</th>
                    <th>Loại</th>
                    <th>Số CC</th>
                    <th>Ngày hết hạn</th>
                    <th>Còn lại</th>
                    <th>Trạng thái</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredCrewCerts.length === 0 ? (
                    <tr><td colSpan={9} className="vp-empty">
                      <Users size={24} />
                      <p>{crewCerts.length === 0 ? 'Chưa có dữ liệu chứng chỉ từ snapshot — hãy thực hiện sync từ tàu' : 'Không tìm thấy'}</p>
                    </td></tr>
                  ) : filteredCrewCerts.map((c, idx) => {
                    const st = getCertStatusStyle(c.status);
                    const catCol = CATEGORY_COLORS[c.category || ''] || { bg: '#f1f5f9', color: '#475569' };
                    return (
                      <tr key={c.id} className={`vp-tr${idx % 2 === 1 ? ' vp-tr--alt' : ''}`}>
                        <td style={{ color: '#6b7c8f', fontSize: '11.5px' }}>{idx + 1}</td>
                        <td style={{ fontWeight: 500, fontSize: '12px' }}>{c.crewMemberName}</td>
                        <td style={{ fontFamily: 'monospace', fontWeight: 600, fontSize: '11.5px' }}>{c.certificateCode}</td>
                        <td style={{ fontSize: '12.5px' }}>{c.certificateName}</td>
                        <td>
                          <span style={{ display: 'inline-block', padding: '1px 7px', borderRadius: '999px', fontSize: '10.5px', fontWeight: 500, background: catCol.bg, color: catCol.color }}>
                            {CATEGORY_LABELS[c.category || ''] || c.category || '—'}
                          </span>
                        </td>
                        <td style={{ color: '#6b7c8f', fontSize: '11.5px' }}>{c.certificateNumber || '—'}</td>
                        <td style={{ fontSize: '12px' }}>{c.expiryDate ? new Date(c.expiryDate).toLocaleDateString('vi-VN') : '—'}</td>
                        <td style={{ fontSize: '12px' }}>
                          {c.daysUntilExpiry !== null ? (
                            <span style={{ fontWeight: 600, color: c.daysUntilExpiry <= 0 ? '#dc2626' : c.daysUntilExpiry <= 30 ? '#d97706' : '#16a34a' }}>
                              {c.daysUntilExpiry <= 0 ? 'Quá hạn' : `${c.daysUntilExpiry}d`}
                            </span>
                          ) : '—'}
                        </td>
                        <td>
                          <span style={{ display: 'inline-block', padding: '1px 8px', borderRadius: '999px', fontSize: '11px', fontWeight: 600, background: st.bg, color: st.color }}>
                            {st.label}
                          </span>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
          <div style={{ fontSize: '12px', color: '#6b7c8f' }}>
            Hiển thị {filteredCrewCerts.length} / {crewCerts.length} chứng chỉ thuyền viên
          </div>
        </>
      )}

      {/* Picker Modal */}
      {showPicker && (
        <div className="vp-overlay" onClick={() => { setShowPicker(false); setSelectedToAdd(new Set()); }}>
          <div className="vp-modal" onClick={e => e.stopPropagation()} style={{ maxWidth: '640px' }}>
            <div className="vp-modal-head">
              <h2>Gán chứng chỉ cho {vesselName}</h2>
              <button className="vp-icon-btn" onClick={() => { setShowPicker(false); setSelectedToAdd(new Set()); }}><X size={15} /></button>
            </div>
            <div style={{ padding: '10px 16px 6px' }}>
              <div style={{ position: 'relative' }}>
                <Search size={14} style={{ position: 'absolute', left: '10px', top: '50%', transform: 'translateY(-50%)', color: '#99aab8', pointerEvents: 'none' }} />
                <input type="text" placeholder="Tìm chứng chỉ..." value={pickerSearch} onChange={e => setPickerSearch(e.target.value)}
                  style={{ width: '100%', padding: '7px 10px 7px 32px', border: '1px solid #C5D9EC', borderRadius: '5px', fontSize: '12.5px', outline: 'none' }} />
              </div>
            </div>
            <div style={{ maxHeight: '380px', overflowY: 'auto', padding: '0 16px 8px' }}>
              {filteredAvailable.length === 0 ? (
                <p style={{ textAlign: 'center', color: '#6b7c8f', padding: '24px', fontSize: '13px' }}>
                  {available.length === 0 ? 'Tất cả chứng chỉ đã được gán cho tàu này' : 'Không tìm thấy'}
                </p>
              ) : (
                <table className="vp-table" style={{ fontSize: '12px' }}>
                  <thead>
                    <tr className="vp-tr-labels">
                      <th style={{ width: '36px', textAlign: 'center' }}>
                        <input type="checkbox" checked={filteredAvailable.length > 0 && filteredAvailable.every(c => selectedToAdd.has(c.id))}
                          onChange={() => selectAll(filteredAvailable.map(c => c.id))} style={{ cursor: 'pointer' }} />
                      </th>
                      <th>Mã</th><th>Tên chứng chỉ</th><th>Loại</th><th>Bắt buộc</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredAvailable.map((c, idx) => {
                      const sel = selectedToAdd.has(c.id);
                      const catCol = CATEGORY_COLORS[c.category || ''] || { bg: '#f1f5f9', color: '#475569' };
                      return (
                        <tr key={c.id} className={`vp-tr${idx % 2 === 1 ? ' vp-tr--alt' : ''}${sel ? ' vp-tr--selected' : ''}`}
                          onClick={() => toggleSelect(c.id)} style={{ cursor: 'pointer' }}>
                          <td style={{ textAlign: 'center' }}>
                            <input type="checkbox" checked={sel} onChange={() => toggleSelect(c.id)} style={{ cursor: 'pointer' }} />
                          </td>
                          <td style={{ fontFamily: 'monospace', fontWeight: 600 }}>{c.certificateCode}</td>
                          <td style={{ fontWeight: 500 }}>{c.certificateName}</td>
                          <td>
                            <span style={{ display: 'inline-block', padding: '1px 7px', borderRadius: '999px', fontSize: '10.5px', fontWeight: 500, background: catCol.bg, color: catCol.color }}>
                              {CATEGORY_LABELS[c.category || ''] || c.category || '—'}
                            </span>
                          </td>
                          <td>{c.isMandatory ? <span style={{ color: '#dc2626', fontWeight: 600, fontSize: '11px' }}>Bắt buộc</span> : <span style={{ color: '#6b7c8f', fontSize: '11px' }}>Tùy chọn</span>}</td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              )}
            </div>
            <div className="vp-form-footer">
              <span style={{ fontSize: '12px', color: '#6b7c8f' }}>
                {selectedToAdd.size > 0 ? `Đã chọn ${selectedToAdd.size} chứng chỉ` : 'Chọn chứng chỉ để gán'}
              </span>
              <div style={{ display: 'flex', gap: '8px' }}>
                <button className="vp-btn" onClick={() => { setShowPicker(false); setSelectedToAdd(new Set()); }}>Hủy</button>
                <button className="vp-btn vp-btn--primary" disabled={selectedToAdd.size === 0 || saving} onClick={handleAdd}>
                  {saving && <Loader2 size={12} className="spin" />}
                  {saving ? 'Đang gán...' : <><Check size={13} /> Gán {selectedToAdd.size > 0 ? `(${selectedToAdd.size})` : ''}</>}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
