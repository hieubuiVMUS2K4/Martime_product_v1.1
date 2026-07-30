import React, { useState, useEffect, useCallback } from 'react';
import { Plus, Trash2, Loader2, ShieldCheck, Search, X, Check, Users, AlertTriangle, CheckCircle2, XCircle } from 'lucide-react';
import { ENV } from '../../config/env';
import '../../pages/VesselManagement/VesselsPage.css';
// Dung chung he lop cl-* voi VesselCrewTab de hai tab trong chi tiet tau cung mot khuon.
import '../../pages/MasterDataManagement/Crew/CrewListPage.css';

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
  COMPETENCY: { bg: '#dce9f8', color: '#1b4c7e' },
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
  const [assignPage, setAssignPage] = useState(1);
  const [crewPage, setCrewPage] = useState(1);
  const [filterCode, setFilterCode] = useState('');
  const [filterMandatory, setFilterMandatory] = useState('');
  const [filterCrewName, setFilterCrewName] = useState('');
  const [filterCertCode, setFilterCertCode] = useState('');

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
    (!filterCode || a.certificateCode.toLowerCase().includes(filterCode.toLowerCase())) &&
    (!filterName || a.certificateName.toLowerCase().includes(filterName.toLowerCase())) &&
    (!filterMandatory || (filterMandatory === 'yes' ? a.isMandatory : !a.isMandatory))
  );

  // Filter picker
  const filteredAvailable = available.filter(c =>
    !pickerSearch || c.certificateName.toLowerCase().includes(pickerSearch.toLowerCase()) || c.certificateCode.toLowerCase().includes(pickerSearch.toLowerCase())
  );

  // Filter crew certs
  const filteredCrewCerts = crewCerts.filter(c =>
    (!crewCertCat || c.category === crewCertCat) &&
    (!filterCrewName || c.crewMemberName.toLowerCase().includes(filterCrewName.toLowerCase())) &&
    (!filterCertCode || c.certificateCode.toLowerCase().includes(filterCertCode.toLowerCase())) &&
    (!crewCertSearch || c.certificateName.toLowerCase().includes(crewCertSearch.toLowerCase()))
  );

  // Phân trang phía client — dữ liệu đã tải hết sẵn nên không cần gọi lại API.
  const PAGE_SIZE = 15;
  const assignTotalPages = Math.max(1, Math.ceil(filteredAssignments.length / PAGE_SIZE));
  const crewTotalPages = Math.max(1, Math.ceil(filteredCrewCerts.length / PAGE_SIZE));
  const pagedAssignments = filteredAssignments.slice((assignPage - 1) * PAGE_SIZE, assignPage * PAGE_SIZE);
  const pagedCrewCerts = filteredCrewCerts.slice((crewPage - 1) * PAGE_SIZE, crewPage * PAGE_SIZE);

  // Lọc xong mà đang đứng ở trang không còn tồn tại thì kéo về trang cuối hợp lệ,
  // nếu không bảng sẽ trống trơn dù vẫn có kết quả.
  useEffect(() => { if (assignPage > assignTotalPages) setAssignPage(assignTotalPages); }, [assignPage, assignTotalPages]);
  useEffect(() => { if (crewPage > crewTotalPages) setCrewPage(crewTotalPages); }, [crewPage, crewTotalPages]);

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
    <div className="cl-page relative">

      {/* Tab switcher */}
      <div style={{ display: 'flex', borderBottom: '2px solid #e2eaf2', gap: '0' }}>
        <button
          onClick={() => setActiveTab('assignments')}
          style={{
            padding: '8px 18px', fontSize: '13px', fontWeight: 600, border: 'none', background: 'none', cursor: 'pointer',
            borderBottom: activeTab === 'assignments' ? '2px solid #0b2545' : '2px solid transparent',
            color: activeTab === 'assignments' ? '#0b2545' : '#6b7c8f',
            marginBottom: '-2px', display: 'flex', alignItems: 'center', gap: '6px',
          }}
        >
          <ShieldCheck size={14} /> Loại chứng chỉ yêu cầu
          <span style={{ background: activeTab === 'assignments' ? '#0b2545' : '#e2eaf2', color: activeTab === 'assignments' ? '#fff' : '#6b7c8f', borderRadius: '10px', padding: '0 7px', fontSize: '11px' }}>
            {assignments.length}
          </span>
        </button>
        <button
          onClick={() => setActiveTab('crew')}
          style={{
            padding: '8px 18px', fontSize: '13px', fontWeight: 600, border: 'none', background: 'none', cursor: 'pointer',
            borderBottom: activeTab === 'crew' ? '2px solid #0b2545' : '2px solid transparent',
            color: activeTab === 'crew' ? '#0b2545' : '#6b7c8f',
            marginBottom: '-2px', display: 'flex', alignItems: 'center', gap: '6px',
          }}
        >
          <Users size={14} /> Chứng chỉ thuyền viên
          <span style={{ background: activeTab === 'crew' ? '#0b2545' : '#e2eaf2', color: activeTab === 'crew' ? '#fff' : '#6b7c8f', borderRadius: '10px', padding: '0 7px', fontSize: '11px' }}>
            {crewCerts.length}
          </span>
        </button>
      </div>

      {/* ── TAB 1: Loại chứng chỉ yêu cầu ── */}
      {activeTab === 'assignments' && (
        <>
          <div className="cl-header">
            <div className="cl-header-left">
              <h1 className="cl-title">Loại chứng chỉ yêu cầu</h1>
              <span className="cl-count-badge">{filteredAssignments.length}</span>
            </div>
            <div className="cl-header-right">
              <button className="cl-btn cl-btn--primary" onClick={() => setShowPicker(true)}>
                <Plus size={13} /> Gán chứng chỉ
              </button>
            </div>
          </div>

          <div className="cl-table-card">
            <table className="cl-table">
              <thead>
                <tr className="cl-tr-labels">
                  <th style={{ width: '17%' }}>Mã</th>
                  <th style={{ width: '36%' }}>Tên chứng chỉ</th>
                  <th style={{ width: '12%' }}>Loại</th>
                  <th style={{ width: '12%' }}>Bắt buộc</th>
                  <th style={{ width: '15%' }}>Ngày gán</th>
                  <th style={{ width: '6%', borderRight: 'none' }}></th>
                </tr>
                {/* Hàng lọc theo từng cột — cùng khuôn với VesselCrewTab */}
                <tr className="cl-tr-filters">
                  <th>
                    <div className="cl-search-wrap">
                      <input className="cl-cf" placeholder="Tìm kiếm" value={filterCode} onChange={e => { setFilterCode(e.target.value); setAssignPage(1); }} />
                    </div>
                  </th>
                  <th>
                    <div className="cl-search-wrap">
                      <input className="cl-cf" placeholder="Tìm kiếm" value={filterName} onChange={e => { setFilterName(e.target.value); setAssignPage(1); }} />
                    </div>
                  </th>
                  <th>
                    <select className="cl-cf" value={filterCat} onChange={e => { setFilterCat(e.target.value); setAssignPage(1); }}>
                      <option value="">Tất cả</option>
                      {Object.entries(CATEGORY_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
                    </select>
                  </th>
                  <th>
                    <select className="cl-cf" value={filterMandatory} onChange={e => { setFilterMandatory(e.target.value); setAssignPage(1); }}>
                      <option value="">Tất cả</option>
                      <option value="yes">Bắt buộc</option>
                      <option value="no">Tùy chọn</option>
                    </select>
                  </th>
                  <th></th>
                  <th style={{ borderRight: 'none' }}></th>
                </tr>
              </thead>
              <tbody>
                {filteredAssignments.length === 0 ? (
                  <tr><td colSpan={6} className="cl-empty">
                    <ShieldCheck size={24} />
                    <p>{assignments.length === 0 ? 'Chưa có chứng chỉ nào được gán cho tàu này' : 'Không tìm thấy'}</p>
                    {assignments.length === 0 && <button className="cl-btn cl-btn--primary" onClick={() => setShowPicker(true)}><Plus size={13} /> Gán chứng chỉ</button>}
                  </td></tr>
                ) : pagedAssignments.map((a, idx) => {
                  const catCol = CATEGORY_COLORS[a.category || ''] || { bg: '#f1f5f9', color: '#475569' };
                  return (
                    <tr key={a.id} className={`cl-tr${idx % 2 === 1 ? ' cl-tr--alt' : ''}`}>
                        <td style={{ fontWeight: 600, fontSize: '12px', fontFamily: 'monospace' }}>{a.certificateCode}</td>
                      <td style={{ fontWeight: 500 }}>{a.certificateName}</td>
                      <td >
                        <span style={{ display: 'inline-block', padding: '1px 8px', borderRadius: '999px', fontSize: '11px', fontWeight: 500, background: catCol.bg, color: catCol.color }}>
                          {CATEGORY_LABELS[a.category || ''] || a.category || '—'}
                        </span>
                      </td>
                      <td >{a.isMandatory ? <span style={{ color: '#dc2626', fontWeight: 600, fontSize: '11.5px' }}>Bắt buộc</span> : <span style={{ color: '#6b7c8f', fontSize: '11.5px' }}>Tùy chọn</span>}</td>
                      <td style={{ color: '#6b7c8f', fontSize: '12px' }}>{new Date(a.assignedAt).toLocaleDateString('vi-VN')}</td>
                      <td style={{ borderRight: 'none' }}>
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
          {assignTotalPages > 1 && (
            <div className="bg-gray-50 px-4 py-3 flex items-center justify-between border-t border-gray-200">
              <div className="text-sm text-gray-600">
                Hiển thị {pagedAssignments.length} / {assignments.length} chứng chỉ
              </div>
              <div className="flex items-center gap-2">
                <button onClick={() => setAssignPage(Math.max(1, assignPage - 1))} disabled={assignPage === 1}
                  className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed">← Trước</button>
                <span className="text-sm text-gray-600">Trang {assignPage} / {assignTotalPages}</span>
                <button onClick={() => setAssignPage(Math.min(assignTotalPages, assignPage + 1))} disabled={assignPage === assignTotalPages}
                  className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed">Tiếp →</button>
              </div>
            </div>
          )}
        </>
      )}

      {/* ── TAB 2: Chứng chỉ thuyền viên từ snapshot ── */}
      {activeTab === 'crew' && (
        <>
          <div className="cl-header">
            <div className="cl-header-left">
              <h1 className="cl-title">Chứng chỉ thuyền viên</h1>
              <span className="cl-count-badge">{filteredCrewCerts.length}</span>
            </div>
            <div className="cl-header-right">
              <button className="cl-btn cl-btn--ghost" onClick={fetchCrewCerts} title="Làm mới">
                <Loader2 size={13} className={crewCertsLoading ? 'spin' : ''} />
              </button>
            </div>
          </div>

          {crewCertsLoading ? (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '40px', gap: '10px', color: '#6b7c8f' }}>
              <Loader2 size={20} className="spin" /><span>Đang tải chứng chỉ thuyền viên...</span>
            </div>
          ) : (
            <div className="cl-table-card">
              <table className="cl-table">
                <thead>
                  <tr className="cl-tr-labels">
                    <th style={{ width: '16%' }}>Thuyền viên</th>
                    <th style={{ width: '12%' }}>Mã CC</th>
                    <th style={{ width: '24%' }}>Tên chứng chỉ</th>
                    <th style={{ width: '9%' }}>Loại</th>
                    <th style={{ width: '14%' }}>Số CC</th>
                    <th style={{ width: '10%' }}>Ngày hết hạn</th>
                    <th style={{ width: '7%' }}>Còn lại</th>
                    <th style={{ width: '7%', borderRight: 'none' }}>Trạng thái</th>
                  </tr>
                  {/* Hàng lọc theo từng cột — cùng khuôn với VesselCrewTab */}
                  <tr className="cl-tr-filters">
                    <th>
                      <div className="cl-search-wrap">
                        <input className="cl-cf" placeholder="Tìm kiếm" value={filterCrewName} onChange={e => { setFilterCrewName(e.target.value); setCrewPage(1); }} />
                      </div>
                    </th>
                    <th>
                      <div className="cl-search-wrap">
                        <input className="cl-cf" placeholder="Tìm kiếm" value={filterCertCode} onChange={e => { setFilterCertCode(e.target.value); setCrewPage(1); }} />
                      </div>
                    </th>
                    <th>
                      <div className="cl-search-wrap">
                        <input className="cl-cf" placeholder="Tìm kiếm" value={crewCertSearch} onChange={e => { setCrewCertSearch(e.target.value); setCrewPage(1); }} />
                      </div>
                    </th>
                    <th>
                      <select className="cl-cf" value={crewCertCat} onChange={e => { setCrewCertCat(e.target.value); setCrewPage(1); }}>
                        <option value="">Tất cả</option>
                        {Object.entries(CATEGORY_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
                      </select>
                    </th>
                    <th></th>
                    <th></th>
                    <th></th>
                    <th style={{ borderRight: 'none' }}></th>
                  </tr>
                </thead>
                <tbody>
                  {filteredCrewCerts.length === 0 ? (
                    <tr><td colSpan={8} className="cl-empty">
                      <Users size={24} />
                      <p>{crewCerts.length === 0 ? 'Chưa có dữ liệu chứng chỉ từ snapshot — hãy thực hiện sync từ tàu' : 'Không tìm thấy'}</p>
                    </td></tr>
                  ) : pagedCrewCerts.map((c, idx) => {
                    const st = getCertStatusStyle(c.status);
                    const catCol = CATEGORY_COLORS[c.category || ''] || { bg: '#f1f5f9', color: '#475569' };
                    return (
                      <tr key={c.id} className={`cl-tr${idx % 2 === 1 ? ' cl-tr--alt' : ''}`}>
                        <td style={{ fontWeight: 500, fontSize: '12px' }}>{c.crewMemberName}</td>
                        <td style={{ fontFamily: 'monospace', fontWeight: 600, fontSize: '11.5px' }}>{c.certificateCode}</td>
                        <td style={{ fontSize: '12.5px' }}>{c.certificateName}</td>
                        <td >
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
                        <td style={{ borderRight: 'none' }}>
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
          {crewTotalPages > 1 && (
            <div className="bg-gray-50 px-4 py-3 flex items-center justify-between border-t border-gray-200">
              <div className="text-sm text-gray-600">
                Hiển thị {pagedCrewCerts.length} / {crewCerts.length} chứng chỉ thuyền viên
              </div>
              <div className="flex items-center gap-2">
                <button onClick={() => setCrewPage(Math.max(1, crewPage - 1))} disabled={crewPage === 1}
                  className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed">← Trước</button>
                <span className="text-sm text-gray-600">Trang {crewPage} / {crewTotalPages}</span>
                <button onClick={() => setCrewPage(Math.min(crewTotalPages, crewPage + 1))} disabled={crewPage === crewTotalPages}
                  className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed">Tiếp →</button>
              </div>
            </div>
          )}
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
                  style={{ width: '100%', padding: '7px 10px 7px 32px', border: '1px solid #d6dee8', borderRadius: '5px', fontSize: '12.5px', outline: 'none' }} />
              </div>
            </div>
            <div style={{ maxHeight: '380px', overflowY: 'auto', padding: '0 16px 8px' }}>
              {filteredAvailable.length === 0 ? (
                <p style={{ textAlign: 'center', color: '#6b7c8f', padding: '24px', fontSize: '13px' }}>
                  {available.length === 0 ? 'Tất cả chứng chỉ đã được gán cho tàu này' : 'Không tìm thấy'}
                </p>
              ) : (
                <table className="cl-table" style={{ fontSize: '12px' }}>
                  <thead>
                    <tr className="cl-tr-labels">
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
                <button className="cl-btn cl-btn--primary" disabled={selectedToAdd.size === 0 || saving} onClick={handleAdd}>
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


