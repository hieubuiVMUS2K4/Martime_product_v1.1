import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import './Sidebar.css';

export interface MenuItem {
  path: string;
  label: string;
  icon?: string;
}

export interface SidebarProps {
  menuItems: MenuItem[];
}

export const Sidebar: React.FC<SidebarProps> = ({ menuItems }) => {
  const location = useLocation();

  return (
    <div className="sidebar">
      {menuItems.map((item) => (
        <Link
          key={item.path}
          to={item.path}
          className={`menu-item ${location.pathname === item.path ? 'active' : ''}`}
        >
          {item.label}
        </Link>
      ))}
    </div>
  );
};
