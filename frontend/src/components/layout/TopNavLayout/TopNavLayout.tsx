import React, { useState } from 'react';
import { Outlet, NavLink, useLocation } from 'react-router-dom';
import {
  LayoutDashboard, Users, ShieldCheck, Ship,
  ClipboardList, Menu, X, ChevronRight, Anchor,
  Settings, Bell, RefreshCw
} from 'lucide-react';
import './TopNavLayout.css';

interface NavItem {
  path: string;
  label: string;
  icon: React.ReactNode;
}

const navItems: NavItem[] = [
  { path: '/dashboard', label: 'Dashboard', icon: <LayoutDashboard size={18} /> },
  { path: '/crew', label: 'Thuyền viên', icon: <Users size={18} /> },
  { path: '/certificates', label: 'Chứng chỉ', icon: <ShieldCheck size={18} /> },
  { path: '/vessels', label: 'Tàu', icon: <Ship size={18} /> },
  { path: '/work-assignments', label: 'Phân công', icon: <ClipboardList size={18} /> },
  { path: '/sync', label: 'Đồng bộ', icon: <RefreshCw size={18} /> },
];

function getBreadcrumbs(pathname: string): { label: string; path: string }[] {
  const segments = pathname.split('/').filter(Boolean);
  const crumbs: { label: string; path: string }[] = [];
  
  const labelMap: Record<string, string> = {
    dashboard: 'Dashboard',
    crew: 'Thuyền viên',
    certificates: 'Chứng chỉ',
    vessels: 'Tàu',
    'work-assignments': 'Phân công',
    sync: 'Đồng bộ',
    categories: 'Danh mục',
    pms: 'PMS',
    'master-schedule': 'Master Schedule',
    new: 'Thêm mới',
    detail: 'Chi tiết',
  };

  let pathSoFar = '';
  for (const seg of segments) {
    pathSoFar += `/${seg}`;
    // Skip UUID segments in breadcrumb labels
    const isUuid = /^[0-9a-f]{8}-/.test(seg);
    crumbs.push({
      label: isUuid ? 'Chi tiết' : (labelMap[seg] || seg),
      path: pathSoFar,
    });
  }
  return crumbs;
}

export const TopNavLayout: React.FC = () => {
  const location = useLocation();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const breadcrumbs = getBreadcrumbs(location.pathname);

  return (
    <div className="app-shell">
      {/* ===== Top Navigation ===== */}
      <header className="topnav">
        <div className="topnav-inner">
          {/* Logo / Brand */}
          <NavLink to="/dashboard" className="topnav-brand">
            <div className="brand-icon">
              <Anchor size={20} />
            </div>
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
                {item.icon}
                <span>{item.label}</span>
              </NavLink>
            ))}
          </nav>

          {/* Right side actions */}
          <div className="topnav-actions">
            <button className="topnav-icon-btn" title="Thông báo">
              <Bell size={18} />
            </button>
            <button className="topnav-icon-btn" title="Cài đặt">
              <Settings size={18} />
            </button>
            <div className="topnav-avatar">
              <span>A</span>
            </div>

            {/* Mobile menu toggle */}
            <button
              className="topnav-mobile-toggle"
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              aria-label="Toggle menu"
            >
              {mobileMenuOpen ? <X size={20} /> : <Menu size={20} />}
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
                {item.icon}
                <span>{item.label}</span>
              </NavLink>
            ))}
          </div>
        )}
      </header>

      {/* ===== Breadcrumb ===== */}
      {breadcrumbs.length > 0 && (
        <div className="breadcrumb-bar">
          <div className="breadcrumb-inner">
            {breadcrumbs.map((crumb, i) => (
              <React.Fragment key={crumb.path}>
                {i > 0 && <ChevronRight size={14} className="breadcrumb-sep" />}
                {i === breadcrumbs.length - 1 ? (
                  <span className="breadcrumb-current">{crumb.label}</span>
                ) : (
                  <NavLink to={crumb.path} className="breadcrumb-link">
                    {crumb.label}
                  </NavLink>
                )}
              </React.Fragment>
            ))}
          </div>
        </div>
      )}

      {/* ===== Main Content ===== */}
      <main className="main-area">
        <div className="main-area-inner">
          <Outlet />
        </div>
      </main>
    </div>
  );
};
