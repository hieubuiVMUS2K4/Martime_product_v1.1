import React, { useState } from 'react';
import { Outlet, NavLink } from 'react-router-dom';
import { Anchor, Bell, Menu, X } from 'lucide-react';
import './TopNavLayout.css';

const navItems = [
  { path: '/crew',      label: 'Thuyền viên' },
  { path: '/vessels',   label: 'Danh sách tàu' },
  { path: '/onboarding', label: 'Onboarding' },
  { path: '/verification-queue', label: 'Xác minh' },
  { path: '/report', label: 'Báo cáo' },
  { path: '/sync',      label: 'Đồng bộ' },
];

export const TopNavLayout: React.FC = () => {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

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
            <button className="topnav-icon-btn" title="Thông báo">
              <Bell size={15} />
            </button>
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
