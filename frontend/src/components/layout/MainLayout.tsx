import React from 'react';
import { Outlet } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import type { MenuItem } from './Sidebar';
import './MainLayout.css';

const menuItems: MenuItem[] = [
  { path: '/dashboard', label: 'Dashboard' },
  { path: '/categories', label: 'QL danh mục' },
  { path: '/vessels', label: 'QL tàu' },
  { path: '/crew', label: 'QL thuyền viên' },
  { path: '/voyages', label: 'QL hải trình' },
  { path: '/work-assignments', label: 'QL phân công công việc' },
];

export const MainLayout: React.FC = () => {
  return (
    <div className="main-layout">
      <Sidebar menuItems={menuItems} />
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
};
