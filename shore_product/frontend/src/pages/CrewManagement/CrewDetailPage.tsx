import React, { useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft, User, ShieldCheck, FileText, Ship,
  Calendar, MapPin, Phone, Mail, AlertTriangle,
  Pencil, Clock, CheckCircle2, XCircle, Plus
} from 'lucide-react';
import { useCrewDetail, useCrewCertificates, useReferenceData } from '../../hooks/useCrew';
import { crewApi, certificateApi } from '../../services/crew.service';
import type { CrewDocument, ServiceRecord, TabKey } from '../../types/crew.types';
import './CrewDetailPage.css';

const tabs: { key: TabKey; label: string; icon: React.ReactNode }[] = [
  { key: 'personal', label: 'Thông tin', icon: <User size={16} /> },
  { key: 'certificates', label: 'Chứng chỉ', icon: <ShieldCheck size={16} /> },
  { key: 'documents', label: 'Giấy tờ', icon: <FileText size={16} /> },
  { key: 'service-history', label: 'Lịch sử tàu', icon: <Ship size={16} /> },
];

export const CrewDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: crew, loading, error, refetch } = useCrewDetail(id);
  const { data: certificates, loading: certsLoading } = useCrewCertificates(id);
  const [activeTab, setActiveTab] = useState<TabKey>('personal');
  const [documents, setDocuments] = useState<CrewDocument[]>([]);
  const [serviceRecords, setServiceRecords] = useState<ServiceRecord[]>([]);
  const [docsLoading, setDocsLoading] = useState(false);
  const [recordsLoading, setRecordsLoading] = useState(false);

  // Lazy load documents
  const loadDocuments = useCallback(async () => {
    if (!id || documents.length > 0) return;
    setDocsLoading(true);
    try {
      const [travel, seafarer, employment, health] = await Promise.all([
        crewApi.getDocuments(id, 'travel'),
        crewApi.getDocuments(id, 'seafarer'),
        crewApi.getDocuments(id, 'employment'),
        crewApi.getDocuments(id, 'health'),
      ]);
      setDocuments([...travel, ...seafarer, ...employment, ...health]);
    } catch { /* ignore */ } finally { setDocsLoading(false); }
  }, [id, documents.length]);

  // Lazy load service records
  const loadServiceRecords = useCallback(async () => {
    if (!id || serviceRecords.length > 0) return;
    setRecordsLoading(true);
    try {
      setServiceRecords(await crewApi.getServiceRecords(id));
    } catch { /* ignore */ } finally { setRecordsLoading(false); }
  }, [id, serviceRecords.length]);

  const handleTabChange = useCallback((tab: TabKey) => {
    setActiveTab(tab);
    if (tab === 'documents') loadDocuments();
    if (tab === 'service-history') loadServiceRecords();
  }, [loadDocuments, loadServiceRecords]);

  const formatDate = (d?: string) => d ? new Date(d).toLocaleDateString('vi-VN') : '—';
  const getAge = (d?: string) => {
    if (!d) return '';
    const years = Math.floor((Date.now() - new Date(d).getTime()) / (365.25 * 86400000));
    return `(${years} tuổi)`;
  };

  const getCertStatusClass = (status?: string) => {
    switch (status) {
      case 'VALID': return 'cert-valid';
      case 'EXPIRING_SOON': return 'cert-expiring';
      case 'EXPIRED': return 'cert-expired';
      default: return 'cert-unknown';
    }
  };

  const getCertStatusLabel = (status?: string) => {
    switch (status) {
      case 'VALID': return 'Còn hiệu lực';
      case 'EXPIRING_SOON': return 'Sắp hết hạn';
      case 'EXPIRED': return 'Hết hạn';
      case 'SUSPENDED': return 'Tạm dừng';
      default: return status || '—';
    }
  };

  const getAvatarColor = (name: string) => {
    let hash = 0;
    for (let i = 0; i < name.length; i++) hash = (hash * 31 + name.charCodeAt(i)) | 0;
    const colors = ['#1e40af', '#7c3aed', '#059669', '#d97706', '#dc2626', '#0891b2'];
    return colors[Math.abs(hash) % colors.length];
  };

  const getInitials = (name: string) => {
    const parts = name.split(' ').filter(Boolean);
    return parts.length >= 2
      ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
      : name.substring(0, 2).toUpperCase();
  };

  if (loading) {
    return (
      <div className="crew-detail-page fade-in">
        <div className="detail-skeleton">
          <div className="skeleton" style={{ width: 80, height: 80, borderRadius: '50%' }} />
          <div className="skeleton" style={{ width: '40%', height: 24 }} />
          <div className="skeleton" style={{ width: '25%', height: 16 }} />
          <div className="skeleton" style={{ width: '100%', height: 300, borderRadius: 12, marginTop: 24 }} />
        </div>
      </div>
    );
  }

  if (error || !crew) {
    return (
      <div className="crew-detail-page fade-in">
        <div className="detail-error">
          <XCircle size={48} />
          <p>{error || 'Không tìm thấy thuyền viên'}</p>
          <button className="btn btn-primary btn-sm" onClick={() => navigate('/crew')}>Quay lại</button>
        </div>
      </div>
    );
  }

  return (
    <div className="crew-detail-page fade-in">
      {/* Back button */}
      <button className="back-btn" onClick={() => navigate('/crew')}>
        <ArrowLeft size={16} />
        <span>Danh sách thuyền viên</span>
      </button>

      {/* ===== Profile Header ===== */}
      <div className="profile-header">
        <div className="profile-left">
          <div className="profile-avatar" style={{ backgroundColor: getAvatarColor(crew.fullName) }}>
            {crew.avatarUrl ? (
              <img src={crew.avatarUrl} alt="" />
            ) : (
              getInitials(crew.fullName)
            )}
          </div>
          <div className="profile-info">
            <h1 className="profile-name">{crew.fullName}</h1>
            <div className="profile-meta">
              <span className="profile-rank">{crew.rankName || 'Chưa gán chức danh'}</span>
              <span className="profile-separator">·</span>
              <span className="profile-code">{crew.crewId}</span>
              {crew.department && (
                <>
                  <span className="profile-separator">·</span>
                  <span>{crew.department}</span>
                </>
              )}
            </div>
            <div className="profile-tags">
              <span className={`status-pill ${crew.isOnboard ? 'status-pill--onboard' : 'status-pill--pool'}`}>
                <span className="status-dot" />
                {crew.isOnboard ? 'Đang trên tàu' : 'Ở bờ'}
              </span>
              {crew.isSynced === false && (
                <span className="sync-badge">
                  <Clock size={12} /> Chưa đồng bộ
                </span>
              )}
            </div>
          </div>
        </div>
        <div className="profile-right">
          <button className="btn btn-sm" onClick={() => navigate(`/crew?edit=${crew.id}`)}>
            <Pencil size={14} /> Chỉnh sửa
          </button>
        </div>
      </div>

      {/* ===== Quick Info Cards ===== */}
      <div className="quick-info">
        {crew.dateOfBirth && (
          <div className="qi-item">
            <Calendar size={15} />
            <span>{formatDate(crew.dateOfBirth)} {getAge(crew.dateOfBirth)}</span>
          </div>
        )}
        {crew.nationality && (
          <div className="qi-item">
            <MapPin size={15} />
            <span>{crew.nationality}</span>
          </div>
        )}
        {crew.phoneNumber && (
          <div className="qi-item">
            <Phone size={15} />
            <span>{crew.phoneNumber}</span>
          </div>
        )}
        {crew.emailAddress && (
          <div className="qi-item">
            <Mail size={15} />
            <span>{crew.emailAddress}</span>
          </div>
        )}
        {crew.passportNumber && (
          <div className="qi-item">
            <FileText size={15} />
            <span>HC: {crew.passportNumber} {crew.passportExpiry ? `(đến ${formatDate(crew.passportExpiry)})` : ''}</span>
          </div>
        )}
      </div>

      {/* ===== Tab Navigation ===== */}
      <div className="detail-tabs">
        {tabs.map(t => (
          <button
            key={t.key}
            className={`detail-tab ${activeTab === t.key ? 'detail-tab--active' : ''}`}
            onClick={() => handleTabChange(t.key)}
          >
            {t.icon}
            <span>{t.label}</span>
            {t.key === 'certificates' && certificates.length > 0 && (
              <span className="tab-count">{certificates.length}</span>
            )}
          </button>
        ))}
      </div>

      {/* ===== Tab Content ===== */}
      <div className="tab-content fade-in">
        {/* --- Personal Info --- */}
        {activeTab === 'personal' && (
          <div className="info-sections">
            <div className="info-section">
              <h3 className="info-section-title">Thông tin cá nhân</h3>
              <div className="info-grid">
                <InfoRow label="Họ và tên" value={crew.fullName} />
                <InfoRow label="Mã TV" value={crew.crewId} />
                <InfoRow label="Ngày sinh" value={formatDate(crew.dateOfBirth)} />
                <InfoRow label="Nơi sinh" value={crew.placeOfBirth} />
                <InfoRow label="Quốc tịch" value={crew.nationality} />
                <InfoRow label="CMND/CCCD" value={crew.idCardNumber} />
                <InfoRow label="Tình trạng HN" value={crew.maritalStatus} />
                <InfoRow label="Nhóm máu" value={crew.bloodGroup} />
              </div>
            </div>

            <div className="info-section">
              <h3 className="info-section-title">Thể chất</h3>
              <div className="info-grid">
                <InfoRow label="Chiều cao" value={crew.height ? `${crew.height} cm` : undefined} />
                <InfoRow label="Cân nặng" value={crew.weight ? `${crew.weight} kg` : undefined} />
                <InfoRow label="Cỡ giày" value={crew.shoeSize} />
                <InfoRow label="Cỡ áo" value={crew.clothingSize} />
                <InfoRow label="Hút thuốc" value={crew.isSmoker === true ? 'Có' : crew.isSmoker === false ? 'Không' : undefined} />
                <InfoRow label="Tiêm COVID" value={crew.isCovidVaccinated === true ? 'Có' : crew.isCovidVaccinated === false ? 'Không' : undefined} />
              </div>
            </div>

            <div className="info-section">
              <h3 className="info-section-title">Hợp đồng & Tàu</h3>
              <div className="info-grid">
                <InfoRow label="Ngày gia nhập" value={formatDate(crew.joinDate)} />
                <InfoRow label="Ngày lên tàu" value={formatDate(crew.embarkDate)} />
                <InfoRow label="Ngày xuống tàu" value={formatDate(crew.disembarkDate)} />
                <InfoRow label="HĐ hết hạn" value={formatDate(crew.contractEnd)} />
              </div>
            </div>

            {(crew.nextOfKinName || crew.emergencyContact) && (
              <div className="info-section">
                <h3 className="info-section-title">Người thân & Khẩn cấp</h3>
                <div className="info-grid">
                  <InfoRow label="Tên người thân" value={crew.nextOfKinName} />
                  <InfoRow label="Quan hệ" value={crew.nextOfKinRelation} />
                  <InfoRow label="SĐT người thân" value={crew.nextOfKinPhone} />
                  <InfoRow label="Địa chỉ" value={crew.nextOfKinAddress} />
                  <InfoRow label="Liên hệ KN" value={crew.emergencyContact} />
                </div>
              </div>
            )}

            {crew.educationInstitution && (
              <div className="info-section">
                <h3 className="info-section-title">Học vấn</h3>
                <div className="info-grid">
                  <InfoRow label="Trường" value={crew.educationInstitution} />
                  <InfoRow label="Chuyên ngành" value={crew.educationCourse} />
                  <InfoRow label="Số năm" value={crew.educationPeriodYears?.toString()} />
                  <InfoRow label="Năm TN" value={crew.educationGraduationYear?.toString()} />
                </div>
              </div>
            )}
          </div>
        )}

        {/* --- Certificates --- */}
        {activeTab === 'certificates' && (
          <div className="cert-section">
            {certsLoading ? (
              <div className="tab-loading">
                {[...Array(4)].map((_, i) => (
                  <div key={i} className="skeleton" style={{ height: 60, borderRadius: 8 }} />
                ))}
              </div>
            ) : certificates.length === 0 ? (
              <div className="tab-empty">
                <ShieldCheck size={40} />
                <p>Chưa có chứng chỉ nào</p>
              </div>
            ) : (
              <div className="cert-list">
                {certificates.map(cert => (
                  <div key={cert.id} className={`cert-card ${getCertStatusClass(cert.status)}`}>
                    <div className="cert-card-left">
                      <span className="cert-name">{cert.certificateName || cert.certificateCode}</span>
                      <span className="cert-meta">
                        {cert.certificateNumber && <span>#{cert.certificateNumber}</span>}
                        {cert.category && <span className="cert-category">{cert.category}</span>}
                      </span>
                    </div>
                    <div className="cert-card-center">
                      <span className="cert-dates">
                        {formatDate(cert.issueDate)} → {formatDate(cert.expiryDate)}
                      </span>
                      {cert.issuingAuthority && (
                        <span className="cert-authority">{cert.issuingAuthority}</span>
                      )}
                    </div>
                    <div className="cert-card-right">
                      <span className={`cert-status-badge ${getCertStatusClass(cert.status)}`}>
                        {cert.status === 'VALID' && <CheckCircle2 size={14} />}
                        {cert.status === 'EXPIRING_SOON' && <AlertTriangle size={14} />}
                        {cert.status === 'EXPIRED' && <XCircle size={14} />}
                        {getCertStatusLabel(cert.status)}
                      </span>
                      {cert.daysUntilExpiry !== undefined && cert.daysUntilExpiry <= 90 && cert.daysUntilExpiry > 0 && (
                        <span className="cert-days-left">{cert.daysUntilExpiry} ngày</span>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* --- Documents --- */}
        {activeTab === 'documents' && (
          <div className="docs-section">
            {docsLoading ? (
              <div className="tab-loading">
                {[...Array(3)].map((_, i) => (
                  <div key={i} className="skeleton" style={{ height: 50, borderRadius: 8 }} />
                ))}
              </div>
            ) : documents.length === 0 ? (
              <div className="tab-empty">
                <FileText size={40} />
                <p>Chưa có giấy tờ nào</p>
              </div>
            ) : (
              <div className="doc-list">
                {['travel', 'seafarer', 'employment', 'health'].map(cat => {
                  const catDocs = documents.filter(d => d.category === cat);
                  if (catDocs.length === 0) return null;
                  const catLabel: Record<string, string> = {
                    travel: 'Hộ chiếu & Giấy đi đường',
                    seafarer: 'Sổ thuyền viên',
                    employment: 'Hợp đồng lao động',
                    health: 'Sức khỏe',
                  };
                  return (
                    <div key={cat} className="doc-category">
                      <h4 className="doc-cat-title">{catLabel[cat] || cat}</h4>
                      {catDocs.map(doc => (
                        <div key={doc.id} className="doc-row">
                          <FileText size={16} className="doc-icon" />
                          <div className="doc-info">
                            <span className="doc-type">{doc.documentType}</span>
                            {doc.documentNumber && <span className="doc-num">#{doc.documentNumber}</span>}
                          </div>
                          <span className="doc-dates">{formatDate(doc.issueDate)} → {formatDate(doc.expiryDate)}</span>
                        </div>
                      ))}
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        )}

        {/* --- Service History --- */}
        {activeTab === 'service-history' && (
          <div className="records-section">
            {recordsLoading ? (
              <div className="tab-loading">
                {[...Array(3)].map((_, i) => (
                  <div key={i} className="skeleton" style={{ height: 50, borderRadius: 8 }} />
                ))}
              </div>
            ) : serviceRecords.length === 0 ? (
              <div className="tab-empty">
                <Ship size={40} />
                <p>Chưa có lịch sử phục vụ</p>
              </div>
            ) : (
              <div className="service-timeline">
                {serviceRecords.map((rec, idx) => (
                  <div key={rec.id} className="timeline-item">
                    <div className="timeline-dot" />
                    {idx < serviceRecords.length - 1 && <div className="timeline-line" />}
                    <div className="timeline-content">
                      <div className="timeline-header">
                        <span className="timeline-vessel">{rec.vesselName}</span>
                        {rec.vesselIMO && <span className="timeline-imo">IMO {rec.vesselIMO}</span>}
                      </div>
                      <div className="timeline-meta">
                        <span>{rec.rankDuringService || '—'}</span>
                        <span className="timeline-sep">·</span>
                        <span>{formatDate(rec.signOnDate)} — {formatDate(rec.signOffDate)}</span>
                      </div>
                      {rec.tradingArea && <span className="timeline-area">{rec.tradingArea}</span>}
                      {rec.remarks && <p className="timeline-remarks">{rec.remarks}</p>}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

// Simple Info Row component
const InfoRow: React.FC<{ label: string; value?: string }> = ({ label, value }) => (
  <div className="info-row">
    <span className="info-label">{label}</span>
    <span className="info-value">{value || '—'}</span>
  </div>
);
