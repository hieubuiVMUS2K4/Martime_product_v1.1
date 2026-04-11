import React, { useState, useCallback, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import './CrewDetailPage.css';
import {
  ArrowLeft, Upload, CheckCircle, XCircle, AlertTriangle,
  Ship, MapPin, Calendar, Eye, ClipboardList, FileCheck, History, ScrollText, Plus, Pencil, Trash2,
} from 'lucide-react';
import { useCrewDetail, useCrewCertificates } from '../../hooks/useCrew';
import { useCrewOnboarding, useCrewDocumentSubmissions, useCrewStatusHistory, useCrewAuditLog } from '../../hooks/useCrewManagement';
import { crewApi, certificateApi, referenceApi } from '../../services/crew.service';
import { useToast } from '../../components/common/Toast';
import type { CrewDocument, ServiceRecord, Rank, Country, CrewCertificate } from '../../types/crew.types';
import type { UpdateCrewRequest } from '../../types/crew.types';
import { AddCrewCertificateModal } from './AddCrewCertificateModal';
import { AddDocumentModal } from './AddDocumentModal';
import { AddHealthDocumentModal } from './AddHealthDocumentModal';
import ImageViewerModal from '../../components/common/ImageViewerModal';
import ProtectedImage from '../../components/common/ProtectedImage';

type TabType = 'basic-data' | 'documents' | 'voyage-history' | 'onboarding' | 'doc-workflow' | 'status-history' | 'audit';

const fmt = (d?: string) => d ? new Date(d).toLocaleDateString('en-GB') : '�';

const calcAge = (dob?: string) => {
  if (!dob) return '';
  const diff = Date.now() - new Date(dob).getTime();
  return String(Math.floor(diff / (365.25 * 86400000)));
};

export const CrewDetailPage: React.FC = () => {
  const { id, vesselId } = useParams<{ id: string; vesselId?: string }>();
  const navigate = useNavigate();
  const { data: crew, loading, error, refetch } = useCrewDetail(id);
  const { data: certificates, setData: setCertificates, loading: certsLoading, refetch: refetchCerts } = useCrewCertificates(id);
  const toast = useToast();

  const [activeTab, setActiveTab] = useState<TabType>('basic-data');
  const [edited, setEdited] = useState<UpdateCrewRequest>({});
  const [saving, setSaving] = useState(false);
  const [ranks, setRanks] = useState<Rank[]>([]);
  const [countries, setCountries] = useState<Country[]>([]);

  // Edge changes tracking
  const edgeChanges: { field: string; oldValue: string; newValue: string; changedAt: string }[] = React.useMemo(() => {
    if (!crew?.edgeChanges) return [];
    try { return JSON.parse(crew.edgeChanges); } catch { return []; }
  }, [crew?.edgeChanges]);

  const changedFields = React.useMemo(() => new Set(edgeChanges.map(c => c.field)), [edgeChanges]);
  const changeMap = React.useMemo(() => {
    const m: Record<string, { oldValue: string; newValue: string }> = {};
    for (const c of edgeChanges) m[c.field] = { oldValue: c.oldValue, newValue: c.newValue };
    return m;
  }, [edgeChanges]);
  const hasUnviewedChanges = edgeChanges.length > 0;

  // Map fields to sections for tab badge counts
  const SECTION_FIELDS: Record<string, string[]> = {
    'basic-data': [
      'fullName', 'phoneNumber', 'emailAddress', 'department', 'address',
      'placeOfBirth', 'idCardNumber', 'maritalStatus', 'notes', 'dateOfBirth',
      'rankId', 'countryId', 'height', 'weight', 'bloodGroup', 'clothingSize',
      'shoeSize', 'cateringSize', 'isSmoker', 'isCovidVaccinated',
      'joinDate', 'embarkDate', 'disembarkDate', 'contractEnd', 'isOnboard',
      'nextOfKinName', 'nextOfKinRelation', 'nextOfKinPhone', 'nextOfKinAddress',
      'educationInstitution', 'educationCourse', 'educationPeriodYears', 'educationGraduationYear',
    ],
  };

  const getTabChangeCount = (tabKey: string) => {
    if (!hasUnviewedChanges) return 0;
    const fields = SECTION_FIELDS[tabKey];
    if (!fields) return 0;
    return edgeChanges.filter(c => fields.includes(c.field)).length;
  };

  const handleMarkViewed = async () => {
    if (!id) return;
    try {
      await crewApi.markChangesViewed(id);
      await refetch();
    } catch { /* ignore */ }
  };

  // Helper: inline style for changed fields (d�ng inline style d? ch?c ch?n hi?n d?)
  const fieldHighlight = (fieldName: string) =>
    changedFields.has(fieldName) ? ' cd-field--changed' : '';

  const fieldStyle = (fieldName: string): React.CSSProperties =>
    changedFields.has(fieldName)
      ? { borderColor: '#ef4444', background: '#fef2f2', boxShadow: '0 0 0 2px rgba(239,68,68,0.2)' }
      : {};

  // Helper: render change indicator next to a field
  const changeIndicator = (fieldName: string) => {
    const c = changeMap[fieldName];
    if (!c) return null;
    return (
      <div style={{ display: 'flex', alignItems: 'center', gap: 5, marginTop: 2 }}>
        <span style={{ width: 7, height: 7, borderRadius: '50%', background: '#ef4444', flexShrink: 0 }} />
        <span style={{ fontSize: 11, color: '#dc2626' }}>
          T�u d� s?a: <s style={{ color: '#9ca3af' }}>{c.oldValue || '(tr?ng)'}</s> ? <strong style={{ color: '#b91c1c' }}>{c.newValue}</strong>
        </span>
      </div>
    );
  };

  // Certificate management
  const [showAddCertModal, setShowAddCertModal] = useState(false);
  const [editingCert, setEditingCert] = useState<CrewCertificate | null>(null);
  const [uploadingCertId, setUploadingCertId] = useState<number | null>(null);
  const [isImageViewerOpen, setIsImageViewerOpen] = useState(false);
  const [imageViewerUrl, setImageViewerUrl] = useState<string | null>(null);
  const [imageViewerCertId, setImageViewerCertId] = useState<number | null>(null);
  const [imageViewerDocTarget, setImageViewerDocTarget] = useState<{ docId: string; category: string } | null>(null);

  // Avatar upload
  const [pendingAvatarFile, setPendingAvatarFile] = useState<File | null>(null);
  const [pendingAvatarPreview, setPendingAvatarPreview] = useState<string | null>(null);
  const [uploadingAvatar, setUploadingAvatar] = useState(false);

  // Documents
  const [travelDocs, setTravelDocs] = useState<CrewDocument[]>([]);
  const [seafarerDocs, setSeafarerDocs] = useState<CrewDocument[]>([]);
  const [employmentDocs, setEmploymentDocs] = useState<CrewDocument[]>([]);
  const [healthDocs, setHealthDocs] = useState<CrewDocument[]>([]);
  const [docsLoading, setDocsLoading] = useState(false);
  const [isAddDocModalOpen, setIsAddDocModalOpen] = useState(false);
  const [isAddHealthDocModalOpen, setIsAddHealthDocModalOpen] = useState(false);
  const [docFileUploadTarget, setDocFileUploadTarget] = useState<{ docId: string; category: string } | null>(null);
  const [uploadingDocFile, setUploadingDocFile] = useState(false);
  const docFileInputRef = React.useRef<HTMLInputElement>(null);

  // Service records
  const [serviceRecords, setServiceRecords] = useState<ServiceRecord[]>([]);
  const [recordsLoading, setRecordsLoading] = useState(false);

  // Crew management workflow hooks (lazy � only fetch when tab is active)
  const { data: onboardingCase, loading: onbLoading } = useCrewOnboarding(
    activeTab === 'onboarding' ? id : undefined
  );
  const { data: docSubmissions, loading: docSubLoading } = useCrewDocumentSubmissions(
    activeTab === 'doc-workflow' ? id : undefined
  );
  const { data: statusHistory, loading: statusHistLoading } = useCrewStatusHistory(
    activeTab === 'status-history' ? id : undefined
  );
  const { data: auditLogs, loading: auditLoading } = useCrewAuditLog(
    activeTab === 'audit' ? id : undefined
  );

  useEffect(() => {
    if (crew) setEdited(crew);
  }, [crew]);

  useEffect(() => {
    referenceApi.getRanks().then(setRanks).catch(() => {});
    referenceApi.getCountries().then(setCountries).catch(() => {});
  }, []);

  const loadDocuments = useCallback(async (force = false) => {
    if (!id || (!force && travelDocs.length > 0)) return;
    setDocsLoading(true);
    try {
      const [t, s, e, h] = await Promise.all([
        crewApi.getDocuments(id, 'travel'),
        crewApi.getDocuments(id, 'seafarer'),
        crewApi.getDocuments(id, 'employment'),
        crewApi.getDocuments(id, 'health'),
      ]);
      setTravelDocs(t); setSeafarerDocs(s); setEmploymentDocs(e); setHealthDocs(h);
    } catch { /* ignore */ } finally { setDocsLoading(false); }
  }, [id, travelDocs.length]);

  const loadServiceRecords = useCallback(async () => {
    if (!id || serviceRecords.length > 0) return;
    setRecordsLoading(true);
    try {
      setServiceRecords(await crewApi.getServiceRecords(id));
    } catch { /* ignore */ } finally { setRecordsLoading(false); }
  }, [id, serviceRecords.length]);

  const handleTabChange = (tab: TabType) => {
    setActiveTab(tab);
    if (tab === 'documents') loadDocuments();
    if (tab === 'voyage-history') loadServiceRecords();
  };

  const set = (key: keyof UpdateCrewRequest, value: UpdateCrewRequest[keyof UpdateCrewRequest]) =>
    setEdited(prev => ({ ...prev, [key]: value }));

  const handleSave = async () => {
    if (!id) return;
    setSaving(true);
    try {
      await crewApi.update(id, edited);
      await refetch();
      toast.success('Luu th�nh c�ng!');
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Kh�ng th? luu d? li?u');
    } finally { setSaving(false); }
  };

  // Avatar handlers
  const handleAvatarChoose = () => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/jpeg,image/jpg,image/png,image/gif';
    input.onchange = (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (!file) return;
      if (file.size > 5 * 1024 * 1024) { toast.error('?nh kh�ng du?c vu?t qu� 5MB'); return; }
      setPendingAvatarFile(file);
      const reader = new FileReader();
      reader.onloadend = () => setPendingAvatarPreview(reader.result as string);
      reader.readAsDataURL(file);
    };
    input.click();
  };

  const handleAvatarSave = async () => {
    if (!id || !pendingAvatarFile) return;
    try {
      setUploadingAvatar(true);
      const formData = new FormData();
      formData.append('file', pendingAvatarFile);
      const res = await crewApi.uploadAvatar(id, formData);
      if (res.crewMember) await refetch();
      setPendingAvatarFile(null);
      setPendingAvatarPreview(null);
      toast.success('C?p nh?t ?nh th�nh c�ng!');
    } catch (err: any) {
      toast.error(err.message || 'Upload th?t b?i');
    } finally {
      setUploadingAvatar(false);
    }
  };

  const handleAvatarCancel = () => {
    setPendingAvatarFile(null);
    setPendingAvatarPreview(null);
  };

  // Certificate handlers
  const handleCertificateFileUpload = (certId: number) => {    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.jpg,.jpeg,.png,.gif,.pdf';
    input.onchange = async (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (!file) return;
      try {
        setUploadingCertId(certId);
        const formData = new FormData();
        formData.append('file', file);
        const result = await certificateApi.uploadCertificateFile(certId, formData);
        // Instant update: patch the local cert state so the image renders immediately
        setCertificates(prev => prev.map(c =>
          c.id === certId ? { ...c, documentFilePath: result.documentFilePath } : c
        ));
        toast.success('Upload file th�nh c�ng!');
      } catch (err: any) {
        toast.error(err.message || 'Upload th?t b?i');
      } finally {
        setUploadingCertId(null);
      }
    };
    input.click();
  };

  const handleViewCertificateImage = (fileUrl: string, certId: number) => {
    setImageViewerUrl(fileUrl);
    setImageViewerCertId(certId);
    setIsImageViewerOpen(true);
  };

  const handleCertificateUploadHandler = async (documentId: string, formData: FormData) => {
    const certId = parseInt(documentId);
    const result = await certificateApi.uploadCertificateFile(certId, formData);
    // Instant update local state
    setCertificates(prev => prev.map(c =>
      c.id === certId ? { ...c, documentFilePath: result.documentFilePath } : c
    ));
    // Update the image viewer URL immediately
    setImageViewerUrl(result.documentFilePath);
    return result;
  };

  const handleDeleteCertificate = async (certId: number) => {
    if (!window.confirm('B?n c� ch?c mu?n x�a ch?ng ch? n�y?')) return;
    try {
      await certificateApi.deleteCrewCertificate(certId);
      await refetchCerts();
      toast.success('�� x�a ch?ng ch?');
    } catch (err: any) {
      toast.error(err.message || 'Kh�ng th? x�a ch?ng ch?');
    }
  };

  const getCertStatus = (expiryDate?: string) => {
    if (!expiryDate) return { label: 'N/A', color: 'text-gray-500', bg: 'bg-gray-100', Icon: AlertTriangle };
    const days = Math.floor((new Date(expiryDate).getTime() - Date.now()) / 86400000);
    if (days < 0) return { label: 'Expired', color: 'text-red-600', bg: 'bg-red-100', Icon: XCircle, days };
    if (days < 90) return { label: 'Expiring', color: 'text-yellow-600', bg: 'bg-yellow-100', Icon: AlertTriangle, days };
    return { label: 'Valid', color: 'text-green-600', bg: 'bg-green-100', Icon: CheckCircle, days };
  };

  if (loading) return (
    <div className="flex items-center justify-center min-h-screen">
      <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600" />
    </div>
  );

  if (error || !crew) return (
    <div className="p-8">
      <div className="bg-red-50 text-red-700 px-4 py-3 rounded">
        <p className="font-semibold">Error loading crew details</p>
        <p className="text-sm">{error || 'Crew member not found'}</p>
      </div>
    </div>
  );

  const age = calcAge(edited.dateOfBirth ?? crew.dateOfBirth);

  const fieldCls = 'cd-field';
  const labelCls = 'cd-label';

  const handleDocFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!docFileUploadTarget || !id) return;
    const file = e.target.files?.[0];
    if (!file) return;
    e.target.value = '';
    setUploadingDocFile(true);
    try {
      const fd = new FormData();
      fd.append('file', file);
      await crewApi.uploadDocumentFile(id, docFileUploadTarget.category, docFileUploadTarget.docId, fd);
      toast.success('Tai anh/file tai lieu thanh cong!');
      await loadDocuments(true);
    } catch (err: unknown) {
      toast.error((err instanceof Error ? err.message : null) || 'Khong the tai file');
    } finally {
      setUploadingDocFile(false);
      setDocFileUploadTarget(null);
    }
  };

  const triggerDocFileUpload = (docId: string, category: string) => {
    setDocFileUploadTarget({ docId, category });
    docFileInputRef.current?.click();
  };

  const DocTable = ({ docs, emoji }: { docs: CrewDocument[]; emoji: string }) => (
    <div className="overflow-x-auto">
      <table className="w-full border-collapse text-sm" style={{ tableLayout: 'fixed' }}>
        <thead className="cd-table-thead">
          <tr>
            <th style={{ width: '22%' }}>Name</th>
            <th style={{ width: '18%' }}>Number</th>
            <th style={{ width: '14%' }}>Issue Date</th>
            <th style={{ width: '14%' }}>Expiry Date</th>
            <th style={{ width: '14%' }}>Country</th>
            <th style={{ width: '18%', textAlign: 'center' }}>File</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {docs.length === 0 ? (
            <tr><td colSpan={6} className="px-4 py-6 text-center text-gray-400">No documents</td></tr>
          ) : docs.map(doc => (
            <tr key={doc.id} className="hover:bg-gray-50">
              <td className="px-4 py-2 font-medium text-gray-800 truncate">
                <span className="mr-2">{emoji}</span>{doc.documentType}
              </td>
              <td className="px-4 py-2 text-gray-600 truncate">{doc.documentNumber || '--'}</td>
              <td className="px-4 py-2 text-gray-600">{fmt(doc.issueDate)}</td>
              <td className="px-4 py-2 text-gray-600">{fmt(doc.expiryDate)}</td>
              <td className="px-4 py-2 text-gray-600 truncate">{doc.countryName || '--'}</td>
              <td className="px-4 py-2 text-center">
                <div className="inline-flex items-center gap-1 justify-center">
                  {doc.fileUrl && (
                    <button
                      onClick={() => {
                        setImageViewerUrl(doc.fileUrl!);
                        setImageViewerCertId(null);
                        setImageViewerDocTarget({ docId: doc.id, category: doc.category });
                        setIsImageViewerOpen(true);
                      }}
                      title="Xem file"
                      className="inline-flex items-center justify-center w-7 h-7 rounded bg-teal-500 hover:bg-teal-600 text-white"
                      style={{ border: 'none', cursor: 'pointer' }}>
                      <Eye className="w-3.5 h-3.5" />
                    </button>
                  )}
                  {!doc.fileUrl && (
                    <button
                      onClick={() => triggerDocFileUpload(doc.id, doc.category)}
                      disabled={uploadingDocFile}
                      title="Tải ảnh/file lên"
                      className="inline-flex items-center justify-center w-7 h-7 rounded text-white bg-gray-400 hover:bg-gray-500"
                      style={{ border: 'none', cursor: 'pointer' }}>
                      <Upload className="w-3.5 h-3.5" />
                    </button>
                  )}
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );



  return (
    <div className="min-h-screen bg-gray-100">
      {/* ── Header ── */}
      <div className="bg-white border-b border-gray-200 px-6 py-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <button onClick={() => navigate(vesselId ? `/vessels/${vesselId}` : '/crew')} className="p-2 hover:bg-gray-100 rounded">
              <ArrowLeft className="h-5 w-5" />
            </button>
            <h1 className="text-lg font-semibold text-gray-800 uppercase">
              EDIT {crew.fullName} � {crew.rankName || 'CREW'}
            </h1>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={handleSave}
              disabled={saving}
              className="px-6 py-2 text-white rounded font-medium disabled:opacity-50 text-sm"
              style={{ background: '#0d7377' }}
            >
              {saving ? 'Saving...' : 'Save'}
            </button>
          </div>
        </div>
      </div>
      {/* Hold Notification Banner */}
      {crew.onboardStatus === 'OnHold' && (
        <div style={{
          background: '#fff7ed', border: '1px solid #fb923c', borderRadius: 0,
          padding: '12px 24px', display: 'flex', alignItems: 'flex-start', gap: 12
        }}>
          <AlertTriangle className="h-5 w-5 flex-shrink-0" style={{ color: '#ea580c', marginTop: 2 }} />
          <div style={{ flex: 1 }}>
            <div style={{ fontWeight: 700, fontSize: 14, color: '#c2410c', marginBottom: 4 }}>
              T�u y�u c?u b? sung h? so cho thuy?n vi�n n�y
            </div>
            {crew.reviewNotes && (
              <div style={{ fontSize: 13, color: '#9a3412', background: '#ffedd5', borderRadius: 6, padding: '8px 12px', marginTop: 4 }}>
                <strong>Ghi ch� t? t�u:</strong> {crew.reviewNotes}
              </div>
            )}
            {crew.onboardStatusChangedBy && (
              <div style={{ fontSize: 12, color: '#a0aec0', marginTop: 6 }}>
                B?i: {crew.onboardStatusChangedBy}
                {crew.onboardStatusChangedAt && ` � ${new Date(crew.onboardStatusChangedAt).toLocaleString('vi-VN')}`}
              </div>
            )}
          </div>
        </div>
      )}
      {/* Edge Changes Banner */}
      {hasUnviewedChanges && (
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '10px 24px', background: '#fef2f2', borderBottom: '2px solid #fca5a5' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, fontSize: 13, color: '#991b1b' }}>
            <span style={{ display: 'inline-flex', alignItems: 'center', justifyContent: 'center', minWidth: 22, height: 22, padding: '0 6px', background: '#ef4444', color: '#fff', fontSize: 12, fontWeight: 700, borderRadius: 11 }}>{edgeChanges.length}</span>
            <span>T�u d� thay d?i <strong>{edgeChanges.length}</strong> th�ng tin. C�c tru?ng thay d?i du?c d�nh d?u <span style={{ color: '#ef4444', fontWeight: 700 }}>M�U �?</span> b�n du?i.</span>
          </div>
          <button onClick={handleMarkViewed} style={{ padding: '5px 14px', fontSize: 12, fontWeight: 600, color: '#fff', background: '#0d7377', border: 'none', borderRadius: 4, cursor: 'pointer' }}>? �� xem</button>
        </div>
      )}
      {/* Tabs */}
      <div className="bg-white" style={{ borderBottom: '1px solid #b5e3da' }}>
        <div className="px-6 flex gap-1">
          {([
            { key: 'basic-data', label: 'Basic Data' },
            { key: 'documents', label: 'Documents' },
            { key: 'voyage-history', label: 'Voyage History', icon: <Ship className="w-4 h-4" /> },
            { key: 'onboarding', label: 'Onboarding', icon: <ClipboardList className="w-4 h-4" /> },
            { key: 'doc-workflow', label: 'Doc Workflow', icon: <FileCheck className="w-4 h-4" /> },
            { key: 'status-history', label: 'Status', icon: <History className="w-4 h-4" /> },
            { key: 'audit', label: 'Audit', icon: <ScrollText className="w-4 h-4" /> },
          ] as { key: TabType; label: string; icon?: React.ReactNode }[]).map(tab => (
            <button
              key={tab.key}
              onClick={() => handleTabChange(tab.key)}
              className={`flex items-center gap-1.5 px-6 py-3 text-sm font-medium border-b-2 transition-colors ${
                activeTab === tab.key ? '' : 'border-transparent text-gray-600 hover:text-gray-800'
              }`}
              style={activeTab === tab.key ? { borderBottomColor: '#0d7377', color: '#0d7377', background: '#e6f5f3' } : {}}
            >
              {tab.icon}{tab.label}
              {(() => {
                const changeCount = getTabChangeCount(tab.key);
                return changeCount > 0 ? <span className="cd-tab-change-badge">{changeCount}</span> : null;
              })()}
              {tab.key === 'documents' && (travelDocs.length + seafarerDocs.length + healthDocs.length) > 0 && (
                <span className="ml-1 px-1.5 py-0.5 rounded-full text-xs bg-gray-200 text-gray-600">
                  {travelDocs.length + seafarerDocs.length + employmentDocs.length + healthDocs.length}
                </span>
              )}
              {tab.key === 'voyage-history' && serviceRecords.length > 0 && (
                <span className="ml-1 px-1.5 py-0.5 rounded-full text-xs bg-gray-200 text-gray-600">
                  {serviceRecords.length}
                </span>
              )}
            </button>
          ))}
        </div>
      </div>

      {/* ── Content ── */}
      <div className="p-3 space-y-3">

        {/* ════════ BASIC DATA ════════ */}
        {activeTab === 'basic-data' && (
          <>
            {/* Main form card */}
            <div className="cd-section">
            <div className="cd-section-body" style={{ padding: '16px 20px' }}>
              <div className="grid grid-cols-12 gap-6">

                {/* Col 1: Name + Rank */}
                <div className="col-span-3 space-y-3">
                  <div>
                    <label className={labelCls}>Full Name</label>
                    <input className={`${fieldCls}${fieldHighlight('fullName')}`} style={fieldStyle('fullName')} value={edited.fullName ?? ''} onChange={e => set('fullName', e.target.value)} />
                    {changeIndicator('fullName')}
                  </div>
                  <div>
                    <label className={labelCls}>Rank</label>
                    <select className={`${fieldCls}${fieldHighlight('rankId')}`} style={fieldStyle('rankId')} value={edited.rankId ?? ''} onChange={e => set('rankId', e.target.value ? Number(e.target.value) : undefined)}>
                      <option value="">Select rank</option>
                      {ranks.map(r => <option key={r.id} value={r.id}>{r.rankName} ({r.rankCode})</option>)}
                    </select>
                    {changeIndicator('rankId')}
                  </div>
                  <div>
                    <label className={labelCls}>Department</label>
                    <input className={`${fieldCls}${fieldHighlight('department')}`} style={fieldStyle('department')} value={edited.department ?? ''} onChange={e => set('department', e.target.value)} />
                    {changeIndicator('department')}
                  </div>
                  <div>
                    <label className={labelCls}>Date of Birth</label>
                    <input type="date" className={`${fieldCls}${fieldHighlight('dateOfBirth')}`} style={fieldStyle('dateOfBirth')} value={(edited.dateOfBirth ?? '').split('T')[0]} onChange={e => set('dateOfBirth', e.target.value)} />
                    {changeIndicator('dateOfBirth')}
                  </div>
                </div>

                {/* Col 2: Personal */}
                <div className="col-span-3 space-y-3">
                  <div>
                    <label className={labelCls}>Age</label>
                    <input className={fieldCls} value={age} readOnly />
                  </div>
                  <div>
                    <label className={labelCls}>Place of Birth</label>
                    <input className={`${fieldCls}${fieldHighlight('placeOfBirth')}`} style={fieldStyle('placeOfBirth')} value={edited.placeOfBirth ?? ''} onChange={e => set('placeOfBirth', e.target.value)} />
                    {changeIndicator('placeOfBirth')}
                  </div>
                  <div>
                    <label className={labelCls}>Nationality</label>
                    <select className={`${fieldCls}${fieldHighlight('countryId')}`} style={fieldStyle('countryId')} value={edited.countryId ?? ''} onChange={e => set('countryId', e.target.value ? Number(e.target.value) : undefined)}>
                      <option value="">Select country</option>
                      {countries.map(c => <option key={c.id} value={c.id}>{c.countryName}</option>)}
                    </select>
                    {changeIndicator('countryId')}
                  </div>
                  <div>
                    <label className={labelCls}>ID Card Number</label>
                    <input className={`${fieldCls}${fieldHighlight('idCardNumber')}`} style={fieldStyle('idCardNumber')} value={edited.idCardNumber ?? ''} onChange={e => set('idCardNumber', e.target.value)} />
                    {changeIndicator('idCardNumber')}
                  </div>
                </div>

                {/* Col 3: Contact */}
                <div className="col-span-3 space-y-3">
                  <div>
                    <label className={labelCls}>Phone Number</label>
                    <input className={`${fieldCls}${fieldHighlight('phoneNumber')}`} style={fieldStyle('phoneNumber')} value={edited.phoneNumber ?? ''} onChange={e => set('phoneNumber', e.target.value)} />
                    {changeIndicator('phoneNumber')}
                  </div>
                  <div>
                    <label className={labelCls}>Email</label>
                    <input type="email" className={`${fieldCls}${fieldHighlight('emailAddress')}`} style={fieldStyle('emailAddress')} value={edited.emailAddress ?? ''} onChange={e => set('emailAddress', e.target.value)} />
                    {changeIndicator('emailAddress')}
                  </div>
                  <div>
                    <label className={labelCls}>Marital Status</label>
                    <select className={`${fieldCls}${fieldHighlight('maritalStatus')}`} style={fieldStyle('maritalStatus')} value={edited.maritalStatus ?? ''} onChange={e => set('maritalStatus', e.target.value)}>
                      <option value="">Select</option>
                      <option>Single</option><option>Married</option><option>Divorced</option><option>Widowed</option>
                    </select>
                    {changeIndicator('maritalStatus')}
                  </div>
                  <div className="grid grid-cols-2 gap-2">
                    <div>
                      <label className={labelCls}>Height (cm)</label>
                      <input type="number" className={`${fieldCls}${fieldHighlight('height')}`} style={fieldStyle('height')} value={edited.height ?? ''} onChange={e => set('height', e.target.value ? Number(e.target.value) : undefined)} />
                      {changeIndicator('height')}
                    </div>
                    <div>
                      <label className={labelCls}>Weight (kg)</label>
                      <input type="number" className={`${fieldCls}${fieldHighlight('weight')}`} style={fieldStyle('weight')} value={edited.weight ?? ''} onChange={e => set('weight', e.target.value ? Number(e.target.value) : undefined)} />
                      {changeIndicator('weight')}
                    </div>
                  </div>
                </div>

                {/* Col 4: Avatar + ID */}
                <div className="col-span-3 flex flex-col items-center">
                  <div className="mb-3">
                    <label className={`${labelCls}`} style={{ textAlign: 'center' }}>Company ID Number</label>
                    <input className={`${fieldCls} text-center w-32`} value={edited.crewId ?? ''} onChange={e => set('crewId', e.target.value)} />
                  </div>
                  <div className="w-40 h-52 rounded-lg overflow-hidden bg-gray-200 shadow-md relative">
                    <ProtectedImage
                      src={pendingAvatarPreview || crew.avatarUrl}
                      fallbackSrc="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 260'%3E%3Crect width='200' height='260' fill='%23e5e7eb'/%3E%3Ccircle cx='100' cy='70' r='35' fill='%239ca3af'/%3E%3Cellipse cx='100' cy='180' rx='65' ry='50' fill='%239ca3af'/%3E%3C/svg%3E"
                      alt="Avatar"
                      className="w-full h-full object-cover"
                    />
                    {uploadingAvatar && (
                      <div className="absolute inset-0 bg-black bg-opacity-50 flex items-center justify-center">
                        <div className="animate-spin rounded-full h-8 w-8 border-4 border-white border-t-transparent" />
                      </div>
                    )}
                    {pendingAvatarPreview && !uploadingAvatar && (
                      <div className="absolute top-1 right-1 bg-yellow-500 text-white text-xs px-1.5 py-0.5 rounded font-medium">NEW</div>
                    )}
                  </div>
                  <div className="flex gap-2 mt-2">
                    {pendingAvatarFile ? (
                      <>
                        <button
                          onClick={handleAvatarSave}
                          disabled={uploadingAvatar}
                          className={`px-3 py-1.5 text-white text-xs rounded flex items-center gap-1 ${uploadingAvatar ? 'bg-gray-400 cursor-not-allowed' : 'bg-green-600 hover:bg-green-700'}`}
                        >
                          <Upload className="w-3 h-3" /> {uploadingAvatar ? '�ang luu...' : 'Luu'}
                        </button>
                        <button
                          onClick={handleAvatarCancel}
                          disabled={uploadingAvatar}
                          className="px-3 py-1.5 text-white text-xs rounded bg-gray-500 hover:bg-gray-600"
                        >
                          Hu?
                        </button>
                      </>
                    ) : (
                      <button
                        onClick={handleAvatarChoose}
                        className="px-3 py-1.5 text-white text-xs rounded bg-teal-600 hover:bg-teal-700 flex items-center gap-1"
                      >
                        <Upload className="w-3 h-3" /> Thay ?nh
                      </button>
                    )}
                  </div>
                  <p className="text-xs text-gray-400 mt-1 text-center">
                    {pendingAvatarFile ? 'Nh?n Luu d? x�c nh?n' : 'JPG/PNG, t?i da 5MB'}
                  </p>
                  <div className="mt-3 flex items-center gap-2">
                    <input type="checkbox" checked={edited.isOnboard ?? false} onChange={e => set('isOnboard', e.target.checked)} className="w-4 h-4 text-teal-600" />
                    <label className="text-sm font-medium text-gray-700">On Board</label>
                  </div>
                </div>
              </div>
            </div>
            </div>

            {/* Physical Details */}
            <div className="cd-section">
              <div className="cd-section-header"><h3 className="cd-section-title">Physical Details &amp; Preferences</h3></div>
              <div className="cd-section-body">
              <div className="grid grid-cols-4 gap-4">
                <div>
                  <label className={labelCls}>Blood Group</label>
                  <select className={`${fieldCls}${fieldHighlight('bloodGroup')}`} style={fieldStyle('bloodGroup')} value={edited.bloodGroup ?? ''} onChange={e => set('bloodGroup', e.target.value)}>
                    <option value="">Select</option>
                    {['A+','A-','B+','B-','AB+','AB-','O+','O-'].map(g => <option key={g}>{g}</option>)}
                  </select>
                  {changeIndicator('bloodGroup')}
                </div>
                <div>
                  <label className={labelCls}>Clothing Size</label>
                  <input className={`${fieldCls}${fieldHighlight('clothingSize')}`} style={fieldStyle('clothingSize')} placeholder="e.g., L, XL" value={edited.clothingSize ?? ''} onChange={e => set('clothingSize', e.target.value)} />
                  {changeIndicator('clothingSize')}
                </div>
                <div>
                  <label className={labelCls}>Shoe Size</label>
                  <input className={`${fieldCls}${fieldHighlight('shoeSize')}`} style={fieldStyle('shoeSize')} placeholder="e.g., 42" value={edited.shoeSize ?? ''} onChange={e => set('shoeSize', e.target.value)} />
                  {changeIndicator('shoeSize')}
                </div>
                <div>
                  <label className={labelCls}>Catering Size</label>
                  <input className={`${fieldCls}${fieldHighlight('cateringSize')}`} style={fieldStyle('cateringSize')} placeholder="e.g., M" value={edited.cateringSize ?? ''} onChange={e => set('cateringSize', e.target.value)} />
                  {changeIndicator('cateringSize')}
                </div>
              </div>
              <div className="flex gap-6 mt-4">
                <label className="flex items-center gap-2 text-sm">
                  <input type="checkbox" checked={edited.isSmoker ?? false} onChange={e => set('isSmoker', e.target.checked)} className="w-4 h-4" />
                  Smoker
                </label>
                <label className="flex items-center gap-2 text-sm">
                  <input type="checkbox" checked={edited.isCovidVaccinated ?? false} onChange={e => set('isCovidVaccinated', e.target.checked)} className="w-4 h-4" />
                  COVID-19 Vaccinated
                </label>
              </div>
              </div>
            </div>

            {/* Employment Dates */}
            <div className="cd-section">
              <div className="cd-section-header"><h3 className="cd-section-title">Employment Dates</h3></div>
              <div className="cd-section-body">
              <div className="grid grid-cols-4 gap-4">
                {([
                  { label: 'Join Date', key: 'joinDate' },
                  { label: 'Embark Date', key: 'embarkDate' },
                  { label: 'Disembark Date', key: 'disembarkDate' },
                  { label: 'Contract End', key: 'contractEnd' },
                ] as { label: string; key: keyof UpdateCrewRequest }[]).map(({ label, key }) => (
                  <div key={key}>
                    <label className={labelCls}>{label}</label>
                    <input type="date" className={`${fieldCls}${fieldHighlight(key)}`} style={fieldStyle(key)}
                      value={((edited[key] as string) ?? '').split('T')[0]}
                      onChange={e => set(key, e.target.value)} />
                    {changeIndicator(key)}
                  </div>
                ))}
              </div>
              </div>
            </div>

            {/* Next of Kin */}
            <div className="cd-section">
              <div className="cd-section-header"><h3 className="cd-section-title">Next of Kin / Emergency Contact</h3></div>
              <div className="cd-section-body">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className={labelCls}>Full Name</label>
                  <input className={`${fieldCls}${fieldHighlight('nextOfKinName')}`} style={fieldStyle('nextOfKinName')} value={edited.nextOfKinName ?? ''} onChange={e => set('nextOfKinName', e.target.value)} />
                  {changeIndicator('nextOfKinName')}
                </div>
                <div>
                  <label className={labelCls}>Relationship</label>
                  <select className={`${fieldCls}${fieldHighlight('nextOfKinRelation')}`} style={fieldStyle('nextOfKinRelation')} value={edited.nextOfKinRelation ?? ''} onChange={e => set('nextOfKinRelation', e.target.value)}>
                    <option value="">Select</option>
                    {['Father','Mother','Spouse','Sibling','Child','Other'].map(r => <option key={r}>{r}</option>)}
                  </select>
                  {changeIndicator('nextOfKinRelation')}
                </div>
                <div>
                  <label className={labelCls}>Phone Number</label>
                  <input className={`${fieldCls}${fieldHighlight('nextOfKinPhone')}`} style={fieldStyle('nextOfKinPhone')} value={edited.nextOfKinPhone ?? ''} onChange={e => set('nextOfKinPhone', e.target.value)} />
                  {changeIndicator('nextOfKinPhone')}
                </div>
                <div>
                  <label className={labelCls}>Address</label>
                  <input className={`${fieldCls}${fieldHighlight('nextOfKinAddress')}`} style={fieldStyle('nextOfKinAddress')} value={edited.nextOfKinAddress ?? ''} onChange={e => set('nextOfKinAddress', e.target.value)} />
                  {changeIndicator('nextOfKinAddress')}
                </div>
              </div>
              </div>
            </div>

            {/* Education */}
            <div className="cd-section">
              <div className="cd-section-header"><h3 className="cd-section-title">Education Background</h3></div>
              <div className="cd-section-body">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className={labelCls}>Institution / University</label>
                  <input className={`${fieldCls}${fieldHighlight('educationInstitution')}`} style={fieldStyle('educationInstitution')} placeholder="e.g., Vietnam Maritime University" value={edited.educationInstitution ?? ''} onChange={e => set('educationInstitution', e.target.value)} />
                  {changeIndicator('educationInstitution')}
                </div>
                <div>
                  <label className={labelCls}>Course / Major</label>
                  <input className={`${fieldCls}${fieldHighlight('educationCourse')}`} style={fieldStyle('educationCourse')} placeholder="e.g., BSc Nautical Science" value={edited.educationCourse ?? ''} onChange={e => set('educationCourse', e.target.value)} />
                  {changeIndicator('educationCourse')}
                </div>
                <div>
                  <label className={labelCls}>Period (Years)</label>
                  <input type="number" className={`${fieldCls}${fieldHighlight('educationPeriodYears')}`} style={fieldStyle('educationPeriodYears')} placeholder="e.g., 4" value={edited.educationPeriodYears ?? ''} onChange={e => set('educationPeriodYears', e.target.value ? Number(e.target.value) : undefined)} />
                  {changeIndicator('educationPeriodYears')}
                </div>
                <div>
                  <label className={labelCls}>Graduation Year</label>
                  <input type="number" className={`${fieldCls}${fieldHighlight('educationGraduationYear')}`} style={fieldStyle('educationGraduationYear')} placeholder="e.g., 2020" value={edited.educationGraduationYear ?? ''} onChange={e => set('educationGraduationYear', e.target.value ? Number(e.target.value) : undefined)} />
                  {changeIndicator('educationGraduationYear')}
                </div>
              </div>
              </div>
            </div>

            {/* Notes */}
            <div className="cd-section">
              <div className="cd-section-header"><h3 className="cd-section-title">Notes</h3></div>
              <div className="cd-section-body">
              <textarea className={`${fieldCls} resize-none${fieldHighlight('notes')}`} style={fieldStyle('notes')} rows={4} value={edited.notes ?? ''} onChange={e => set('notes', e.target.value)} />
              </div>
            </div>
          </>
        )}

        {/* ════════ DOCUMENTS ════════ */}
        {activeTab === 'documents' && (
          <>
            {docsLoading ? (
              <div className="flex items-center justify-center py-20">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
              </div>
            ) : (
              <>
                {/* Identity Documents */}
                <div className="cd-section">
                  <div className="cd-section-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <h3 className="cd-section-title">
                      Identity Documents ({travelDocs.length + seafarerDocs.length + employmentDocs.length})
                    </h3>
                    <button
                      onClick={() => setIsAddDocModalOpen(true)}
                      style={{ display: 'inline-flex', alignItems: 'center', gap: 6, padding: '6px 14px', background: '#0d7377', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13, fontWeight: 600 }}
                    >
                      <Plus className="w-4 h-4" /> Th�m t�i li?u
                    </button>
                  </div>
                  <DocTable docs={[...travelDocs, ...seafarerDocs, ...employmentDocs]} emoji="??" />
                </div>

                {/* Health Documents */}
                <div className="cd-section">
                  <div className="cd-section-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <h3 className="cd-section-title">
                      Health Documents ({healthDocs.length})
                    </h3>
                    <button
                      onClick={() => setIsAddHealthDocModalOpen(true)}
                      style={{ display: 'inline-flex', alignItems: 'center', gap: 6, padding: '6px 14px', background: '#0d7377', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13, fontWeight: 600 }}
                    >
                      <Plus className="w-4 h-4" /> Th�m t�i li?u s?c kh?e
                    </button>
                  </div>
                  <DocTable docs={healthDocs} emoji="??" />
                </div>

                {/* Certificates */}
                <div className="cd-section">
                  <div className="cd-section-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <h3 className="cd-section-title">
                      Certificates ({certificates?.length ?? 0})
                    </h3>
                    <button
                      onClick={() => { setEditingCert(null); setShowAddCertModal(true); }}
                      style={{ display: 'inline-flex', alignItems: 'center', gap: 6, padding: '6px 14px', background: '#0d7377', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13, fontWeight: 600 }}
                    >
                      <Plus className="w-4 h-4" /> Th�m ch?ng ch?
                    </button>
                  </div>
                  {certsLoading ? (
                    <div className="flex items-center justify-center py-10">
                      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
                    </div>
                  ) : !certificates || certificates.length === 0 ? (
                    <div className="text-center py-10 text-gray-400">
                      <p>Chua c� ch?ng ch? n�o</p>
                      <button
                        onClick={() => { setEditingCert(null); setShowAddCertModal(true); }}
                        style={{ marginTop: 8, padding: '6px 16px', background: '#0d7377', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13 }}
                      >
                        + Th�m ch?ng ch? d?u ti�n
                      </button>
                    </div>
                  ) : (
                    <div className="overflow-x-auto">
                      <table className="w-full text-sm border-collapse" style={{ tableLayout: 'fixed' }}>
                        <thead className="cd-table-thead">
                          <tr>
                            <th style={{ width: '20%' }}>Certificate Name</th>
                            <th style={{ width: '12%' }}>Number</th>
                            <th style={{ width: '11%' }}>Issue Date</th>
                            <th style={{ width: '11%' }}>Expiry Date</th>
                            <th style={{ width: '14%' }}>Issuing Authority</th>
                            <th style={{ width: '10%' }}>Status</th>
                            <th style={{ width: '8%', textAlign: 'center' }}>File</th>
                            <th style={{ width: '14%', textAlign: 'center' }}>Actions</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                          {certificates.map(cert => {
                            const s = getCertStatus(cert.expiryDate);
                            const fileUrl = cert.documentFilePath || cert.fileUrl;
                            return (
                              <tr key={cert.id} className="hover:bg-gray-50">
                                <td className="px-4 py-2">
                                  <div className="font-medium text-gray-900 truncate">{cert.certificateName || cert.certificateCode}</div>
                                  {cert.category && <div className="text-xs text-gray-400">{cert.category}</div>}
                                </td>
                                <td className="px-4 py-2 font-mono text-xs text-gray-700 truncate">{cert.certificateNumber || '�'}</td>
                                <td className="px-4 py-2 text-gray-600">{fmt(cert.issueDate)}</td>
                                <td className="px-4 py-2 text-gray-700">
                                  <div className="font-medium">{fmt(cert.expiryDate)}</div>
                                  {s.days !== undefined && <div className={`text-xs ${s.color}`}>{s.days} days</div>}
                                </td>
                                <td className="px-4 py-2 text-gray-600 truncate">{cert.issuingAuthority || '�'}</td>
                                <td className="px-4 py-2">
                                  <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-semibold ${s.bg} ${s.color}`}>
                                    <s.Icon className="w-3 h-3" />
                                    {s.label}
                                  </span>
                                </td>
                                <td className="px-4 py-2 text-center">
                                  <button
                                    onClick={() => fileUrl
                                      ? handleViewCertificateImage(fileUrl, cert.id)
                                      : handleCertificateFileUpload(cert.id)}
                                    disabled={uploadingCertId === cert.id}
                                    style={{ background: 'none', border: 'none', cursor: 'pointer', padding: 0 }}
                                    title={fileUrl ? 'Xem file' : 'Upload file'}
                                  >
                                    {uploadingCertId === cert.id ? (
                                      <div className="animate-spin" style={{ width: 16, height: 16, borderRadius: '50%', border: '2px solid #14b8a6', borderTopColor: 'transparent', display: 'inline-block' }} />
                                    ) : fileUrl ? (
                                      <span className="inline-flex items-center justify-center w-7 h-7 rounded bg-teal-500 hover:bg-teal-600 text-white">
                                        <Eye className="w-3.5 h-3.5" />
                                      </span>
                                    ) : (
                                      <span className="inline-flex items-center justify-center w-7 h-7 rounded bg-gray-200 hover:bg-gray-300 text-gray-500">
                                        <Upload className="w-3.5 h-3.5" />
                                      </span>
                                    )}
                                  </button>
                                </td>
                                <td className="px-4 py-2 text-center">
                                  <div style={{ display: 'flex', justifyContent: 'center', gap: 4 }}>
                                    <button
                                      onClick={() => { setEditingCert(cert); setShowAddCertModal(true); }}
                                      title="S?a"
                                      style={{ background: 'none', border: '1px solid #d1d5db', borderRadius: 4, padding: '4px 8px', cursor: 'pointer', display: 'inline-flex', alignItems: 'center' }}
                                    >
                                      <Pencil className="w-3.5 h-3.5" style={{ color: '#4b5563' }} />
                                    </button>
                                    <button
                                      onClick={() => handleDeleteCertificate(cert.id)}
                                      title="X�a"
                                      style={{ background: 'none', border: '1px solid #fecaca', borderRadius: 4, padding: '4px 8px', cursor: 'pointer', display: 'inline-flex', alignItems: 'center' }}
                                    >
                                      <Trash2 className="w-3.5 h-3.5" style={{ color: '#dc2626' }} />
                                    </button>
                                  </div>
                                </td>
                              </tr>
                            );
                          })}
                        </tbody>
                      </table>
                    </div>
                  )}
                </div>
              </>
            )}
          </>
        )}

        {/* ════════ VOYAGE HISTORY ════════ */}
        {activeTab === 'voyage-history' && (
          <div className="cd-section">
            <div className="cd-section-header">
              <div className="flex items-center gap-2">
                <Ship className="w-4 h-4" style={{ color: '#0d7377' }} />
                <h3 className="cd-section-title">Service Records</h3>
              </div>
              <span style={{ fontSize: 12, color: '#6b7280' }}>{serviceRecords.length} record(s)</span>
            </div>

            {recordsLoading ? (
              <div className="flex items-center justify-center py-16">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
              </div>
            ) : serviceRecords.length === 0 ? (
              <div className="text-center py-16">
                <Ship className="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <p className="text-gray-500 font-medium">No service records found</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead className="cd-table-thead">
                    <tr>
                      <th>Vessel</th>
                      <th>IMO</th>
                      <th>Rank</th>
                      <th>Sign-On</th>
                      <th>Sign-Off</th>
                      <th>Trade Area</th>
                      <th>Remarks</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {serviceRecords.map(rec => {
                      const days = rec.signOnDate && rec.signOffDate
                        ? Math.floor((new Date(rec.signOffDate).getTime() - new Date(rec.signOnDate).getTime()) / 86400000)
                        : rec.signOnDate
                          ? Math.floor((Date.now() - new Date(rec.signOnDate).getTime()) / 86400000)
                          : null;
                      return (
                        <tr key={rec.id} className="hover:bg-gray-50">
                          <td className="px-4 py-3">
                            <div className="flex items-center gap-2">
                              <Ship className="w-4 h-4 text-gray-400 flex-shrink-0" />
                              <div>
                                <div className="font-medium text-gray-800">{rec.vesselName}</div>
                                {days !== null && <div className="text-xs text-gray-400">{days} days</div>}
                              </div>
                            </div>
                          </td>
                          <td className="px-4 py-3 text-gray-500 font-mono text-xs">{rec.vesselIMO || '�'}</td>
                          <td className="px-4 py-3">
                            <span className="inline-flex px-2 py-0.5 rounded text-xs font-medium bg-teal-100 text-teal-700">
                              {rec.rankDuringService || '�'}
                            </span>
                          </td>
                          <td className="px-4 py-3">
                            <div className="flex items-center gap-1 text-gray-700">
                              <Calendar className="w-3.5 h-3.5 text-green-500" />
                              {fmt(rec.signOnDate)}
                            </div>
                          </td>
                          <td className="px-4 py-3">
                            {rec.signOffDate ? (
                              <div className="flex items-center gap-1 text-gray-700">
                                <Calendar className="w-3.5 h-3.5 text-red-400" />
                                {fmt(rec.signOffDate)}
                              </div>
                            ) : (
                              <span className="inline-flex px-2 py-0.5 rounded text-xs font-medium bg-green-100 text-green-700">
                                <MapPin className="w-3 h-3 mr-1" />On Board
                              </span>
                            )}
                          </td>
                          <td className="px-4 py-3 text-gray-600">{rec.tradingArea || '�'}</td>
                          <td className="px-4 py-3 text-gray-500 text-xs">{rec.remarks || '�'}</td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}

        {/* -- Onboarding Tab -- */}
        {activeTab === 'onboarding' && (
          <div className="bg-white rounded-lg border border-gray-200 p-4">
            <h2 className="text-lg font-semibold text-gray-800 mb-4">Onboarding</h2>
            {onbLoading ? (
              <div className="text-center py-8 text-gray-400">Loading onboarding data...</div>
            ) : !onboardingCase ? (
              <div className="text-center py-8 text-gray-400">
                <ClipboardList className="w-10 h-10 mx-auto mb-2 opacity-50" />
                <p>No onboarding case found for this crew member</p>
                <button
                  onClick={() => navigate(`/onboarding/new?crewId=${id}`)}
                  className="mt-3 px-4 py-2 text-sm bg-teal-600 text-white rounded hover:bg-teal-700"
                >
                  Create Onboarding Case
                </button>
              </div>
            ) : (
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <span className={`inline-block px-2 py-1 rounded text-xs font-semibold ${
                    onboardingCase.status === 'Activated' ? 'bg-green-100 text-green-700' :
                    onboardingCase.status === 'InProgress' ? 'bg-yellow-100 text-yellow-700' :
                    'bg-gray-100 text-gray-600'
                  }`}>{onboardingCase.status}</span>
                  {onboardingCase.referenceVesselName && (
                    <span className="text-sm text-gray-500"><Ship className="w-3.5 h-3.5 inline mr-1" />{onboardingCase.referenceVesselName}</span>
                  )}
                </div>
                <div className="flex items-center gap-3 mb-4">
                  <div className="flex-1 h-2 bg-gray-200 rounded-full overflow-hidden" style={{ maxWidth: 300 }}>
                    <div
                      className="h-full bg-green-500 rounded-full"
                      style={{ width: `${onboardingCase.totalItems > 0 ? Math.round(onboardingCase.completedItems / onboardingCase.totalItems * 100) : 0}%` }}
                    />
                  </div>
                  <span className="text-sm text-gray-600">
                    {onboardingCase.completedItems}/{onboardingCase.totalItems} items
                  </span>
                </div>
                <div className="space-y-2">
                  {onboardingCase.checklistItems.sort((a, b) => a.sortOrder - b.sortOrder).map(item => (
                    <div key={item.id} className={`flex items-center gap-3 px-3 py-2 rounded border ${
                      item.status === 'Completed' ? 'border-green-200 bg-green-50' :
                      item.status === 'Waived' ? 'border-purple-200 bg-purple-50 opacity-75' :
                      'border-gray-200'
                    }`}>
                      {item.status === 'Completed' ? <CheckCircle className="w-4 h-4 text-green-500 flex-shrink-0" /> :
                       item.status === 'Waived' ? <XCircle className="w-4 h-4 text-purple-400 flex-shrink-0" /> :
                       <div className="w-4 h-4 rounded-full border-2 border-gray-300 flex-shrink-0" />}
                      <span className="flex-1 text-sm text-gray-700">{item.title}</span>
                      {item.isMandatory && <span className="text-xs px-1.5 py-0.5 rounded bg-yellow-100 text-yellow-700">Required</span>}
                      <span className="text-xs text-gray-400">{item.status}</span>
                    </div>
                  ))}
                </div>
                <button
                  onClick={() => navigate(`/onboarding/${onboardingCase.id}`)}
                  className="mt-4 px-4 py-2 text-sm border border-blue-300 text-teal-600 rounded hover:bg-teal-50"
                >
                  View Full Details
                </button>
              </div>
            )}
          </div>
        )}

        {/* -- Document Workflow Tab -- */}
        {activeTab === 'doc-workflow' && (
          <div className="bg-white rounded-lg border border-gray-200 p-4">
            <h2 className="text-lg font-semibold text-gray-800 mb-4">Document Submissions</h2>
            {docSubLoading ? (
              <div className="text-center py-8 text-gray-400">Loading document submissions...</div>
            ) : docSubmissions.length === 0 ? (
              <div className="text-center py-8 text-gray-400">
                <FileCheck className="w-10 h-10 mx-auto mb-2 opacity-50" />
                <p>No document submissions yet</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full border-collapse text-sm">
                  <thead className="cd-table-thead">
                    <tr>
                      <th>Document</th>
                      <th>Number</th>
                      <th>Status</th>
                      <th>Submitted</th>
                      <th>Expiry</th>
                      <th>Verification</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {docSubmissions.map(doc => (
                      <tr key={doc.id} className="hover:bg-gray-50">
                        <td className="px-4 py-2 font-medium text-gray-800">
                          {doc.documentTitle || doc.documentType}
                        </td>
                        <td className="px-4 py-2 text-gray-600">{doc.documentNumber || '�'}</td>
                        <td className="px-4 py-2">
                          <span className={`inline-block px-2 py-0.5 rounded text-xs font-semibold ${
                            doc.status === 'Verified' ? 'bg-green-100 text-green-700' :
                            doc.status === 'Rejected' ? 'bg-red-100 text-red-700' :
                            doc.status === 'Submitted' || doc.status === 'SentForVerification' ? 'bg-teal-100 text-teal-700' :
                            'bg-gray-100 text-gray-600'
                          }`}>{doc.status}</span>
                        </td>
                        <td className="px-4 py-2 text-gray-600">{fmt(doc.submittedAt)}</td>
                        <td className="px-4 py-2 text-gray-600">{fmt(doc.expiryDate)}</td>
                        <td className="px-4 py-2 text-gray-500 text-xs">{doc.verificationStatus || '�'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}

        {/* -- Status History Tab -- */}
        {activeTab === 'status-history' && (
          <div className="bg-white rounded-lg border border-gray-200 p-4">
            <h2 className="text-lg font-semibold text-gray-800 mb-4">Status History</h2>
            {statusHistLoading ? (
              <div className="text-center py-8 text-gray-400">Loading status history...</div>
            ) : statusHistory.length === 0 ? (
              <div className="text-center py-8 text-gray-400">
                <History className="w-10 h-10 mx-auto mb-2 opacity-50" />
                <p>No status changes recorded</p>
              </div>
            ) : (
              <div className="space-y-3">
                {statusHistory.map(entry => (
                  <div key={entry.id} className="flex items-start gap-3 px-4 py-3 border border-gray-200 rounded-lg">
                    <div className="mt-0.5">
                      <History className="w-4 h-4 text-gray-400" />
                    </div>
                    <div className="flex-1">
                      <div className="flex items-center gap-2 text-sm">
                        <span className="px-2 py-0.5 rounded text-xs font-semibold bg-gray-100 text-gray-600">{entry.fromStatus}</span>
                        <span className="text-gray-400">?</span>
                        <span className="px-2 py-0.5 rounded text-xs font-semibold bg-teal-100 text-teal-700">{entry.toStatus}</span>
                      </div>
                      {entry.reason && <p className="text-sm text-gray-600 mt-1">{entry.reason}</p>}
                      <p className="text-xs text-gray-400 mt-1">by {entry.changedBy} � {fmt(entry.changedAt)}</p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* -- Audit Tab -- */}
        {activeTab === 'audit' && (
          <div className="bg-white rounded-lg border border-gray-200 p-4">
            <h2 className="text-lg font-semibold text-gray-800 mb-4">Audit Trail</h2>
            {auditLoading ? (
              <div className="text-center py-8 text-gray-400">Loading audit log...</div>
            ) : auditLogs.length === 0 ? (
              <div className="text-center py-8 text-gray-400">
                <ScrollText className="w-10 h-10 mx-auto mb-2 opacity-50" />
                <p>No audit records found</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full border-collapse text-sm">
                  <thead className="cd-table-thead">
                    <tr>
                      <th>Action</th>
                      <th>Entity</th>
                      <th>Actor</th>
                      <th>Details</th>
                      <th>Timestamp</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {auditLogs.map(log => (
                      <tr key={log.id} className="hover:bg-gray-50">
                        <td className="px-4 py-2">
                          <span className="px-2 py-0.5 rounded text-xs font-semibold bg-gray-100 text-gray-700">{log.action}</span>
                        </td>
                        <td className="px-4 py-2 text-gray-600 text-xs">{log.entityType}</td>
                        <td className="px-4 py-2 text-gray-700">{log.actor}</td>
                        <td className="px-4 py-2 text-gray-500 text-xs truncate" style={{ maxWidth: 300 }}>{log.details || '�'}</td>
                        <td className="px-4 py-2 text-gray-500 text-xs">{fmt(log.timestamp)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}
      </div>

      {/* Certificate Modals */}
      {id && (
        <AddCrewCertificateModal
          isOpen={showAddCertModal}
          onClose={() => { setShowAddCertModal(false); setEditingCert(null); }}
          onSave={() => { refetchCerts(); }}
          crewMemberId={id}
          editingCertificate={editingCert}
        />
      )}

      {/* Document Modals */}
      {id && (
        <>
          <AddDocumentModal
            isOpen={isAddDocModalOpen}
            crewMemberId={id}
            onClose={() => setIsAddDocModalOpen(false)}
            onSuccess={() => loadDocuments(true)}
          />
          <AddHealthDocumentModal
            isOpen={isAddHealthDocModalOpen}
            crewMemberId={id}
            onClose={() => setIsAddHealthDocModalOpen(false)}
            onSuccess={() => loadDocuments(true)}
          />
        </>
      )}

      {/* Hidden file input for document file upload */}
      <input
        ref={docFileInputRef}
        type="file"
        accept=".jpg,.jpeg,.png,.gif,.pdf"
        style={{ display: 'none' }}
        onChange={handleDocFileUpload}
      />

<ImageViewerModal
        isOpen={isImageViewerOpen}
        imageUrl={imageViewerUrl}
        documentId={
          imageViewerCertId != null
            ? String(imageViewerCertId)
            : imageViewerDocTarget != null
              ? imageViewerDocTarget.docId
              : undefined
        }
        customUploadHandler={
          imageViewerCertId != null
            ? handleCertificateUploadHandler
            : imageViewerDocTarget != null && id
              ? async (_docId: string, formData: FormData) => {
                  const result = await crewApi.uploadDocumentFile(id, imageViewerDocTarget.category, imageViewerDocTarget.docId, formData);
                  setImageViewerUrl(result.fileUrl);
                  await loadDocuments(true);
                  return result;
                }
              : undefined
        }
        onClose={() => {
          setIsImageViewerOpen(false);
          setImageViewerUrl(null);
          setImageViewerCertId(null);
          setImageViewerDocTarget(null);
        }}
        onFileChanged={() => {}}
      />
    </div>
  );
};



