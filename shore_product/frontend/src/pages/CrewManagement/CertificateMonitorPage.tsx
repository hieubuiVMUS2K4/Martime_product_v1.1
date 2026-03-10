import React, { useState, useMemo } from 'react';
import {
  ShieldCheck, AlertTriangle, XCircle, CheckCircle2,
  Search, ChevronDown, ChevronUp, RefreshCw, Filter
} from 'lucide-react';
import { useExpiringCertificates, useCompliance } from '../../hooks/useCrew';
import { useNavigate } from 'react-router-dom';
import './CertificateMonitorPage.css';

type SortField = 'name' | 'cert' | 'expiry' | 'status';
type SortDir = 'asc' | 'desc';
type StatusFilter = 'all' | 'VALID' | 'EXPIRING_SOON' | 'EXPIRED';

export const CertificateMonitorPage: React.FC = () => {
  const [daysAhead, setDaysAhead] = useState(90);
  const { data: expiring, loading: expLoading, refetch: refetchExp } = useExpiringCertificates(daysAhead);
  const { data: compliance, refetch: refetchComp } = useCompliance();
  const navigate = useNavigate();

  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('all');
  const [sortField, setSortField] = useState<SortField>('expiry');
  const [sortDir, setSortDir] = useState<SortDir>('asc');
  const [showFilters, setShowFilters] = useState(false);

  // KPI calculations
  const kpi = useMemo(() => {
    if (!expiring.length) return { valid: 0, expiringSoon: 0, expired: 0, total: 0 };
    const valid = expiring.filter(c => c.status === 'VALID').length;
    const expSoon = expiring.filter(c => c.status === 'EXPIRING_SOON').length;
    const expired = expiring.filter(c => c.status === 'EXPIRED').length;
    return { valid, expiringSoon: expSoon, expired, total: expiring.length };
  }, [expiring]);

  // Filter + sort
  const filteredCerts = useMemo(() => {
    let list = [...expiring];
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter(c =>
        (c.certificateName || '').toLowerCase().includes(q) ||
        (c.certificateCode || '').toLowerCase().includes(q) ||
        (c.certificateNumber || '').toLowerCase().includes(q) ||
        (c.crewMemberName || '').toLowerCase().includes(q)
      );
    }
    if (statusFilter !== 'all') {
      list = list.filter(c => c.status === statusFilter);
    }
    list.sort((a, b) => {
      let cmp = 0;
      switch (sortField) {
        case 'name': cmp = (a.crewMemberName || '').localeCompare(b.crewMemberName || ''); break;
        case 'cert': cmp = (a.certificateName || '').localeCompare(b.certificateName || ''); break;
        case 'expiry': cmp = new Date(a.expiryDate || 0).getTime() - new Date(b.expiryDate || 0).getTime(); break;
        case 'status': {
          const order: Record<string, number> = { EXPIRED: 0, EXPIRING_SOON: 1, VALID: 2 };
          cmp = (order[a.status || ''] ?? 3) - (order[b.status || ''] ?? 3);
          break;
        }
      }
      return sortDir === 'asc' ? cmp : -cmp;
    });
    return list;
  }, [expiring, search, statusFilter, sortField, sortDir]);

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDir(d => d === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDir('asc');
    }
  };

  const SortIcon = ({ field }: { field: SortField }) => {
    if (sortField !== field) return <ChevronDown size={12} style={{ opacity: 0.2 }} />;
    return sortDir === 'asc' ? <ChevronUp size={12} /> : <ChevronDown size={12} />;
  };

  const formatDate = (d?: string) => d ? new Date(d).toLocaleDateString('vi-VN') : '—';
  const getStatusClass = (s?: string) => {
    if (s === 'VALID') return 'mon-valid';
    if (s === 'EXPIRING_SOON') return 'mon-expiring';
    if (s === 'EXPIRED') return 'mon-expired';
    return '';
  };
  const getStatusLabel = (s?: string) => {
    if (s === 'VALID') return 'Hiệu lực';
    if (s === 'EXPIRING_SOON') return 'Sắp hết hạn';
    if (s === 'EXPIRED') return 'Hết hạn';
    return s || '—';
  };

  const refreshAll = () => { refetchExp(); refetchComp(); };

  return (
    <div className="cert-monitor-page fade-in">
      {/* Header */}
      <div className="cm-header">
        <div>
          <h1 className="cm-title">Giám sát chứng chỉ</h1>
          <p className="cm-subtitle">Theo dõi hiệu lực chứng chỉ của toàn đội thuyền viên</p>
        </div>
        <div className="cm-header-actions">
          <select
            className="cm-days-select"
            value={daysAhead}
            onChange={e => setDaysAhead(Number(e.target.value))}
          >
            <option value={30}>30 ngày tới</option>
            <option value={60}>60 ngày tới</option>
            <option value={90}>90 ngày tới</option>
            <option value={180}>180 ngày tới</option>
            <option value={365}>1 năm tới</option>
          </select>
          <button className="cm-refresh-btn" onClick={refreshAll} title="Làm mới">
            <RefreshCw size={15} />
          </button>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="cm-kpi-grid">
        <div className="kpi-card kpi-total">
          <ShieldCheck size={22} />
          <div className="kpi-body">
            <span className="kpi-value">{kpi.total}</span>
            <span className="kpi-label">Tổng chứng chỉ</span>
          </div>
        </div>
        <div className="kpi-card kpi-valid" onClick={() => setStatusFilter(statusFilter === 'VALID' ? 'all' : 'VALID')}>
          <CheckCircle2 size={22} />
          <div className="kpi-body">
            <span className="kpi-value">{kpi.valid}</span>
            <span className="kpi-label">Còn hiệu lực</span>
          </div>
        </div>
        <div className="kpi-card kpi-warning" onClick={() => setStatusFilter(statusFilter === 'EXPIRING_SOON' ? 'all' : 'EXPIRING_SOON')}>
          <AlertTriangle size={22} />
          <div className="kpi-body">
            <span className="kpi-value">{kpi.expiringSoon}</span>
            <span className="kpi-label">Sắp hết hạn</span>
          </div>
        </div>
        <div className="kpi-card kpi-danger" onClick={() => setStatusFilter(statusFilter === 'EXPIRED' ? 'all' : 'EXPIRED')}>
          <XCircle size={22} />
          <div className="kpi-body">
            <span className="kpi-value">{kpi.expired}</span>
            <span className="kpi-label">Đã hết hạn</span>
          </div>
        </div>
      </div>

      {/* Search & Filter bar */}
      <div className="cm-toolbar">
        <div className="cm-search-wrap">
          <Search size={16} className="cm-search-icon" />
          <input
            type="text"
            className="cm-search"
            placeholder="Tìm chứng chỉ, thuyền viên..."
            value={search}
            onChange={e => setSearch(e.target.value)}
          />
          {search && (
            <button className="cm-search-clear" onClick={() => setSearch('')}>×</button>
          )}
        </div>
        <button
          className={`cm-filter-toggle ${showFilters ? 'active' : ''}`}
          onClick={() => setShowFilters(!showFilters)}
        >
          <Filter size={14} /> Lọc
        </button>
      </div>

      {showFilters && (
        <div className="cm-filter-bar">
          <label className="cm-filter-label">
            Trạng thái
            <select value={statusFilter} onChange={e => setStatusFilter(e.target.value as StatusFilter)}>
              <option value="all">Tất cả</option>
              <option value="VALID">Còn hiệu lực</option>
              <option value="EXPIRING_SOON">Sắp hết hạn</option>
              <option value="EXPIRED">Hết hạn</option>
            </select>
          </label>
        </div>
      )}

      {/* Table */}
      <div className="cm-table-wrap">
        {expLoading ? (
          <div className="cm-loading">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="skeleton" style={{ height: 44, borderRadius: 6 }} />
            ))}
          </div>
        ) : filteredCerts.length === 0 ? (
          <div className="cm-empty">
            <ShieldCheck size={44} />
            <p>{search ? 'Không tìm thấy chứng chỉ phù hợp' : 'Không có chứng chỉ nào trong khoảng thời gian này'}</p>
          </div>
        ) : (
          <table className="cm-table">
            <thead>
              <tr>
                <th onClick={() => handleSort('name')}>
                  Thuyền viên <SortIcon field="name" />
                </th>
                <th onClick={() => handleSort('cert')}>
                  Chứng chỉ <SortIcon field="cert" />
                </th>
                <th>Số chứng chỉ</th>
                <th>Ngày cấp</th>
                <th onClick={() => handleSort('expiry')}>
                  Ngày hết hạn <SortIcon field="expiry" />
                </th>
                <th>Còn lại</th>
                <th onClick={() => handleSort('status')}>
                  Trạng thái <SortIcon field="status" />
                </th>
              </tr>
            </thead>
            <tbody>
              {filteredCerts.map(cert => (
                <tr
                  key={cert.id}
                  className={`cm-row ${getStatusClass(cert.status)}`}
                  onClick={() => cert.crewMemberId && navigate(`/crew/${cert.crewMemberId}`)}
                >
                  <td className="cm-crew-name">{cert.crewMemberName || '—'}</td>
                  <td>
                    <span className="cm-cert-name">{cert.certificateName || cert.certificateCode}</span>
                    {cert.category && <span className="cm-cert-cat">{cert.category}</span>}
                  </td>
                  <td className="cm-cert-num">{cert.certificateNumber || '—'}</td>
                  <td>{formatDate(cert.issueDate)}</td>
                  <td className="cm-expiry">{formatDate(cert.expiryDate)}</td>
                  <td>
                    {cert.daysUntilExpiry !== undefined ? (
                      <span className={`cm-days ${cert.daysUntilExpiry <= 0 ? 'text-danger' : cert.daysUntilExpiry <= 30 ? 'text-warning' : ''}`}>
                        {cert.daysUntilExpiry <= 0 ? 'Quá hạn' : `${cert.daysUntilExpiry} ngày`}
                      </span>
                    ) : '—'}
                  </td>
                  <td>
                    <span className={`cm-status-badge ${getStatusClass(cert.status)}`}>
                      {getStatusLabel(cert.status)}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Compliance Summary */}
      {compliance.length > 0 && (
        <div className="cm-compliance">
          <h3 className="cm-compliance-title">
            <ShieldCheck size={18} /> Tuân thủ tổng quan
          </h3>
          <div className="cm-compliance-grid">
            <div className="comp-item">
              <span className="comp-value">{compliance.length}</span>
              <span className="comp-label">Thuyền viên</span>
            </div>
            <div className="comp-item">
              <span className="comp-value">{compliance.reduce((s, c) => s + c.totalHeld, 0)}</span>
              <span className="comp-label">Chứng chỉ</span>
            </div>
            <div className="comp-item comp-green">
              <span className="comp-value">{compliance.filter(c => c.compliancePercentage === 100).length}</span>
              <span className="comp-label">Đủ CC</span>
            </div>
            <div className="comp-item comp-yellow">
              <span className="comp-value">{compliance.reduce((s, c) => s + c.expiringCertificates.length, 0)}</span>
              <span className="comp-label">Sắp hết hạn</span>
            </div>
            <div className="comp-item comp-red">
              <span className="comp-value">{compliance.reduce((s, c) => s + c.missingCertificates.length, 0)}</span>
              <span className="comp-label">Thiếu CC</span>
            </div>
            <div className="comp-item">
              <span className="comp-value">
                {compliance.length > 0
                  ? `${Math.round(compliance.reduce((s, c) => s + c.compliancePercentage, 0) / compliance.length)}%`
                  : '—'}
              </span>
              <span className="comp-label">Tỉ lệ tuân thủ TB</span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
