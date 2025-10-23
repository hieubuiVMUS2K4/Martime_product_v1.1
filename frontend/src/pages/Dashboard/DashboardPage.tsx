import React from 'react';
import { Card, CardHeader, CardBody } from '../../components/common';
import type { DashboardData } from '../../types/dashboard.types';
import './DashboardPage.css';

const mockData: DashboardData = {
  stats: [
    { title: 'TỔNG SỐ TÀU', value: 24 },
    { title: 'THUYỀN VIÊN', value: 156 },
    { title: 'HẢI TRÌNH', value: 18 },
    { title: 'BÁO CÁO', value: 47 },
  ],
  modules: [
    {
      title: 'Quản trị hệ thống',
      items: ['Quản lý người dùng', 'Đăng nhập', 'Đăng xuất', 'Đổi mật khẩu'],
    },
    {
      title: 'Quản lý danh mục',
      items: [
        'Quản lý loại tàu',
        'Quản lý loại vật tư',
        'Quản lý chức vụ',
        'Quản lý loại báo cáo',
        'Quản lý loại chứng chỉ',
        'Quản lý loại báo trí',
        'Quản lý loại chi phí',
      ],
    },
    {
      title: 'Quản lý thuyền viên',
      items: ['Hồ sơ thuyền viên', 'Hồ sơ tài liệu, chứng chỉ'],
    },
    {
      title: 'Quản lý tàu',
      items: ['Quản lý chi phí', 'Quản lý vật tư', 'Quản lý kế hoạch bảo trì'],
    },
    {
      title: 'Quản lý hành trình',
      items: ['Tối ưu hóa hành trình', 'Nhật ký hành trình'],
    },
    {
      title: 'Thống kê báo cáo',
      items: [
        'Báo cáo chi phí hải trình',
        'Thống kê sự cố phát sinh',
        'Thống kê vật tư, nhiên liệu',
        'Báo cáo hàng ngày',
      ],
    },
  ],
};

export const DashboardPage: React.FC = () => {
  return (
    <div className="dashboard-page">
      <div className="page-header">
        <h1>Dashboard</h1>
      </div>

      <div className="stats-grid">
        {mockData.stats.map((stat) => (
          <Card key={stat.title} className="stat-card">
            <h3>{stat.title}</h3>
            <div className="stat-number">{stat.value}</div>
          </Card>
        ))}
      </div>

      <div className="modules-grid">
        {mockData.modules.map((module) => (
          <Card key={module.title} className="module-card">
            <CardHeader>
              <h3>{module.title}</h3>
            </CardHeader>
            <CardBody>
              <ul>
                {module.items.map((item) => (
                  <li key={item}>{item}</li>
                ))}
              </ul>
            </CardBody>
          </Card>
        ))}
      </div>
    </div>
  );
};
