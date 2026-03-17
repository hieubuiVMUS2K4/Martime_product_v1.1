import React, { useState, useRef, useEffect } from 'react';
import { Outlet, NavLink, useLocation } from 'react-router-dom';
import { Anchor, Bell, Menu, X, Ship, ChevronDown, Check } from 'lucide-react';
import { useVessel } from '../../../contexts/VesselContext';
import './TopNavLayout.css';

// Simple flat nav items
const navItems = [
  { path: '/crew',               label: 'Thuyền viên' },
  { path: '/vessels',            label: 'Danh sách tàu' },
  { path: '/onboarding',         label: 'Onboarding' },
  { path: '/verification-queue', label: 'Xác minh' },
  { path: '/compliance',         label: 'Tuân thủ' },
  { path: '/assignments',        label: 'Phân công' },
  { path: '/external-requests',  label: 'Tuyển ngoài' },
  { path: '/travel',             label: 'Di chuyển' },
  { path: '/onboard-events',     label: 'Onboard' },
  { path: '/report',             label: 'Báo cáo' },
  { path: '/sync',               label: 'Đồng bộ' },
];

// Dropdown menus with grouped items
const dropdownMenus: {
  label: string;
  basePaths: string[];
  groups: { label: string; items: { path: string; label: string }[] }[];
}[] = [];

export const TopNavLayout: React.FC = () => {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [vesselDropdownOpen, setVesselDropdownOpen] = useState(false);
  const [openMenu, setOpenMenu] = useState<string | null>(null);
  const { vessels, selectedVessel, selectVessel, isLoading } = useVessel();
  const dropdownRef = useRef<HTMLDivElement>(null);
  const navDropdownRefs = useRef<Record<string, HTMLDivElement | null>>({});
  const location = useLocation();

  // Close vessel dropdown on outside click
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setVesselDropdownOpen(false);
      }
      // Close nav dropdowns
      const clickedInsideNav = Object.values(navDropdownRefs.current).some(
        ref => ref && ref.contains(e.target as Node)
      );
      if (!clickedInsideNav) setOpenMenu(null);
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // Close nav dropdown on route change
  useEffect(() => { setOpenMenu(null); setMobileMenuOpen(false); }, [location.pathname]);

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
            {/* Flat items */}
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

            {/* Dropdown menus */}
            {dropdownMenus.map((menu) => {
              const isActive = menu.basePaths.some(p => location.pathname.startsWith(p));
              const isOpen = openMenu === menu.label;
              return (
                <div
                  key={menu.label}
                  className="topnav-dropdown-wrap"
                  ref={el => { navDropdownRefs.current[menu.label] = el; }}
                >
                  <button
                    className={`topnav-link topnav-dropdown-btn ${isActive ? 'topnav-link--active' : ''}`}
                    onClick={() => setOpenMenu(isOpen ? null : menu.label)}
                  >
                    {menu.label}
                    <ChevronDown size={12} className={`nav-chevron ${isOpen ? 'nav-chevron--open' : ''}`} />
                  </button>

                  {isOpen && (
                    <div className="topnav-dropdown-panel">
                      {menu.groups.map((group) => (
                        <div key={group.label} className="topnav-dropdown-group">
                          <div className="topnav-dropdown-group-label">{group.label}</div>
                          {group.items.map((item) => (
                            <NavLink
                              key={item.path}
                              to={item.path}
                              className={({ isActive }) =>
                                `topnav-dropdown-item ${isActive ? 'topnav-dropdown-item--active' : ''}`
                              }
                            >
                              {item.label}
                            </NavLink>
                          ))}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              );
            })}
          </nav>

          {/* Vessel Selector */}
          <div className="vessel-selector" ref={dropdownRef}>
            <button
              className={`vessel-selector-btn ${selectedVessel ? 'vessel-selector-btn--active' : ''}`}
              onClick={() => setVesselDropdownOpen(!vesselDropdownOpen)}
              title="Chọn tàu"
            >
              <Ship size={14} />
              <span className="vessel-selector-label">
                {isLoading ? 'Đang tải...' : selectedVessel ? selectedVessel.name : 'Tất cả tàu'}
              </span>
              <ChevronDown size={12} className={`vessel-chevron ${vesselDropdownOpen ? 'vessel-chevron--open' : ''}`} />
            </button>
            {vesselDropdownOpen && (
              <div className="vessel-dropdown">
                <div className="vessel-dropdown-header">Chọn tàu</div>
                <button
                  className={`vessel-dropdown-item ${!selectedVessel ? 'vessel-dropdown-item--active' : ''}`}
                  onClick={() => { selectVessel(null); setVesselDropdownOpen(false); }}
                >
                  <Ship size={13} />
                  <span>Tất cả tàu (Fleet)</span>
                  {!selectedVessel && <Check size={13} className="vessel-check" />}
                </button>
                <div className="vessel-dropdown-divider" />
                {vessels.map(v => (
                  <button
                    key={v.id}
                    className={`vessel-dropdown-item ${selectedVessel?.id === v.id ? 'vessel-dropdown-item--active' : ''}`}
                    onClick={() => { selectVessel(v.id); setVesselDropdownOpen(false); }}
                  >
                    <Ship size={13} />
                    <div className="vessel-dropdown-info">
                      <span className="vessel-dropdown-name">{v.name}</span>
                      <span className="vessel-dropdown-imo">IMO: {v.imo}</span>
                    </div>
                    {selectedVessel?.id === v.id && <Check size={13} className="vessel-check" />}
                  </button>
                ))}
              </div>
            )}
          </div>

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
            {dropdownMenus.flatMap(menu =>
              menu.groups.flatMap(group =>
                group.items.map(item => (
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
                ))
              )
            )}
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