import React, { useState, useEffect, useRef } from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { Anchor, Bell, Menu, X } from 'lucide-react';
import { crewApi } from '../../../services/crew.service';
import type { HoldNotification } from '../../../services/crew.service';
import './TopNavLayout.css';

const LAST_SEEN_KEY = 'hold_notifications_last_seen';

function getLastSeenDate(): Date {
  const stored = localStorage.getItem(LAST_SEEN_KEY);
  return stored ? new Date(stored) : new Date(0);
}

function markAllSeen() {
  localStorage.setItem(LAST_SEEN_KEY, new Date().toISOString());
}

const navItems = [
  { path: '/categories', label: 'Danh mục' },
  { path: '/vessels',   label: 'Danh sách tàu' },
  { path: '/onboarding', label: 'Onboarding' },
  { path: '/verification-queue', label: 'Xác minh' },
  { path: '/compliance', label: 'Tuân thủ' },
  { path: '/assignments', label: 'Phân công' },
  { path: '/external-requests', label: 'Tuyển ngoài' },
  { path: '/travel', label: 'Di chuyển' },
  { path: '/onboard-events', label: 'Onboard' },
  { path: '/report', label: 'Báo cáo' },
  { path: '/sync',      label: 'Đồng bộ' },
];

export const TopNavLayout: React.FC = () => {
  const navigate = useNavigate();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [notifications, setNotifications] = useState<HoldNotification[]>([]);
  const [bellOpen, setBellOpen] = useState(false);
  const bellRef = useRef<HTMLDivElement>(null);

  const unreadCount = notifications.filter(
    n => new Date(n.onboardStatusChangedAt) > getLastSeenDate()
  ).length;

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      try {
        const data = await crewApi.holdNotifications();
        if (!cancelled) setNotifications(data);
      } catch { /* silent */ }
    };
    load();
    const interval = setInterval(load, 60_000);
    return () => { cancelled = true; clearInterval(interval); };
  }, []);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (bellRef.current && !bellRef.current.contains(e.target as Node)) {
        setBellOpen(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const handleBellClick = () => {
    setBellOpen(prev => !prev);
    if (!bellOpen) {
      markAllSeen();
      setNotifications(prev => [...prev]); // force re-render to clear badge
    }
  };

  const handleNotificationClick = (n: HoldNotification) => {
    setBellOpen(false);
    navigate(`/vessels/${n.vesselId}`);
  };

  return (
    <div className="app-shell">
      {/* ===== Top Navigation ===== */}
      <header className="topnav">
        <div className="topnav-inner">
          {/* Brand */}
          <NavLink to="/crew" className="topnav-brand">
            <Anchor size={15} />
            <span className="brand-text">Maritime</span>
          </NavLink>

          {/* Desktop Nav Links */}
          <nav className="topnav-links">
            {navItems.map((item) => (
              <NavLink
                key={item.path}
                to={item.path}
                className={({ isActive }) =>
                  `topnav-link ${isActive ? 'topnav-link--active' : ''}`
                }
              >
                {item.label}
              </NavLink>
            ))}
          </nav>

          {/* Right side actions */}
          <div className="topnav-actions">
            {/* Notification Bell */}
            <div ref={bellRef} style={{ position: 'relative' }}>
              <button
                className="topnav-icon-btn"
                title="Thông báo"
                onClick={handleBellClick}
                style={{ position: 'relative' }}
              >
                <Bell size={15} />
                {unreadCount > 0 && (
                  <span style={{
                    position: 'absolute', top: 0, right: 0,
                    background: '#ef4444', color: '#fff',
                    borderRadius: '50%', width: 16, height: 16,
                    fontSize: 10, fontWeight: 700,
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                    lineHeight: 1,
                  }}>{unreadCount > 9 ? '9+' : unreadCount}</span>
                )}
              </button>

              {/* Dropdown */}
              {bellOpen && (
                <div style={{
                  position: 'absolute', right: 0, top: 'calc(100% + 8px)',
                  width: 340, background: '#fff',
                  border: '1px solid #e5e7eb', borderRadius: 10,
                  boxShadow: '0 8px 24px rgba(0,0,0,0.12)', zIndex: 200,
                  overflow: 'hidden',
                }}>
                  <div style={{
                    padding: '10px 14px', borderBottom: '1px solid #f3f4f6',
                    fontWeight: 700, fontSize: 13, color: '#1e293b',
                    display: 'flex', justifyContent: 'space-between', alignItems: 'center'
                  }}>
                    <span>Thông báo tàu</span>
                    {notifications.length > 0 && (
                      <span style={{ fontSize: 11, color: '#64748b', fontWeight: 400 }}>
                        {notifications.length} yêu cầu tạm giữ
                      </span>
                    )}
                  </div>
                  {notifications.length === 0 ? (
                    <div style={{ padding: '20px 14px', textAlign: 'center', color: '#94a3b8', fontSize: 13 }}>
                      Không có thông báo mới
                    </div>
                  ) : (
                    <div style={{ maxHeight: 320, overflowY: 'auto' }}>
                      {notifications.map(n => {
                        const isNew = new Date(n.onboardStatusChangedAt) > getLastSeenDate();
                        return (
                          <button
                            key={n.id}
                            onClick={() => handleNotificationClick(n)}
                            style={{
                              display: 'block', width: '100%', textAlign: 'left',
                              padding: '10px 14px', border: 'none', cursor: 'pointer',
                              background: isNew ? '#fff7ed' : '#fff',
                              borderBottom: '1px solid #f3f4f6',
                            }}
                          >
                            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                              {isNew && (
                                <span style={{
                                  width: 8, height: 8, borderRadius: '50%',
                                  background: '#ef4444', flexShrink: 0
                                }} />
                              )}
                              <div style={{ flex: 1 }}>
                                <div style={{ fontWeight: 600, fontSize: 13, color: '#1e293b' }}>
                                  {n.fullName}
                                  <span style={{
                                    marginLeft: 6, background: '#fed7aa', color: '#c2410c',
                                    fontSize: 10, fontWeight: 700, padding: '1px 6px', borderRadius: 6
                                  }}>Tạm giữ</span>
                                </div>
                                <div style={{ fontSize: 12, color: '#64748b', marginTop: 2 }}>
                                  Tàu: <strong>{n.vesselName}</strong>
                                  {n.onboardStatusChangedBy && ` • Bởi: ${n.onboardStatusChangedBy}`}
                                </div>
                                <div style={{ fontSize: 11, color: '#94a3b8', marginTop: 2 }}>
                                  {new Date(n.onboardStatusChangedAt).toLocaleString('vi-VN', {
                                    day: '2-digit', month: '2-digit', year: 'numeric',
                                    hour: '2-digit', minute: '2-digit'
                                  })}
                                </div>
                              </div>
                            </div>
                          </button>
                        );
                      })}
                    </div>
                  )}
                </div>
              )}
            </div>

            <div className="topnav-avatar">A</div>

            {/* Mobile menu toggle */}
            <button
              className="topnav-mobile-toggle"
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              aria-label="Toggle menu"
            >
              {mobileMenuOpen ? <X size={18} /> : <Menu size={18} />}
            </button>
          </div>
        </div>

        {/* Mobile dropdown menu */}
        {mobileMenuOpen && (
          <div className="topnav-mobile-menu">
            {navItems.map((item) => (
              <NavLink
                key={item.path}
                to={item.path}
                className={({ isActive }) =>
                  `topnav-mobile-link ${isActive ? 'topnav-mobile-link--active' : ''}`
                }
                onClick={() => setMobileMenuOpen(false)}
              >
                {item.label}
              </NavLink>
            ))}
          </div>
        )}
      </header>

      {/* ===== Main Content ===== */}
      <main className="main-area">
        <div className="main-area-inner">
          <Outlet />
        </div>
      </main>
    </div>
  );
};
