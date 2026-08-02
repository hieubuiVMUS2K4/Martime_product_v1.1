import React, { useState, useEffect, useCallback } from 'react';
import { Loader2, Users } from 'lucide-react';
import { ENV } from '../../config/env';
import '../../pages/VesselManagement/VesselsPage.css';
// Dung chung he lop cl-* voi VesselCrewTab de hai tab trong chi tiet tau cung mot khuon.
import '../../pages/MasterDataManagement/Crew/CrewListPage.css';

const BASE = ENV.API_BASE_URL;

interface VesselCertificateTabProps {
  vesselId: string;
  vesselName: string;
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

/**
 * Chứng chỉ của thuyền viên đang ở trên tàu.
 *
 * Không còn phần "gán loại chứng chỉ cho tàu": danh mục loại chứng chỉ do BỜ làm chủ
 * và phát xuống MỌI tàu (giống danh mục vật tư), nên không có khái niệm tàu nào được
 * gán loại nào nữa. Loại chứng chỉ nào bắt buộc với ai được suy ra từ chức danh
 * (rank_certificates) và cờ IsMandatory, đúng như logic tính tuân thủ vẫn dùng.
 */
export function VesselCertificateTab({ vesselId }: VesselCertificateTabProps) {
  const [crewCerts, setCrewCerts] = useState<CrewCert[]>([]);
  const [loading, setLoading] = useState(true);

  /* Bộ lọc theo cột */
  const [crewCertSearch, setCrewCertSearch] = useState('');
  const [crewCertCat, setCrewCertCat] = useState('');
  const [filterCrewName, setFilterCrewName] = useState('');
  const [filterCertCode, setFilterCertCode] = useState('');
  const [crewPage, setCrewPage] = useState(1);

  const fetchCrewCerts = useCallback(async () => {
    setLoading(true);
    try {
      const res = await fetch(`${BASE}/vessels/${vesselId}/certificates/crew`);
      if (res.ok) setCrewCerts(await res.json());
    } catch { /* ignore */ }
    finally { setLoading(false); }
  }, [vesselId]);

  useEffect(() => { fetchCrewCerts(); }, [fetchCrewCerts]);

  const filteredCrewCerts = crewCerts.filter(c =>
    (!crewCertCat || c.category === crewCertCat) &&
    (!filterCrewName || c.crewMemberName.toLowerCase().includes(filterCrewName.toLowerCase())) &&
    (!filterCertCode || c.certificateCode.toLowerCase().includes(filterCertCode.toLowerCase())) &&
    (!crewCertSearch || c.certificateName.toLowerCase().includes(crewCertSearch.toLowerCase()))
  );

  // Phân trang phía client — dữ liệu đã tải hết sẵn nên không cần gọi lại API.
  const PAGE_SIZE = 15;
  const crewTotalPages = Math.max(1, Math.ceil(filteredCrewCerts.length / PAGE_SIZE));
  const pagedCrewCerts = filteredCrewCerts.slice((crewPage - 1) * PAGE_SIZE, crewPage * PAGE_SIZE);

  // Lọc xong mà đang đứng ở trang không còn tồn tại thì kéo về trang cuối hợp lệ,
  // nếu không bảng sẽ trống trơn dù vẫn có kết quả.
  useEffect(() => { if (crewPage > crewTotalPages) setCrewPage(crewTotalPages); }, [crewPage, crewTotalPages]);

  const getCertStatusStyle = (status: string) => {
    if (status === 'EXPIRED') return { bg: '#fef2f2', color: '#dc2626', label: 'Hết hạn' };
    if (status === 'EXPIRING_SOON') return { bg: '#fffbeb', color: '#d97706', label: 'Sắp hết hạn' };
    return { bg: '#f0fdf4', color: '#16a34a', label: 'Còn hiệu lực' };
  };

  return (
    <div className="cl-page relative">
      <div className="cl-header">
        <div className="cl-header-left">
          <Users size={16} className="cl-header-icon" />
          <h1 className="cl-title">Chứng chỉ thuyền viên</h1>
          <span className="cl-count-badge">{filteredCrewCerts.length}</span>
        </div>
        <div className="cl-header-right">
          <button className="cl-btn cl-btn--ghost" onClick={fetchCrewCerts} title="Làm mới">
            <Loader2 size={13} className={loading ? 'spin' : ''} />
          </button>
        </div>
      </div>

      {loading ? (
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
    </div>
  );
}
