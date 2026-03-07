import React from 'react';

export const DashboardPage: React.FC = () => {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '60vh', gap: '16px' }}>
      <div style={{ fontSize: '48px' }}>🚧</div>
      <h2 style={{ margin: 0, fontSize: '22px', fontWeight: 600, color: '#334155' }}>Đang phát triển</h2>
      <p style={{ margin: 0, color: '#64748b', fontSize: '15px' }}>Tính năng báo cáo sẽ sớm ra mắt.</p>
    </div>
  );
};
