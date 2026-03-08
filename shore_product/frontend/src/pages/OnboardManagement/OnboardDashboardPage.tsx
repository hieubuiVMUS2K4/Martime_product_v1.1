import { useState } from 'react';
import { Ship, Shield, UserCheck, UserX, RefreshCw, Plus, Search } from 'lucide-react';
import { useOnboardEvents, useAccessGrants, useSignOns, useSignOffs } from '../../hooks/useOnboard';
import { onboardEventApi, accessGrantApi, signOnApi, signOffApi } from '../../services/onboard.service';
import type { CreateOnboardEventRequest, CreateSignOnRequest, CreateSignOffRequest } from '../../types/onboard.types';
import { EVENT_TYPE_LABELS, ACCESS_STATUS_LABELS, SIGN_OFF_REASON_LABELS } from '../../types/onboard.types';
import './OnboardDashboardPage.css';

type TabKey = 'events' | 'access' | 'signOn' | 'signOff';

export const OnboardDashboardPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<TabKey>('events');
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState<'event' | 'signOn' | 'signOff' | null>(null);

  const events = useOnboardEvents();
  const access = useAccessGrants();
  const signOns = useSignOns();
  const signOffs = useSignOffs();

  const tabs: { key: TabKey; label: string; icon: React.ReactNode; count: number }[] = [
    { key: 'events', label: 'Sự kiện', icon: <Ship size={16} />, count: events.data.length },
    { key: 'access', label: 'Quyền truy cập', icon: <Shield size={16} />, count: access.data.length },
    { key: 'signOn', label: 'Sign-On', icon: <UserCheck size={16} />, count: signOns.data.length },
    { key: 'signOff', label: 'Sign-Off', icon: <UserX size={16} />, count: signOffs.data.length },
  ];

  const handleRefresh = () => {
    events.refetch();
    access.refetch();
    signOns.refetch();
    signOffs.refetch();
  };

  // Event modal form
  const [eventForm, setEventForm] = useState<CreateOnboardEventRequest>({
    crewMemberId: '', vesselId: '', eventType: 'Arrived', eventTimestamp: new Date().toISOString().slice(0, 16),
  });

  const [signOnForm, setSignOnForm] = useState<CreateSignOnRequest>({
    crewMemberId: '', vesselId: '', rankId: 0, signOnDate: new Date().toISOString().slice(0, 16), signedOnBy: '',
  });

  const [signOffForm, setSignOffForm] = useState<CreateSignOffRequest>({
    crewMemberId: '', vesselId: '', rankId: 0, signOffDate: new Date().toISOString().slice(0, 16),
    reason: 'ContractEnd', signedOffBy: '',
  });

  const handleCreateEvent = async () => {
    await onboardEventApi.create({ ...eventForm, eventTimestamp: new Date(eventForm.eventTimestamp).toISOString() });
    setShowModal(null);
    events.refetch();
  };

  const handleCreateSignOn = async () => {
    await signOnApi.create({ ...signOnForm, signOnDate: new Date(signOnForm.signOnDate).toISOString() });
    setShowModal(null);
    signOns.refetch();
    events.refetch();
    access.refetch();
  };

  const handleCreateSignOff = async () => {
    await signOffApi.create({ ...signOffForm, signOffDate: new Date(signOffForm.signOffDate).toISOString() });
    setShowModal(null);
    signOffs.refetch();
    events.refetch();
    access.refetch();
  };

  const handleRevokeAccess = async (id: string) => {
    await accessGrantApi.revoke(id, { revokeReason: 'Manual revoke', revokedBy: 'Admin' });
    access.refetch();
  };

  const handleSuspendAccess = async (id: string) => {
    await accessGrantApi.suspend(id, { reason: 'Manual suspend', suspendedBy: 'Admin' });
    access.refetch();
  };

  const handleReinstateAccess = async (id: string) => {
    await accessGrantApi.reinstate(id, 'Admin');
    access.refetch();
  };

  const lowerSearch = search.toLowerCase();

  const filteredEvents = events.data.filter(e =>
    (e.crewName?.toLowerCase() || '').includes(lowerSearch) ||
    (e.vesselName?.toLowerCase() || '').includes(lowerSearch)
  );

  const filteredAccess = access.data.filter(g =>
    (g.crewName?.toLowerCase() || '').includes(lowerSearch) ||
    (g.vesselName?.toLowerCase() || '').includes(lowerSearch)
  );

  const filteredSignOns = signOns.data.filter(s =>
    (s.crewName?.toLowerCase() || '').includes(lowerSearch) ||
    (s.vesselName?.toLowerCase() || '').includes(lowerSearch)
  );

  const filteredSignOffs = signOffs.data.filter(s =>
    (s.crewName?.toLowerCase() || '').includes(lowerSearch) ||
    (s.vesselName?.toLowerCase() || '').includes(lowerSearch)
  );

  const getEventBadge = (type: string) => {
    const map: Record<string, string> = {
      Arrived: 'badge-info', SignedOn: 'badge-success', SignedOff: 'badge-warning',
      Departed: 'badge-secondary', CorrectionArrived: 'badge-correction',
      CorrectionSignedOn: 'badge-correction', CorrectionSignedOff: 'badge-correction',
    };
    return map[type] || 'badge-secondary';
  };

  const getAccessBadge = (status: string) => {
    const map: Record<string, string> = {
      Granted: 'badge-success', PendingSync: 'badge-info', Suspended: 'badge-warning',
      Revoked: 'badge-danger', NotGranted: 'badge-secondary',
    };
    return map[status] || 'badge-secondary';
  };

  return (
    <div className="onboard-dashboard">
      <div className="page-header">
        <h1>Onboard & Sign-On/Off</h1>
        <div className="header-actions">
          <button className="btn-icon" onClick={handleRefresh} title="Refresh"><RefreshCw size={18} /></button>
          {activeTab === 'events' && (
            <button className="btn-primary" onClick={() => setShowModal('event')}><Plus size={16} /> Thêm sự kiện</button>
          )}
          {activeTab === 'signOn' && (
            <button className="btn-primary" onClick={() => setShowModal('signOn')}><Plus size={16} /> Sign-On</button>
          )}
          {activeTab === 'signOff' && (
            <button className="btn-primary" onClick={() => setShowModal('signOff')}><Plus size={16} /> Sign-Off</button>
          )}
        </div>
      </div>

      {/* Stats */}
      <div className="stats-row">
        {tabs.map(t => (
          <div key={t.key} className={`stat-card ${activeTab === t.key ? 'active' : ''}`} onClick={() => setActiveTab(t.key)}>
            <div className="stat-icon">{t.icon}</div>
            <div className="stat-info">
              <span className="stat-count">{t.count}</span>
              <span className="stat-label">{t.label}</span>
            </div>
          </div>
        ))}
      </div>

      {/* Search */}
      <div className="search-bar">
        <Search size={16} />
        <input type="text" placeholder="Tìm theo tên thuyền viên hoặc tàu..." value={search}
          onChange={e => setSearch(e.target.value)} />
      </div>

      {/* Tab Content */}
      {activeTab === 'events' && (
        <div className="table-container">
          {events.loading ? <p className="loading-text">Đang tải...</p> : events.error ? <p className="error-text">{events.error}</p> : (
            <table className="data-table">
              <thead>
                <tr>
                  <th>Thuyền viên</th>
                  <th>Tàu</th>
                  <th>Loại sự kiện</th>
                  <th>Thời gian</th>
                  <th>Cảng</th>
                  <th>Xác nhận bởi</th>
                  <th>Nguồn</th>
                </tr>
              </thead>
              <tbody>
                {filteredEvents.map(e => (
                  <tr key={e.id}>
                    <td>{e.crewName || '—'}</td>
                    <td>{e.vesselName || '—'}</td>
                    <td><span className={`badge ${getEventBadge(e.eventType)}`}>{EVENT_TYPE_LABELS[e.eventType] || e.eventType}</span></td>
                    <td>{new Date(e.eventTimestamp).toLocaleString('vi-VN')}</td>
                    <td>{e.portName || e.portCode || '—'}</td>
                    <td>{e.confirmedBy || '—'}</td>
                    <td><span className={`badge ${e.source === 'EDGE' ? 'badge-info' : 'badge-secondary'}`}>{e.source}</span></td>
                  </tr>
                ))}
                {filteredEvents.length === 0 && <tr><td colSpan={7} className="empty-cell">Không có sự kiện nào</td></tr>}
              </tbody>
            </table>
          )}
        </div>
      )}

      {activeTab === 'access' && (
        <div className="table-container">
          {access.loading ? <p className="loading-text">Đang tải...</p> : access.error ? <p className="error-text">{access.error}</p> : (
            <table className="data-table">
              <thead>
                <tr>
                  <th>Thuyền viên</th>
                  <th>Tàu</th>
                  <th>Module</th>
                  <th>Trạng thái</th>
                  <th>Cấp bởi</th>
                  <th>Cấp lúc</th>
                  <th>Hành động</th>
                </tr>
              </thead>
              <tbody>
                {filteredAccess.map(g => (
                  <tr key={g.id}>
                    <td>{g.crewName || '—'}</td>
                    <td>{g.vesselName || '—'}</td>
                    <td>{g.module}</td>
                    <td><span className={`badge ${getAccessBadge(g.status)}`}>{ACCESS_STATUS_LABELS[g.status] || g.status}</span></td>
                    <td>{g.grantedBy || '—'}</td>
                    <td>{g.grantedAt ? new Date(g.grantedAt).toLocaleString('vi-VN') : '—'}</td>
                    <td className="action-cell">
                      {g.status === 'Granted' && (
                        <>
                          <button className="btn-sm btn-warning" onClick={() => handleSuspendAccess(g.id)}>Tạm ngưng</button>
                          <button className="btn-sm btn-danger" onClick={() => handleRevokeAccess(g.id)}>Thu hồi</button>
                        </>
                      )}
                      {g.status === 'Suspended' && (
                        <>
                          <button className="btn-sm btn-success" onClick={() => handleReinstateAccess(g.id)}>Khôi phục</button>
                          <button className="btn-sm btn-danger" onClick={() => handleRevokeAccess(g.id)}>Thu hồi</button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
                {filteredAccess.length === 0 && <tr><td colSpan={7} className="empty-cell">Không có quyền truy cập nào</td></tr>}
              </tbody>
            </table>
          )}
        </div>
      )}

      {activeTab === 'signOn' && (
        <div className="table-container">
          {signOns.loading ? <p className="loading-text">Đang tải...</p> : signOns.error ? <p className="error-text">{signOns.error}</p> : (
            <table className="data-table">
              <thead>
                <tr>
                  <th>Thuyền viên</th>
                  <th>Tàu</th>
                  <th>Chức danh</th>
                  <th>Ngày Sign-On</th>
                  <th>Cảng</th>
                  <th>Ký bởi</th>
                  <th>Nguồn</th>
                </tr>
              </thead>
              <tbody>
                {filteredSignOns.map(s => (
                  <tr key={s.id}>
                    <td>{s.crewName || '—'}</td>
                    <td>{s.vesselName || '—'}</td>
                    <td>{s.rankName || '—'}</td>
                    <td>{new Date(s.signOnDate).toLocaleDateString('vi-VN')}</td>
                    <td>{s.portName || s.portCode || '—'}</td>
                    <td>{s.signedOnBy}</td>
                    <td><span className={`badge ${s.source === 'EDGE' ? 'badge-info' : 'badge-secondary'}`}>{s.source}</span></td>
                  </tr>
                ))}
                {filteredSignOns.length === 0 && <tr><td colSpan={7} className="empty-cell">Chưa có Sign-On nào</td></tr>}
              </tbody>
            </table>
          )}
        </div>
      )}

      {activeTab === 'signOff' && (
        <div className="table-container">
          {signOffs.loading ? <p className="loading-text">Đang tải...</p> : signOffs.error ? <p className="error-text">{signOffs.error}</p> : (
            <table className="data-table">
              <thead>
                <tr>
                  <th>Thuyền viên</th>
                  <th>Tàu</th>
                  <th>Chức danh</th>
                  <th>Ngày Sign-Off</th>
                  <th>Lý do</th>
                  <th>Cảng</th>
                  <th>Ký bởi</th>
                </tr>
              </thead>
              <tbody>
                {filteredSignOffs.map(s => (
                  <tr key={s.id}>
                    <td>{s.crewName || '—'}</td>
                    <td>{s.vesselName || '—'}</td>
                    <td>{s.rankName || '—'}</td>
                    <td>{new Date(s.signOffDate).toLocaleDateString('vi-VN')}</td>
                    <td>{SIGN_OFF_REASON_LABELS[s.reason] || s.reason}</td>
                    <td>{s.portName || s.portCode || '—'}</td>
                    <td>{s.signedOffBy}</td>
                  </tr>
                ))}
                {filteredSignOffs.length === 0 && <tr><td colSpan={7} className="empty-cell">Chưa có Sign-Off nào</td></tr>}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* Create Event Modal */}
      {showModal === 'event' && (
        <div className="modal-overlay" onClick={() => setShowModal(null)}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h2>Thêm sự kiện Onboard</h2>
            <div className="form-grid">
              <label>
                Crew Member ID
                <input type="text" value={eventForm.crewMemberId} onChange={e => setEventForm({ ...eventForm, crewMemberId: e.target.value })} />
              </label>
              <label>
                Vessel ID
                <input type="text" value={eventForm.vesselId} onChange={e => setEventForm({ ...eventForm, vesselId: e.target.value })} />
              </label>
              <label>
                Loại sự kiện
                <select value={eventForm.eventType} onChange={e => setEventForm({ ...eventForm, eventType: e.target.value })}>
                  {Object.entries(EVENT_TYPE_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
                </select>
              </label>
              <label>
                Thời gian
                <input type="datetime-local" value={eventForm.eventTimestamp} onChange={e => setEventForm({ ...eventForm, eventTimestamp: e.target.value })} />
              </label>
              <label>
                Cảng
                <input type="text" value={eventForm.portName || ''} onChange={e => setEventForm({ ...eventForm, portName: e.target.value })} placeholder="Tên cảng" />
              </label>
              <label>
                Xác nhận bởi
                <input type="text" value={eventForm.confirmedBy || ''} onChange={e => setEventForm({ ...eventForm, confirmedBy: e.target.value })} />
              </label>
            </div>
            <div className="modal-actions">
              <button className="btn-secondary" onClick={() => setShowModal(null)}>Hủy</button>
              <button className="btn-primary" onClick={handleCreateEvent}>Tạo sự kiện</button>
            </div>
          </div>
        </div>
      )}

      {/* Sign-On Modal */}
      {showModal === 'signOn' && (
        <div className="modal-overlay" onClick={() => setShowModal(null)}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h2>Sign-On thuyền viên</h2>
            <div className="form-grid">
              <label>
                Crew Member ID
                <input type="text" value={signOnForm.crewMemberId} onChange={e => setSignOnForm({ ...signOnForm, crewMemberId: e.target.value })} />
              </label>
              <label>
                Vessel ID
                <input type="text" value={signOnForm.vesselId} onChange={e => setSignOnForm({ ...signOnForm, vesselId: e.target.value })} />
              </label>
              <label>
                Rank ID
                <input type="number" value={signOnForm.rankId} onChange={e => setSignOnForm({ ...signOnForm, rankId: Number(e.target.value) })} />
              </label>
              <label>
                Ngày Sign-On
                <input type="datetime-local" value={signOnForm.signOnDate} onChange={e => setSignOnForm({ ...signOnForm, signOnDate: e.target.value })} />
              </label>
              <label>
                Cảng
                <input type="text" value={signOnForm.portName || ''} onChange={e => setSignOnForm({ ...signOnForm, portName: e.target.value })} placeholder="Tên cảng" />
              </label>
              <label>
                Ký bởi
                <input type="text" value={signOnForm.signedOnBy} onChange={e => setSignOnForm({ ...signOnForm, signedOnBy: e.target.value })} placeholder="Master / Chief Officer" />
              </label>
            </div>
            <div className="modal-actions">
              <button className="btn-secondary" onClick={() => setShowModal(null)}>Hủy</button>
              <button className="btn-primary" onClick={handleCreateSignOn}>Xác nhận Sign-On</button>
            </div>
          </div>
        </div>
      )}

      {/* Sign-Off Modal */}
      {showModal === 'signOff' && (
        <div className="modal-overlay" onClick={() => setShowModal(null)}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h2>Sign-Off thuyền viên</h2>
            <div className="form-grid">
              <label>
                Crew Member ID
                <input type="text" value={signOffForm.crewMemberId} onChange={e => setSignOffForm({ ...signOffForm, crewMemberId: e.target.value })} />
              </label>
              <label>
                Vessel ID
                <input type="text" value={signOffForm.vesselId} onChange={e => setSignOffForm({ ...signOffForm, vesselId: e.target.value })} />
              </label>
              <label>
                Rank ID
                <input type="number" value={signOffForm.rankId} onChange={e => setSignOffForm({ ...signOffForm, rankId: Number(e.target.value) })} />
              </label>
              <label>
                Ngày Sign-Off
                <input type="datetime-local" value={signOffForm.signOffDate} onChange={e => setSignOffForm({ ...signOffForm, signOffDate: e.target.value })} />
              </label>
              <label>
                Lý do
                <select value={signOffForm.reason} onChange={e => setSignOffForm({ ...signOffForm, reason: e.target.value })}>
                  {Object.entries(SIGN_OFF_REASON_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
                </select>
              </label>
              <label>
                Cảng
                <input type="text" value={signOffForm.portName || ''} onChange={e => setSignOffForm({ ...signOffForm, portName: e.target.value })} placeholder="Tên cảng" />
              </label>
              <label>
                Ký bởi
                <input type="text" value={signOffForm.signedOffBy} onChange={e => setSignOffForm({ ...signOffForm, signedOffBy: e.target.value })} placeholder="Master / Chief Officer" />
              </label>
              <label>
                Chi tiết lý do
                <textarea value={signOffForm.reasonDetail || ''} onChange={e => setSignOffForm({ ...signOffForm, reasonDetail: e.target.value })} rows={2} />
              </label>
            </div>
            <div className="modal-actions">
              <button className="btn-secondary" onClick={() => setShowModal(null)}>Hủy</button>
              <button className="btn-primary" onClick={handleCreateSignOff}>Xác nhận Sign-Off</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
